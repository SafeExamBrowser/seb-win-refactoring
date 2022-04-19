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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Server.Contracts.Events;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Settings.Logging;
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
		private string connectionToken;
		private int currentPowerSupplyValue;
		private bool connectedToPowergrid;
		private int currentWlanValue;
		private string examId;
		private int handNotificationId;
		private HttpClient httpClient;
		private string oauth2Token;
		private int pingNumber;
		private ServerSettings settings;

		public event ServerEventHandler HandConfirmed;
		public event ProctoringConfigurationReceivedEventHandler ProctoringConfigurationReceived;
		public event ProctoringInstructionReceivedEventHandler ProctoringInstructionReceived;
		public event TerminationRequestedEventHandler TerminationRequested;

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

		public ServerResponse Connect()
		{
			var success = TryExecute(HttpMethod.Get, settings.ApiUrl, out var response);
			var message = response.ToLogString();

			if (success && parser.TryParseApi(response.Content, out api))
			{
				logger.Info("Successfully loaded server API.");

				var secret = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.ClientName}:{settings.ClientSecret}"));
				var authorization = ("Authorization", $"Basic {secret}");
				var content = "grant_type=client_credentials&scope=read write";
				var contentType = "application/x-www-form-urlencoded";

				success = TryExecute(HttpMethod.Post, api.AccessTokenEndpoint, out response, content, contentType, authorization);
				message = response.ToLogString();

				if (success && parser.TryParseOauth2Token(response.Content, out oauth2Token))
				{
					logger.Info("Successfully retrieved OAuth2 token.");
				}
				else
				{
					logger.Error("Failed to retrieve OAuth2 token!");
				}
			}
			else
			{
				logger.Error("Failed to load server API!");
			}

			return new ServerResponse(success, message);
		}

		public ServerResponse Disconnect()
		{
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var content = "delete=true";
			var contentType = "application/x-www-form-urlencoded";
			var token = ("SEBConnectionToken", connectionToken);

			var success = TryExecute(HttpMethod.Delete, api.HandshakeEndpoint, out var response, content, contentType, authorization, token);
			var message = response.ToLogString();

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
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var content = $"institutionId={settings.Institution}{(examId == default ? "" : $"&examId={examId}")}";
			var contentType = "application/x-www-form-urlencoded";
			var exams = default(IList<Exam>);

			var success = TryExecute(HttpMethod.Post, api.HandshakeEndpoint, out var response, content, contentType, authorization);
			var message = response.ToLogString();

			if (success)
			{
				var hasExams = parser.TryParseExams(response.Content, out exams);
				var hasToken = parser.TryParseConnectionToken(response, out connectionToken);

				success = hasExams && hasToken;

				if (success)
				{
					logger.Info("Successfully retrieved connection token and available exams.");
				}
				else if (!hasExams)
				{
					logger.Error("Failed to retrieve available exams!");
				}
				else if (!hasToken)
				{
					logger.Error("Failed to retrieve connection token!");
				}
			}
			else
			{
				logger.Error("Failed to load connection token and available exams!");
			}

			return new ServerResponse<IEnumerable<Exam>>(success, exams, message);
		}

		public ServerResponse<Uri> GetConfigurationFor(Exam exam)
		{
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var token = ("SEBConnectionToken", connectionToken);
			var uri = default(Uri);

			var success = TryExecute(HttpMethod.Get, $"{api.ConfigurationEndpoint}?examId={exam.Id}", out var response, default, default, authorization, token);
			var message = response.ToLogString();

			if (success)
			{
				logger.Info("Successfully retrieved exam configuration.");

				success = fileSystem.TrySaveFile(response.Content, out uri);

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
				ConnectionToken = connectionToken,
				Oauth2Token = oauth2Token
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
			this.connectionToken = connectionToken;
			this.examId = examId;
			this.oauth2Token = oauth2Token;

			Initialize(settings);
		}

		public ServerResponse LowerHand()
		{
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var contentType = "application/json;charset=UTF-8";
			var token = ("SEBConnectionToken", connectionToken);
			var json = new JObject
			{
				["type"] = "NOTIFICATION_CONFIRMED",
				["timestamp"] = DateTime.Now.ToUnixTimestamp(),
				["numericValue"] = handNotificationId,
			};
			var content = json.ToString();
			var success = TryExecute(HttpMethod.Post, api.LogEndpoint, out var response, content, contentType, authorization, token);

			if (success)
			{
				logger.Info("Successfully sent lower hand notification.");
			}
			else
			{
				logger.Error("Failed to send lower hand notification!");
			}

			return new ServerResponse(success, response.ToLogString());
		}

		public void Notify(ILogContent content)
		{
			logContent.Enqueue(content);
		}

		public ServerResponse RaiseHand(string message = null)
		{
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var contentType = "application/json;charset=UTF-8";
			var token = ("SEBConnectionToken", connectionToken);
			var json = new JObject
			{
				["type"] = "NOTIFICATION",
				["timestamp"] = DateTime.Now.ToUnixTimestamp(),
				["numericValue"] = ++handNotificationId,
				["text"] = $"<raisehand> {message}"
			};
			var content = json.ToString();
			var success = TryExecute(HttpMethod.Post, api.LogEndpoint, out var response, content, contentType, authorization, token);

			if (success)
			{
				logger.Info("Successfully sent raise hand notification.");
			}
			else
			{
				logger.Error("Failed to send raise hand notification!");
			}

			return new ServerResponse(success, response.ToLogString());
		}

		public ServerResponse SendSessionIdentifier(string identifier)
		{
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var clientInfo = $"client_id={userInfo.GetUserName()}&seb_machine_name={systemInfo.Name}";
			var versionInfo = $"seb_os_name={systemInfo.OperatingSystemInfo}&seb_version={appConfig.ProgramInformationalVersion}";
			var content = $"examId={examId}&{clientInfo}&{versionInfo}&seb_user_session_id={identifier}";
			var contentType = "application/x-www-form-urlencoded";
			var token = ("SEBConnectionToken", connectionToken);

			var success = TryExecute(HttpMethod.Put, api.HandshakeEndpoint, out var response, content, contentType, authorization, token);
			var message = response.ToLogString();

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
				var authorization = ("Authorization", $"Bearer {oauth2Token}");
				var contentType = "application/json;charset=UTF-8";
				var token = ("SEBConnectionToken", connectionToken);

				while (!logContent.IsEmpty)
				{
					if (logContent.TryDequeue(out var c) && c is ILogMessage message)
					{
						var json = new JObject
						{
							["type"] = message.Severity.ToLogType(),
							["timestamp"] = message.DateTime.ToUnixTimestamp(),
							["text"] = message.Message
						};
						var content = json.ToString();

						TryExecute(HttpMethod.Post, api.LogEndpoint, out var response, content, contentType, authorization, token);
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
				var authorization = ("Authorization", $"Bearer {oauth2Token}");
				var content = $"timestamp={DateTime.Now.ToUnixTimestamp()}&ping-number={++pingNumber}";
				var contentType = "application/x-www-form-urlencoded";
				var token = ("SEBConnectionToken", connectionToken);

				if (instructionConfirmations.TryDequeue(out var confirmation))
				{
					content = $"{content}&instruction-confirm={confirmation}";
				}

				var success = TryExecute(HttpMethod.Post, api.PingEndpoint, out var response, content, contentType, authorization, token);

				if (success)
				{
					if (parser.TryParseInstruction(response.Content, out var attributes, out var instruction, out var instructionConfirmation))
					{
						switch (instruction)
						{
							case Instructions.NOTIFICATION_CONFIRM when attributes.Type == "raisehand" && attributes.Id == handNotificationId:
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

						if (instructionConfirmation != default)
						{
							instructionConfirmations.Enqueue(instructionConfirmation);
						}
					}
				}
				else
				{
					logger.Error($"Failed to send ping: {response.ToLogString()}");
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
				var value = Convert.ToInt32(status.BatteryCharge * 100);
				var connected = status.IsOnline;

				if (value != currentPowerSupplyValue)
				{
					var chargeInfo = $"{status.BatteryChargeStatus} at {value}%";
					var gridInfo = $"{(status.IsOnline ? "connected to" : "disconnected from")} the power grid";
					var text = $"<battery> {chargeInfo}, {status.BatteryTimeRemaining} remaining, {gridInfo}";
					SendPowerSupplyStatus(text, value);
					currentPowerSupplyValue = value;
				}
				else if (connected != connectedToPowergrid)
				{
					var text = $"<battery> Device has been {(connected ? "connected to" : "disconnected from")} power grid";
					SendPowerSupplyStatus(text, value);
					connectedToPowergrid = connected;
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to send power supply status!", e);
			}
		}

		private void SendPowerSupplyStatus(string text, int value)
		{
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var contentType = "application/json;charset=UTF-8";
			var token = ("SEBConnectionToken", connectionToken);
			var json = new JObject
			{
				["type"] = LogLevel.Info.ToLogType(),
				["timestamp"] = DateTime.Now.ToUnixTimestamp(),
				["text"] = text,
				["numericValue"] = value
			};
			var content = json.ToString();

			TryExecute(HttpMethod.Post, api.LogEndpoint, out _, content, contentType, authorization, token);
		}

		private void NetworkAdapter_Changed()
		{
			const int NOT_CONNECTED = -1;

			try
			{
				var network = networkAdapter.GetWirelessNetworks().FirstOrDefault(n => n.Status == ConnectionStatus.Connected);

				if (network?.SignalStrength != currentWlanValue)
				{
					var authorization = ("Authorization", $"Bearer {oauth2Token}");
					var contentType = "application/json;charset=UTF-8";
					var token = ("SEBConnectionToken", connectionToken);
					var json = new JObject { ["type"] = LogLevel.Info.ToLogType(), ["timestamp"] = DateTime.Now.ToUnixTimestamp() };

					if (network != default(IWirelessNetwork))
					{
						json["text"] = $"<wlan> {network.Name}: {network.Status}, {network.SignalStrength}%";
						json["numericValue"] = network.SignalStrength;
					}
					else
					{
						json["text"] = "<wlan> not connected";
					}

					TryExecute(HttpMethod.Post, api.LogEndpoint, out var response, json.ToString(), contentType, authorization, token);

					currentWlanValue = network?.SignalStrength ?? NOT_CONNECTED;
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to send wireless status!", e);
			}
		}

		private bool TryExecute(
			HttpMethod method,
			string url,
			out HttpResponseMessage response,
			string content = default,
			string contentType = default,
			params (string name, string value)[] headers)
		{
			response = default;

			for (var attempt = 0; attempt < settings.RequestAttempts && (response == default || !response.IsSuccessStatusCode); attempt++)
			{
				var request = new HttpRequestMessage(method, url);

				if (content != default)
				{
					request.Content = new StringContent(content, Encoding.UTF8);

					if (contentType != default)
					{
						request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
					}
				}

				foreach (var (name, value) in headers)
				{
					request.Headers.Add(name, value);
				}

				try
				{
					response = httpClient.SendAsync(request).GetAwaiter().GetResult();

					if (request.RequestUri.AbsolutePath != api.LogEndpoint && request.RequestUri.AbsolutePath != api.PingEndpoint)
					{
						logger.Debug($"Completed request: {request.Method} '{request.RequestUri}' -> {response.ToLogString()}");
					}
				}
				catch (TaskCanceledException)
				{
					logger.Debug($"Request {request.Method} '{request.RequestUri}' did not complete within {settings.RequestTimeout}ms!");
					break;
				}
				catch (Exception e)
				{
					logger.Debug($"Request {request.Method} '{request.RequestUri}' failed due to {e}");
				}
			}

			return response != default && response.IsSuccessStatusCode;
		}
	}
}
