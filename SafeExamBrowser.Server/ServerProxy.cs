/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Server.Contracts.Events;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Server.Requests;
using SafeExamBrowser.Settings.Server;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using Timer = System.Timers.Timer;

namespace SafeExamBrowser.Server
{
	public class ServerProxy : ILogObserver, IServerProxy
	{
		private readonly AppConfig appConfig;
		private readonly FileSystem fileSystem;
		private readonly ConcurrentQueue<string> instructionConfirmations;
		private readonly ILogger logger;
		private readonly ConcurrentQueue<ILogContent> logContent;
		private readonly Timer logTimer;
		private readonly Parser parser;
		private readonly Timer pingTimer;
		private readonly IPowerSupply powerSupply;
		private readonly ISystemInfo systemInfo;
		private readonly IUserInfo userInfo;
		private readonly INetworkAdapter networkAdapter;

		private ApiVersion1 api;
		private bool connectedToPowergrid;
		private int currentHandId;
		private int currentLockScreenId;
		private int currentPowerSupplyValue;
		private int currentWlanValue;
		private string examId;
		private HttpClient httpClient;
		private int notificationId;
		private int pingNumber;
		private ServerSettings settings;

		public event ServerEventHandler HandConfirmed;
		public event ServerEventHandler LockScreenConfirmed;
		public event ProctoringConfigurationReceivedEventHandler ProctoringConfigurationReceived;
		public event ProctoringInstructionReceivedEventHandler ProctoringInstructionReceived;
		public event TerminationRequestedEventHandler TerminationRequested;
		public event LockScreenRequestedEventHandler LockScreenRequested;

		public ServerProxy(
			AppConfig appConfig,
			ILogger logger,
			ISystemInfo systemInfo,
			IUserInfo userInfo,
			IPowerSupply powerSupply = default,
			INetworkAdapter networkAdapter = default)
		{
			this.api = new ApiVersion1();
			this.appConfig = appConfig;
			this.fileSystem = new FileSystem(appConfig, logger);
			this.instructionConfirmations = new ConcurrentQueue<string>();
			this.logger = logger;
			this.logContent = new ConcurrentQueue<ILogContent>();
			this.logTimer = new Timer();
			this.networkAdapter = networkAdapter;
			this.parser = new Parser(logger);
			this.pingTimer = new Timer();
			this.powerSupply = powerSupply;
			this.systemInfo = systemInfo;
			this.userInfo = userInfo;
		}

		public ServerResponse ConfirmLockScreen()
		{
			var request = new ConfirmLockScreenRequest(api, httpClient, logger, parser, settings);
			var success = request.TryExecute(currentLockScreenId, out var message);

			if (success)
			{
				logger.Info($"Successfully sent notification confirmation for lock screen #{currentLockScreenId}.");
			}
			else
			{
				logger.Error($"Failed to send notification confirmation for lock screen #{currentLockScreenId}!");
			}

			return new ServerResponse(success, message);
		}

		public ServerResponse Connect()
		{
			var request = new ApiRequest(api, httpClient, logger, parser, settings);
			var success = request.TryExecute(out api, out var message);

			if (success)
			{
				logger.Info("Successfully loaded server API.");
				success = new OAuth2TokenRequest(api, httpClient, logger, parser, settings).TryExecute(out message);
			}
			else
			{
				logger.Error("Failed to load server API!");
			}

			return new ServerResponse(success, message);
		}

		public ServerResponse Disconnect()
		{
			var request = new DisconnectionRequest(api, httpClient, logger, parser, settings);
			var success = request.TryExecute(out var message);

			if (success)
			{
				logger.Info("Successfully terminated connection.");
			}
			else
			{
				logger.Error("Failed to terminate connection!");
			}

			return new ServerResponse(success, message);
		}

		public ServerResponse<IEnumerable<Exam>> GetAvailableExams(string examId = default)
		{
			var request = new AvailableExamsRequest(api, appConfig, httpClient, logger, parser, settings, systemInfo, userInfo);
			var success = request.TryExecute(examId, out var exams, out var message);

			if (success)
			{
				logger.Info("Successfully retrieved available exams.");
			}
			else
			{
				logger.Error("Failed to retrieve available exams!");
			}

			return new ServerResponse<IEnumerable<Exam>>(success, exams, message);
		}

		public ServerResponse<Uri> GetConfigurationFor(Exam exam)
		{
			var request = new ExamConfigurationRequest(api, httpClient, logger, parser, settings);
			var success = request.TryExecute(exam, out var content, out var message);
			var uri = default(Uri);

			if (success)
			{
				logger.Info("Successfully retrieved exam configuration.");

				success = fileSystem.TrySaveFile(content, out uri);

				if (success)
				{
					logger.Info($"Successfully saved exam configuration as '{uri}'.");
				}
				else
				{
					logger.Error("Failed to save exam configuration!");
				}
			}
			else
			{
				logger.Error("Failed to retrieve exam configuration!");
			}

			return new ServerResponse<Uri>(success, uri, message);
		}

		public ConnectionInfo GetConnectionInfo()
		{
			return new ConnectionInfo
			{
				Api = JsonConvert.SerializeObject(api),
				ConnectionToken = BaseRequest.ConnectionToken,
				Oauth2Token = BaseRequest.Oauth2Token
			};
		}

		public void Initialize(ServerSettings settings)
		{
			this.settings = settings;

			httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri(settings.ServerUrl);

			if (settings.RequestTimeout > 0)
			{
				httpClient.Timeout = TimeSpan.FromMilliseconds(settings.RequestTimeout);
			}
		}

		public void Initialize(string api, string connectionToken, string examId, string oauth2Token, ServerSettings settings)
		{
			this.api = JsonConvert.DeserializeObject<ApiVersion1>(api);
			this.examId = examId;

			BaseRequest.ConnectionToken = connectionToken;
			BaseRequest.Oauth2Token = oauth2Token;

			Initialize(settings);
		}

		public ServerResponse LockScreen(string text = null)
		{
			var request = new LockScreenRequest(api, httpClient, logger, parser, settings);
			var success = request.TryExecute(currentLockScreenId = ++notificationId, text, out var message);

			if (success)
			{
				logger.Info($"Successfully sent notification for lock screen #{currentLockScreenId}.");
			}
			else
			{
				logger.Error($"Failed to send notification for lock screen #{currentLockScreenId}!");
			}

			return new ServerResponse(success, message);
		}

		public ServerResponse LowerHand()
		{
			var request = new LowerHandRequest(api, httpClient, logger, parser, settings);
			var success = request.TryExecute(currentHandId, out var message);

			if (success)
			{
				logger.Info("Successfully sent lower hand notification.");
			}
			else
			{
				logger.Error("Failed to send lower hand notification!");
			}

			return new ServerResponse(success, message);
		}

		public void Notify(ILogContent content)
		{
			logContent.Enqueue(content);
		}

		public ServerResponse RaiseHand(string text = null)
		{
			var request = new RaiseHandRequest(api, httpClient, logger, parser, settings);
			var success = request.TryExecute(currentHandId = ++notificationId, text, out var message);

			if (success)
			{
				logger.Info("Successfully sent raise hand notification.");
			}
			else
			{
				logger.Error("Failed to send raise hand notification!");
			}

			return new ServerResponse(success, message);
		}

		public ServerResponse SendSelectedExam(Exam exam)
		{
			var request = new SelectExamRequest(api, httpClient, logger, parser, settings);
			var success = request.TryExecute(exam, out var message, out var salt);

			if (success)
			{
				logger.Info("Successfully sent selected exam.");
			}
			else
			{
				logger.Error("Failed to send selected exam!");
			}

			if (success && salt != default)
			{
				logger.Info("App signature key salt detected, performing key exchange...");
				success = TrySendAppSignatureKey(out message);
			}
			else
			{
				logger.Info("No app signature key salt detected, skipping key exchange.");
			}

			return new ServerResponse(success, message);
		}

		public ServerResponse SendSessionIdentifier(string identifier)
		{
			var request = new SessionIdentifierRequest(api, httpClient, logger, parser, settings);
			var success = request.TryExecute(examId, identifier, out var message);

			if (success)
			{
				logger.Info("Successfully sent session identifier.");
			}
			else
			{
				logger.Error("Failed to send session identifier!");
			}

			return new ServerResponse(success, message);
		}

		public void StartConnectivity()
		{
			foreach (var item in logger.GetLog())
			{
				logContent.Enqueue(item);
			}

			logger.Subscribe(this);
			logTimer.AutoReset = false;
			logTimer.Elapsed += LogTimer_Elapsed;
			logTimer.Interval = 500;
			logTimer.Start();
			logger.Info("Started sending log items.");

			pingTimer.AutoReset = false;
			pingTimer.Elapsed += PingTimer_Elapsed;
			pingTimer.Interval = settings.PingInterval;
			pingTimer.Start();
			logger.Info("Started sending pings.");

			if (powerSupply != default && networkAdapter != default)
			{
				powerSupply.StatusChanged += PowerSupply_StatusChanged;
				networkAdapter.Changed += NetworkAdapter_Changed;
				logger.Info("Started monitoring system components.");
			}
		}

		public void StopConnectivity()
		{
			if (powerSupply != default && networkAdapter != default)
			{
				powerSupply.StatusChanged -= PowerSupply_StatusChanged;
				networkAdapter.Changed -= NetworkAdapter_Changed;
				logger.Info("Stopped monitoring system components.");
			}

			logger.Unsubscribe(this);
			logTimer.Stop();
			logTimer.Elapsed -= LogTimer_Elapsed;
			logger.Info("Stopped sending log items.");

			pingTimer.Stop();
			pingTimer.Elapsed -= PingTimer_Elapsed;
			logger.Info("Stopped sending pings.");
		}

		private void LogTimer_Elapsed(object sender, ElapsedEventArgs args)
		{
			try
			{
				while (!logContent.IsEmpty)
				{
					if (logContent.TryDequeue(out var c) && c is ILogMessage message)
					{
						new LogRequest(api, httpClient, logger, parser, settings).TryExecute(message);
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to send log!", e);
			}

			logTimer.Start();
		}

		private void PingTimer_Elapsed(object sender, ElapsedEventArgs args)
		{
			try
			{
				instructionConfirmations.TryDequeue(out var confirmation);

				var request = new PingRequest(api, httpClient, logger, parser, settings);
				var success = request.TryExecute(++pingNumber, out var content, out var message, confirmation);

				if (success)
				{
					if (parser.TryParseInstruction(content, out var attributes, out var instruction, out var instructionConfirmation))
					{
						HandleInstruction(attributes, instruction);

						if (instructionConfirmation != default)
						{
							instructionConfirmations.Enqueue(instructionConfirmation);
						}
					}
				}
				else
				{
					logger.Error($"Failed to send ping: {message}");
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to send ping!", e);
			}

			pingTimer.Start();
		}

		private void PowerSupply_StatusChanged(IPowerSupplyStatus status)
		{
			try
			{
				var connected = status.IsOnline;
				var value = Convert.ToInt32(status.BatteryCharge * 100);
				var text = default(string);

				if (value != currentPowerSupplyValue)
				{
					var chargeInfo = $"{status.BatteryChargeStatus} at {value}%";
					var gridInfo = $"{(status.IsOnline ? "connected to" : "disconnected from")} the power grid";

					currentPowerSupplyValue = value;
					text = $"<battery> {chargeInfo}, {status.BatteryTimeRemaining} remaining, {gridInfo}";
				}
				else if (connected != connectedToPowergrid)
				{
					connectedToPowergrid = connected;
					text = $"<battery> Device has been {(connected ? "connected to" : "disconnected from")} power grid";
				}

				new PowerSupplyRequest(api, httpClient, logger, parser, settings).TryExecute(text, value);
			}
			catch (Exception e)
			{
				logger.Error("Failed to send power supply status!", e);
			}
		}

		private void NetworkAdapter_Changed()
		{
			const int NOT_CONNECTED = -1;

			try
			{
				var network = networkAdapter.GetWirelessNetworks().FirstOrDefault(n => n.Status == ConnectionStatus.Connected);

				if (network?.SignalStrength != currentWlanValue)
				{
					var text = default(string);
					var value = default(int?);

					if (network != default(IWirelessNetwork))
					{
						text = $"<wlan> {network.Name}: {network.Status}, {network.SignalStrength}%";
						value = network.SignalStrength;
					}
					else
					{
						text = "<wlan> not connected";
					}

					new NetworkAdapterRequest(api, httpClient, logger, parser, settings).TryExecute(text, value);
					currentWlanValue = network?.SignalStrength ?? NOT_CONNECTED;
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to send wireless status!", e);
			}
		}

		private void HandleInstruction(Attributes attributes, string instruction)
		{
			switch (instruction)
			{
				case Instructions.LOCK_SCREEN:
					Task.Run(() => LockScreenRequested?.Invoke(attributes.Message));
					break;
				case Instructions.NOTIFICATION_CONFIRM when attributes.Type == AttributeType.LockScreen:
					Task.Run(() => LockScreenConfirmed?.Invoke());
					break;
				case Instructions.NOTIFICATION_CONFIRM when attributes.Type == AttributeType.Hand:
					Task.Run(() => HandConfirmed?.Invoke());
					break;
				case Instructions.PROCTORING:
					Task.Run(() => ProctoringInstructionReceived?.Invoke(attributes.Instruction));
					break;
				case Instructions.PROCTORING_RECONFIGURATION:
					Task.Run(() => ProctoringConfigurationReceived?.Invoke(attributes.AllowChat, attributes.ReceiveAudio, attributes.ReceiveVideo));
					break;
				case Instructions.QUIT:
					Task.Run(() => TerminationRequested?.Invoke());
					break;
			}
		}

		private bool TrySendAppSignatureKey(out string message)
		{
			// TODO:
			// keyGenerator.CalculateAppSignatureKey(configurationKey, server.AppSignatureKeySalt)

			var request = new AppSignatureKeyRequest(api, httpClient, logger, parser, settings);
			var success = request.TryExecute(out message);

			if (success)
			{
				logger.Info("Successfully sent app signature key.");
			}
			else
			{
				logger.Error("Failed to send app signature key!");
			}

			return success;
		}
	}
}
