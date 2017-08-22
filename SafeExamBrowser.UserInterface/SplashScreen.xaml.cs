/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Documents;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.UserInterface.Windows10.ViewModels;

namespace SafeExamBrowser.UserInterface.Windows10
{
	public partial class SplashScreen : Window, ISplashScreen
	{
		private SplashScreenViewModel model = new SplashScreenViewModel();
		private ISettings settings;
		private IText text;

		public SplashScreen(ISettings settings, IText text)
		{
			this.settings = settings;
			this.text = text;

			InitializeComponent();
			InitializeSplashScreen();
		}

		public void InvokeClose()
		{
			Dispatcher.Invoke(Close);
		}

		public void InvokeShow()
		{
			Dispatcher.Invoke(Show);
		}

		public void Progress(int amount = 1)
		{
			model.CurrentProgress += amount;
		}

		public void Regress(int amount = 1)
		{
			model.CurrentProgress -= amount;
		}

		public void SetIndeterminate()
		{
			model.IsIndeterminate = true;
		}

		public void SetMaxProgress(int max)
		{
			model.MaxProgress = max;
		}

		public void UpdateText(TextKey key, bool showBusyIndication = false)
		{
			model.StopBusyIndication();
			model.Status = text.Get(key);

			if (showBusyIndication)
			{
				model.StartBusyIndication();
			}
		}

		private void InitializeSplashScreen()
		{
			InfoTextBlock.Inlines.Add(new Run($"{text.Get(TextKey.Version)} {settings.ProgramVersion}") { FontStyle = FontStyles.Italic });
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new LineBreak());
			InfoTextBlock.Inlines.Add(new Run(settings.ProgramCopyright) { FontSize = 10 });
			
			StatusTextBlock.DataContext = model;
			ProgressBar.DataContext = model;

			// To prevent the progress bar going from max to min value at startup...
			model.MaxProgress = 1;
		}
	}
}
