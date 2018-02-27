/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using System.Threading;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Communication.Messages;
using SafeExamBrowser.Contracts.Communication.Responses;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Communication
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
	public abstract class BaseHost : ICommunication, ICommunicationHost
	{
		private const int TWO_SECONDS = 2000;
		private readonly object @lock = new object();

		private string address;
		private ILogger logger;
		private ServiceHost host;
		private Thread hostThread;

		protected Guid? CommunicationToken { get; private set; }
		protected ILogger Logger { get; private set; }

		public bool IsRunning
		{
			get
			{
				lock (@lock)
				{
					return host?.State == CommunicationState.Opened;
				}
			}
		}

		public BaseHost(string address, ILogger logger)
		{
			this.address = address;
			this.logger = logger;
		}

		protected abstract bool OnConnect(Guid? token);
		protected abstract void OnDisconnect();
		protected abstract Response OnReceive(Message message);
		protected abstract Response OnReceive(SimpleMessagePurport message);

		public ConnectionResponse Connect(Guid? token = null)
		{
			lock (@lock)
			{
				logger.Debug($"Received connection request with authentication token '{token}'.");

				var response = new ConnectionResponse();
				var connected = OnConnect(token);

				if (connected)
				{
					response.CommunicationToken = CommunicationToken = Guid.NewGuid();
					response.ConnectionEstablished = true;
				}

				logger.Debug($"{(connected ? "Accepted" : "Denied")} connection request.");

				return response;
			}
		}

		public DisconnectionResponse Disconnect(DisconnectionMessage message)
		{
			lock (@lock)
			{
				var response = new DisconnectionResponse();

				logger.Debug($"Received disconnection request with message '{ToString(message)}'.");

				if (IsAuthorized(message?.CommunicationToken))
				{
					OnDisconnect();

					CommunicationToken = null;
					response.ConnectionTerminated = true;
				}

				return response;
			}
		}

		public Response Send(Message message)
		{
			lock (@lock)
			{
				var response = new SimpleResponse(SimpleResponsePurport.Unauthorized) as Response;

				if (IsAuthorized(message?.CommunicationToken))
				{
					switch (message)
					{
						case SimpleMessage simpleMessage when simpleMessage.Purport == SimpleMessagePurport.Ping:
							response = new SimpleResponse(SimpleResponsePurport.Acknowledged);
							break;
						case SimpleMessage simpleMessage:
							response = OnReceive(simpleMessage.Purport);
							break;
						default:
							response = OnReceive(message);
							break;
					}
				}

				logger.Debug($"Received message '{ToString(message)}', sending response '{ToString(response)}'.");

				return response;
			}
		}

		public void Start()
		{
			lock (@lock)
			{
				var exception = default(Exception);
				var startedEvent = new AutoResetEvent(false);

				hostThread = new Thread(() => TryStartHost(startedEvent, out exception));
				hostThread.SetApartmentState(ApartmentState.STA);
				hostThread.IsBackground = true;
				hostThread.Start();

				var success = startedEvent.WaitOne(TWO_SECONDS);

				if (!success)
				{
					throw new CommunicationException($"Failed to start communication host for endpoint '{address}' within {TWO_SECONDS / 1000} seconds!", exception);
				}
			}
		}

		public void Stop()
		{
			lock (@lock)
			{
				var success = TryStopHost(out Exception exception);

				if (success)
				{
					logger.Debug($"Terminated communication host for endpoint '{address}'.");
				}
				else
				{
					throw new CommunicationException($"Failed to terminate communication host for endpoint '{address}'!", exception);
				}
			}
		}

		private bool IsAuthorized(Guid? token)
		{
			return CommunicationToken == token;
		}

		private void TryStartHost(AutoResetEvent startedEvent, out Exception exception)
		{
			exception = null;

			try
			{
				host = new ServiceHost(this);
				host.AddServiceEndpoint(typeof(ICommunication), new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport), address);
				host.Closed += Host_Closed;
				host.Closing += Host_Closing;
				host.Faulted += Host_Faulted;
				host.Opened += Host_Opened;
				host.Opening += Host_Opening;
				host.UnknownMessageReceived += Host_UnknownMessageReceived;
				host.Open();

				logger.Debug($"Successfully started communication host for endpoint '{address}'.");

				startedEvent.Set();
			}
			catch (Exception e)
			{
				exception = e;
			}
		}

		private bool TryStopHost(out Exception exception)
		{
			var success = false;

			exception = null;

			try
			{
				host?.Close();
				success = hostThread.Join(TWO_SECONDS);
			}
			catch (Exception e)
			{
				exception = e;
				success = false;
			}

			return success;
		}

		private void Host_Closed(object sender, EventArgs e)
		{
			logger.Debug("Communication host has been closed.");
		}

		private void Host_Closing(object sender, EventArgs e)
		{
			logger.Debug("Communication host is closing...");
		}

		private void Host_Faulted(object sender, EventArgs e)
		{
			logger.Error("Communication host has faulted!");
		}

		private void Host_Opened(object sender, EventArgs e)
		{
			logger.Debug("Communication host has been opened.");
		}

		private void Host_Opening(object sender, EventArgs e)
		{
			logger.Debug("Communication host is opening...");
		}

		private void Host_UnknownMessageReceived(object sender, UnknownMessageReceivedEventArgs e)
		{
			logger.Warn($"Communication host has received an unknown message: {e?.Message}.");
		}

		private string ToString(Message message)
		{
			return message != null ? message.ToString() : "<null>";
		}

		private string ToString(Response response)
		{
			return response != null ? response.ToString() : "<null>";
		}
	}
}
