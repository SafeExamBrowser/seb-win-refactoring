/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Documents;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.UserInterface.ViewModels;

namespace SafeExamBrowser.UserInterface
{
	public partial class SplashScreen : Window, ISplashScreen
	{
		private SplashScreenViewModel model = new SplashScreenViewModel();

		public SplashScreen(ISettings settings)
		{
			InitializeComponent();

			StatusTextBlock.DataContext = model;
			ProgressBar.DataContext = model;

			InfoTextBlock.Inlines.Add(new Run($"Version {settings.ProgramVersion}") { FontStyle = FontStyles.Italic });
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new Run(settings.CopyrightInfo) { FontSize = 10 });
		}

		public void Notify(ILogContent content)
		{
			if (content is ILogMessage)
			{
				model.Status = (content as ILogMessage).Message;
			}
		}

		public void SetMaxProgress(int max)
		{
			model.MaxProgress = max;
		}

		public void UpdateProgress(int amount = 1)
		{
			model.CurrentProgress += amount;
		}
	}
}
