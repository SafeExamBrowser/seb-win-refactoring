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
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.UserInterface.ViewModels;

namespace SafeExamBrowser.UserInterface
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

		public void SetMaxProgress(int max)
		{
			model.MaxProgress = max;
		}

		public void StartBusyIndication()
		{
			model.StartBusyIndication();
		}

		public void StopBusyIndication()
		{
			model.StopBusyIndication();
		}

		public void UpdateProgress(int amount = 1)
		{
			model.CurrentProgress += amount;
		}

		public void UpdateText(Key key)
		{
			model.Status = text.Get(key);
		}

		private void InitializeSplashScreen()
		{
			InfoTextBlock.Inlines.Add(new Run($"{text.Get(Key.Version)} {settings.ProgramVersion}") { FontStyle = FontStyles.Italic });
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
