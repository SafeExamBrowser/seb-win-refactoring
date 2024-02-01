/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.JitsiMeet;
using SafeExamBrowser.Proctoring.ScreenProctoring;
using SafeExamBrowser.Proctoring.ScreenProctoring.Service;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;

namespace SafeExamBrowser.Proctoring
{
	internal class ProctoringFactory
	{
		private readonly AppConfig appConfig;
		private readonly IFileSystem fileSystem;
		private readonly IModuleLogger logger;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		public ProctoringFactory(AppConfig appConfig, IFileSystem fileSystem, IModuleLogger logger, IText text, IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.fileSystem = fileSystem;
			this.logger = logger;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		internal IEnumerable<ProctoringImplementation> CreateAllActive(ProctoringSettings settings)
		{
			var implementations = new List<ProctoringImplementation>();

			if (settings.JitsiMeet.Enabled)
			{
				var logger = this.logger.CloneFor(nameof(JitsiMeet));

				implementations.Add(new JitsiMeetImplementation(appConfig, fileSystem, logger, settings, text, uiFactory));
			}

			if (settings.ScreenProctoring.Enabled)
			{
				var logger = this.logger.CloneFor(nameof(ScreenProctoring));
				var service = new ServiceProxy(logger.CloneFor(nameof(ServiceProxy)));

				implementations.Add(new ScreenProctoringImplementation(logger, service, settings, text));
			}

			return implementations;
		}
	}
}
