/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QRCoder;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Integrity.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Integrity
{
	public class Verificator : IVerificator
	{
		private const int AUTO_REFRESH_COUNT = 4;
		private const int MAX_ACTIVATIONS_PER_MINUTE = 5;

		private static readonly string SESSION = Guid.NewGuid().ToString("N").ToUpper();
		private static readonly TimeSpan WAIT_TIME = TimeSpan.FromMinutes(1);

		private readonly AppConfig appConfig;
		private readonly AutoResetEvent cancel;
		private readonly IIntegrityModule integrityModule;
		private readonly ILogger logger;
		private readonly IMessageBox messageBox;
		private readonly INativeMethods nativeMethods;
		private readonly ISystemInfo systemInfo;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		private int activations;
		private DateTime lastActivation;
		private IVerificatorOverlay overlay;

		public Verificator(
			AppConfig appConfig,
			IIntegrityModule integrityModule,
			ILogger logger,
			IMessageBox messageBox,
			INativeMethods nativeMethods,
			ISystemInfo systemInfo,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.cancel = new AutoResetEvent(false);
			this.integrityModule = integrityModule;
			this.logger = logger;
			this.messageBox = messageBox;
			this.nativeMethods = nativeMethods;
			this.systemInfo = systemInfo;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public void Activate()
		{
			var canActivate = CanActivate();
			var hasInternet = nativeMethods.HasInternetConnection();

			if (canActivate && hasInternet)
			{
				if (overlay == default)
				{
					CreateOverlay();
					GenerateCodes();
				}
				else
				{
					overlay?.BringToForeground();
				}
			}
			else if (!canActivate)
			{
				ShowActivationLimitMessage();
			}
			else
			{
				ShowMissingInternetMessage();
			}
		}

		public void Deactivate()
		{
			if (overlay != default)
			{
				overlay?.Close();
				logger.Info("Deactivated code generation.");
			}
		}

		public void Register(IVerificatorActivator activator)
		{
			activator.Activated += () => Activate();
		}

		private bool CanActivate()
		{
			var activate = false;

			if (DateTime.Now - lastActivation > WAIT_TIME)
			{
				activations = 0;
			}

			if (activations < MAX_ACTIVATIONS_PER_MINUTE)
			{
				activations++;
				lastActivation = DateTime.Now;
				activate = true;
			}

			return activate;
		}

		private void CreateOverlay()
		{
			cancel.Reset();

			overlay = uiFactory.CreateVerificatorOverlay();
			overlay.Closed += () =>
			{
				cancel.Set();
				overlay = default;
			};

			overlay.Show();
			overlay.BringToForeground();
		}

		private void GenerateCodes()
		{
			Task.Run(() =>
			{
				for (var attempt = 0; attempt < AUTO_REFRESH_COUNT; attempt++)
				{
					var payload = GeneratePayload();
					var success = TryGenerateCode(payload);

					if (success)
					{
						logger.Debug($"Successfully generated verificator code ({attempt + 1}/{AUTO_REFRESH_COUNT}).");
					}
					else
					{
						logger.Warn($"Failed to generate verificator code ({attempt + 1}/{AUTO_REFRESH_COUNT})!");
					}

					if (cancel.WaitOne(TimeSpan.FromSeconds(10)))
					{
						break;
					}
				}

				overlay?.Close();
			});

			logger.Info("Activated code generation.");
		}

		private string GeneratePayload()
		{
			var now = DateTime.Now;
			var payload = new JObject
			{
				["country"] = RegionInfo.CurrentRegion.DisplayName,
				["desktopOS"] = "Windows",
				["errorCode"] = 0,
				["localTime"] = now.ToLocalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz"),
				["osVersion"] = systemInfo.OperatingSystemInfo,
				["sebBuild"] = appConfig.ProgramBuildVersion,
				["sebVersion"] = appConfig.ProgramInformationalVersion,
				["session"] = SESSION,
				["utcTime"] = now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
				["warningCode"] = 0
			};

			return payload.ToString();
		}

		private bool TryGenerateCode(string payload)
		{
			var success = integrityModule.TryGenerateVerificatorCode(payload, out var code);

			if (success)
			{
				using (var qrCode = QRCodeGenerator.GenerateQrCode(code, QRCodeGenerator.ECCLevel.Default))
				using (var bitmap = new BitmapByteQRCode(qrCode))
				{
					var bytes = bitmap.GetGraphic(32);

					overlay?.UpdateCode(bytes);
				}
			}

			return success;
		}

		private void ShowActivationLimitMessage()
		{
			var resumeTime = lastActivation.Add(WAIT_TIME).ToString("T");
			var message = text.Get(TextKey.MessageBox_VerificatorActivationLimit).Replace("%%_RESUME_TIME_%%", resumeTime);
			var title = text.Get(TextKey.MessageBox_VerificatorActivationLimitTitle);

			logger.Info($"The activation limit was reached, code generation may resume at {resumeTime}.");
			messageBox.Show(message, title);
		}

		private void ShowMissingInternetMessage()
		{
			logger.Error("Cannot activate code generation due to missing internet connection!");
			messageBox.Show(TextKey.MessageBox_VerificatorMissingInternet, TextKey.MessageBox_VerificatorMissingInternetTitle);
		}
	}
}
