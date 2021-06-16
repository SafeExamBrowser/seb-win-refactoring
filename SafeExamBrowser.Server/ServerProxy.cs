/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
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
using System.Threading;
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
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork;
using Timer = System.Timers.Timer;

namespace SafeExamBrowser.Server
{
	public class ServerProxy : ILogObserver, IServerProxy
	{
		private ApiVersion1 api;
		private AppConfig appConfig;
		private CancellationTokenSource cancellationTokenSource;
		private FileSystem fileSystem;
		private string connectionToken;
		private int currentPowerSupplyValue;
		private int currentWlanValue;
		private string examId;
		private HttpClient httpClient;
		private ConcurrentQueue<string> instructionConfirmations;
		private ILogger logger;
		private ConcurrentQueue<ILogContent> logContent;
		private Parser parser;
		private string oauth2Token;
		private int pingNumber;
		private IPowerSupply powerSupply;
		private ServerSettings settings;
		private Task task;
		private Timer timer;
		private IWirelessAdapter wirelessAdapter;

		public event ProctoringConfigurationReceivedEventHandler ProctoringConfigurationReceived;
		public event ProctoringInstructionReceivedEventHandler ProctoringInstructionReceived;
		public event TerminationRequestedEventHandler TerminationRequested;

		public ServerProxy(
			AppConfig appConfig,
			ILogger logger,
			IPowerSupply powerSupply = default(IPowerSupply),
			IWirelessAdapter wirelessAdapter = default(IWirelessAdapter))
		{
			this.api = new ApiVersion1();
			this.appConfig = appConfig;
			this.cancellationTokenSource = new CancellationTokenSource();
			this.fileSystem = new FileSystem(appConfig, logger);
			this.httpClient = new HttpClient();
			this.instructionConfirmations = new ConcurrentQueue<string>();
			this.logger = logger;
			this.logContent = new ConcurrentQueue<ILogContent>();
			this.parser = new Parser(logger);
			this.powerSupply = powerSupply;
			this.timer = new Timer();
			this.wirelessAdapter = wirelessAdapter;
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

		public ServerResponse<IEnumerable<Exam>> GetAvailableExams(string examId = default(string))
		{
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var content = $"institutionId={settings.Institution}{(examId == default(string) ? "" : $"&examId={examId}")}";
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

			var success = TryExecute(HttpMethod.Get, $"{api.ConfigurationEndpoint}?examId={exam.Id}", out var response, default(string), default(string), authorization, token);
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

		public void Notify(ILogContent content)
		{
			logContent.Enqueue(content);
		}

		public ServerResponse SendSessionIdentifier(string identifier)
		{
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var content = $"examId={examId}&seb_user_session_id={identifier}";
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
			task = new Task(SendLog, cancellationTokenSource.Token);
			task.Start();
			logger.Info("Started sending log items.");

			timer.AutoReset = false;
			timer.Elapsed += Timer_Elapsed;
			timer.Interval = 1000;
			timer.Start();
			logger.Info("Started sending pings.");

			if (powerSupply != default(IPowerSupply) && wirelessAdapter != default(IWirelessAdapter))
			{
				powerSupply.StatusChanged += PowerSupply_StatusChanged;
				wirelessAdapter.NetworksChanged += WirelessAdapter_NetworksChanged;
				logger.Info("Started monitoring system components.");
			}
		}

		public void StopConnectivity()
		{
			if (powerSupply != default(IPowerSupply) && wirelessAdapter != default(IWirelessAdapter))
			{
				powerSupply.StatusChanged -= PowerSupply_StatusChanged;
				wirelessAdapter.NetworksChanged -= WirelessAdapter_NetworksChanged;
				logger.Info("Stopped monitoring system components.");
			}

			logger.Unsubscribe(this);
			cancellationTokenSource.Cancel();
			task?.Wait();
			logger.Info("Stopped sending log items.");

			timer.Stop();
			timer.Elapsed -= Timer_Elapsed;
			logger.Info("Stopped sending pings.");
		}

		private void SendLog()
		{
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var contentType = "application/json;charset=UTF-8";
			var token = ("SEBConnectionToken", connectionToken);

			while (!cancellationTokenSource.IsCancellationRequested)
			{
				try
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
				catch (Exception e)
				{
					logger.Error("Failed to send log!", e);
				}
			}
		}

		private void PowerSupply_StatusChanged(IPowerSupplyStatus status)
		{
			try
			{
				var value = Convert.ToInt32(status.BatteryCharge * 100);

				if (value != currentPowerSupplyValue)
				{
					var authorization = ("Authorization", $"Bearer {oauth2Token}");
					var chargeInfo = $"{status.BatteryChargeStatus} at {value}%";
					var contentType = "application/json;charset=UTF-8";
					var gridInfo = $"{(status.IsOnline ? "connected to" : "disconnected from")} the power grid";
					var token = ("SEBConnectionToken", connectionToken);
					var json = new JObject
					{
						["type"] = LogLevel.Info.ToLogType(),
						["timestamp"] = DateTime.Now.ToUnixTimestamp(),
						["text"] = $"<battery> {chargeInfo}, {status.BatteryTimeRemaining} remaining, {gridInfo}",
						["numericValue"] = value
					};
					var content = json.ToString();

					TryExecute(HttpMethod.Post, api.LogEndpoint, out var response, content, contentType, authorization, token);
					currentPowerSupplyValue = value;
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to send power supply status!", e);
			}
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs args)
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

						if (instructionConfirmation != default(string))
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

			timer.Start();
		}

		private void WirelessAdapter_NetworksChanged()
		{
			const int NOT_CONNECTED = -1;

			try
			{
				var network = wirelessAdapter.GetNetworks().FirstOrDefault(n => n.Status == WirelessNetworkStatus.Connected);

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
			string content = default(string),
			string contentType = default(string),
			params (string name, string value)[] headers)
		{
			response = default(HttpResponseMessage);

			for (var attempt = 0; attempt < settings.RequestAttempts && (response == default(HttpResponseMessage) || !response.IsSuccessStatusCode); attempt++)
			{
				var request = new HttpRequestMessage(method, url);

				if (content != default(string))
				{
					request.Content = new StringContent(content, Encoding.UTF8);

					if (contentType != default(string))
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

			return response != default(HttpResponseMessage) && response.IsSuccessStatusCode;
		}
	}
}
