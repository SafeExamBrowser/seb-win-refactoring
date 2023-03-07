/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Communication.Hosts
{
	/// <summary>
	/// The base implementation of an <see cref="ICommunicationHost"/>. Runs the host on a new, separate thread.
	/// </summary>
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
	public abstract class BaseHost : ICommunication, ICommunicationHost
	{
		private readonly object @lock = new object();

		private string address;
		private IHostObject host;
		private IHostObjectFactory factory;
		private Thread hostThread;
		private int timeout_ms;

		protected IList<Guid> CommunicationToken { get; private set; }
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

		public BaseHost(string address, IHostObjectFactory factory, ILogger logger, int timeout_ms)
		{
			this.address = address;
			this.CommunicationToken = new List<Guid>();
			this.factory = factory;
			this.Logger = logger;
			this.timeout_ms = timeout_ms;
		}

		protected abstract bool OnConnect(Guid? token);
		protected abstract void OnDisconnect(Interlocutor interlocutor);
		protected abstract Response OnReceive(Message message);
		protected abstract Response OnReceive(SimpleMessagePurport message);

		public ConnectionResponse Connect(Guid? token = null)
		{
			lock (@lock)
			{
				Logger.Debug($"Received connection request {(token.HasValue ? $"with authentication token '{token}'" : "without authentication token")}.");

				var response = new ConnectionResponse();
				var connected = OnConnect(token);

				if (connected)
				{
					var communicationToken = Guid.NewGuid();

					response.CommunicationToken = communicationToken;
					response.ConnectionEstablished = true;

					CommunicationToken.Add(communicationToken);
				}

				Logger.Debug($"{(connected ? "Accepted" : "Denied")} connection request.");

				return response;
			}
		}

		public DisconnectionResponse Disconnect(DisconnectionMessage message)
		{
			lock (@lock)
			{
				var response = new DisconnectionResponse();

				Logger.Debug($"Received disconnection request with message '{ToString(message)}'.");

				if (IsAuthorized(message?.CommunicationToken))
				{
					OnDisconnect(message.Interlocutor);

					response.ConnectionTerminated = true;
					CommunicationToken.Remove(message.CommunicationToken);
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

				Logger.Debug($"Received message '{ToString(message)}', sending response '{ToString(response)}'.");

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

				var success = startedEvent.WaitOne(timeout_ms);

				if (!success)
				{
					throw new CommunicationException($"Failed to start communication host for endpoint '{address}' within {timeout_ms / 1000} seconds!", exception);
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
					Logger.Debug($"Terminated communication host for endpoint '{address}'.");
				}
				else if (exception != null)
				{
					throw new CommunicationException($"Failed to terminate communication host for endpoint '{address}'!", exception);
				}
			}
		}

		private bool IsAuthorized(Guid? token)
		{
			return token.HasValue && CommunicationToken.Contains(token.Value);
		}

		private void TryStartHost(AutoResetEvent startedEvent, out Exception exception)
		{
			exception = null;

			try
			{
				host = factory.CreateObject(address, this);

				host.Closed += Host_Closed;
				host.Closing += Host_Closing;
				host.Faulted += Host_Faulted;
				host.Opened += Host_Opened;
				host.Opening += Host_Opening;

				host.Open();
				Logger.Debug($"Successfully started communication host for endpoint '{address}'.");

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
				success = hostThread?.Join(timeout_ms) == true;
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
			Logger.Debug("Communication host has been closed.");
		}

		private void Host_Closing(object sender, EventArgs e)
		{
			Logger.Debug("Communication host is closing...");
		}

		private void Host_Faulted(object sender, EventArgs e)
		{
			Logger.Error("Communication host has faulted!");
		}

		private void Host_Opened(object sender, EventArgs e)
		{
			Logger.Debug("Communication host has been opened.");
		}

		private void Host_Opening(object sender, EventArgs e)
		{
			Logger.Debug("Communication host is opening...");
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
