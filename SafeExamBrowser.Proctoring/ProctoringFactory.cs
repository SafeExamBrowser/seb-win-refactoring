/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Proctoring.ScreenProctoring;
using SafeExamBrowser.Proctoring.ScreenProctoring.Service;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Proctoring
{
	internal class ProctoringFactory
	{
		private readonly AppConfig appConfig;
		private readonly IApplicationMonitor applicationMonitor;
		private readonly IBrowserApplication browser;
		private readonly IFileSystem fileSystem;
		private readonly IModuleLogger logger;
		private readonly INativeMethods nativeMethods;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		public ProctoringFactory(
			AppConfig appConfig,
			IApplicationMonitor applicationMonitor,
			IBrowserApplication browser,
			IFileSystem fileSystem,
			IModuleLogger logger,
			INativeMethods nativeMethods,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.applicationMonitor = applicationMonitor;
			this.browser = browser;
			this.fileSystem = fileSystem;
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		internal IEnumerable<ProctoringImplementation> CreateAllActive(ProctoringSettings settings)
		{
			var implementations = new List<ProctoringImplementation>();

			if (settings.ScreenProctoring.Enabled)
			{
				var logger = this.logger.CloneFor(nameof(ScreenProctoring));
				var service = new ServiceProxy(logger.CloneFor(nameof(ServiceProxy)));

				implementations.Add(new ScreenProctoringImplementation(appConfig, applicationMonitor, browser, logger, nativeMethods, service, settings, text));
			}

			return implementations;
		}
	}
}
