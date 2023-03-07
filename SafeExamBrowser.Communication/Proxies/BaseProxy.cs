/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using System.Timers;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Communication.Proxies
{
	/// <summary>
	/// Base implementation of an <see cref="ICommunicationProxy"/>.
	/// </summary>
	public abstract class BaseProxy : ICommunicationProxy
	{
		private const int ONE_MINUTE = 60000;
		private readonly object @lock = new object();

		private string address;
		private Interlocutor owner;
		private IProxyObject proxy;
		private IProxyObjectFactory factory;
		private Guid? communicationToken;
		private Timer timer;

		protected ILogger Logger { get; }
		public bool IsConnected => communicationToken.HasValue;

		public event CommunicationEventHandler ConnectionLost;

		public BaseProxy(string address, IProxyObjectFactory factory, ILogger logger, Interlocutor owner)
		{
			this.address = address;
			this.factory = factory;
			this.Logger = logger;
			this.owner = owner;
		}

		public virtual bool Connect(Guid? token = null, bool autoPing = true)
		{
			try
			{
				Logger.Debug($"Trying to connect to endpoint '{address}'{(token.HasValue ? $" with authentication token '{token}'" : string.Empty)}...");

				InitializeProxyObject();

				var response = proxy.Connect(token);
				var success = response.ConnectionEstablished;

				communicationToken = response.CommunicationToken;
				Logger.Debug($"Connection was {(success ? "established" : "refused")}.");

				if (success && autoPing)
				{
					StartAutoPing();
				}

				return success;
			}
			catch (EndpointNotFoundException)
			{
				Logger.Warn($"Endpoint '{address}' could not be found!");
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to connect to endpoint '{address}'!", e);
			}

			return false;
		}

		public virtual bool Disconnect()
		{
			try
			{
				if (!IsConnected)
				{
					Logger.Warn($"Cannot disconnect from endpoint '{address}' before being connected!");

					return false;
				}

				StopAutoPing();

				var message = new DisconnectionMessage
				{
					CommunicationToken = communicationToken.Value,
					Interlocutor = owner
				};
				var response = proxy.Disconnect(message);
				var success = response.ConnectionTerminated;

				Logger.Debug($"{(success ? "Disconnected" : "Failed to disconnect")} from '{address}'.");

				if (success)
				{
					communicationToken = null;
				}

				return success;
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to disconnect from endpoint '{address}'!", e);

				return false;
			}
		}

		/// <summary>
		/// Sends the given message, optionally returning a response. If no response is expected, <c>null</c> will be returned.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the given message is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">If no connection has been established yet or the connection is corrupted.</exception>
		protected virtual Response Send(Message message)
		{
			FailIfNull(message);
			FailIfNotConnected();

			message.CommunicationToken = communicationToken.Value;
			Logger.Debug($"Sending message '{ToString(message)}'...");

			var response = proxy.Send(message);

			Logger.Debug($"Received response '{ToString(response)}' for message '{ToString(message)}'.");

			return response;
		}

		/// <summary>
		/// Sends the given purport as <see cref="SimpleMessage"/>.
		/// </summary>
		protected Response Send(SimpleMessagePurport purport)
		{
			return Send(new SimpleMessage(purport));
		}

		/// <summary>
		/// Determines whether the given response is a <see cref="SimpleResponse"/> with purport <see cref="SimpleResponsePurport.Acknowledged"/>.
		/// </summary>
		protected bool IsAcknowledged(Response response)
		{
			return response is SimpleResponse simpleResponse && simpleResponse.Purport == SimpleResponsePurport.Acknowledged;
		}

		/// <summary>
		/// Tests whether the connection to the host is alive by sending a ping message. If the transmission of the message fails or it is
		/// not acknowledged, the <see cref="ConnectionLost"/> event is fired and the auto-ping timer stopped (if it was initialized).
		/// </summary>
		protected void TestConnection()
		{
			try
			{
				var response = Send(SimpleMessagePurport.Ping);

				if (IsAcknowledged(response))
				{
					Logger.Info("Pinged host, connection is alive.");
				}
				else
				{
					Logger.Error($"Host did not acknowledge ping message! Received: {ToString(response)}.");
					timer?.Stop();
					ConnectionLost?.Invoke();
				}
			}
			catch (Exception e)
			{
				Logger.Error("Failed to ping host!", e);
				timer?.Stop();
				ConnectionLost?.Invoke();
			}
		}

		/// <summary>
		/// Retrieves the string representation of the given <see cref="Message"/>, or indicates that a message is <c>null</c>.
		/// </summary>
		protected string ToString(Message message)
		{
			return message != null ? message.ToString() : "<null>";
		}

		/// <summary>
		/// Retrieves the string representation of the given <see cref="Response"/>, or indicates that a response is <c>null</c>.
		/// </summary>
		protected string ToString(Response response)
		{
			return response != null ? response.ToString() : "<null>";
		}

		private void InitializeProxyObject()
		{
			proxy = factory.CreateObject(address);

			proxy.Closed += BaseProxy_Closed;
			proxy.Closing += BaseProxy_Closing;
			proxy.Faulted += BaseProxy_Faulted;
			proxy.Opened += BaseProxy_Opened;
			proxy.Opening += BaseProxy_Opening;
		}

		private void BaseProxy_Closed(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel has been closed.");
		}

		private void BaseProxy_Closing(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel is closing...");
		}

		private void BaseProxy_Faulted(object sender, EventArgs e)
		{
			Logger.Warn("Communication channel has faulted!");
			timer?.Stop();
			ConnectionLost?.Invoke();
		}

		private void BaseProxy_Opened(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel has been opened.");
		}

		private void BaseProxy_Opening(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel is opening...");
		}

		private void FailIfNull(Message message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}
		}

		private void FailIfNotConnected()
		{
			if (!IsConnected)
			{
				throw new InvalidOperationException($"Cannot send message before being connected to endpoint '{address}'!");
			}

			if (proxy == null || proxy.State != CommunicationState.Opened)
			{
				throw new InvalidOperationException($"Tried to send message, but channel was {GetChannelState()}!");
			}
		}

		private string GetChannelState()
		{
			return proxy == null ? "null" : $"in state '{proxy.State}'";
		}

		private void StartAutoPing()
		{
			lock (@lock)
			{
				timer = new Timer(ONE_MINUTE) { AutoReset = true };
				timer.Elapsed += Timer_Elapsed;
				timer.Start();
			}
		}

		private void StopAutoPing()
		{
			lock (@lock)
			{
				timer?.Stop();
			}
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs args)
		{
			lock (@lock)
			{
				if (timer.Enabled)
				{
					TestConnection();
				}
			}
		}
	}
}
