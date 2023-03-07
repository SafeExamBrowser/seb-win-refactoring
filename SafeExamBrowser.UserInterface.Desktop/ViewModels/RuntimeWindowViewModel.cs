/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.UserInterface.Desktop.ViewModels
{
	internal class RuntimeWindowViewModel : ProgressIndicatorViewModel
	{
		private Visibility progressBarVisibility;
		private readonly TextBlock textBlock;

		public Visibility ProgressBarVisibility
		{
			get
			{
				return progressBarVisibility;
			}
			set
			{
				progressBarVisibility = value;
				OnPropertyChanged(nameof(ProgressBarVisibility));
			}
		}

		public RuntimeWindowViewModel(TextBlock textBlock)
		{
			this.textBlock = textBlock;
		}

		public void Notify(ILogContent content)
		{
			switch (content)
			{
				case ILogText text:
					AppendLogText(text);
					break;
				case ILogMessage message:
					AppendLogMessage(message);
					break;
				default:
					throw new NotImplementedException($"The runtime window is not yet implemented for log content of type {content.GetType()}!");
			}
		}

		private void AppendLogMessage(ILogMessage message)
		{
			var time = message.DateTime.ToString("HH:mm:ss.fff");
			var severity = message.Severity.ToString().ToUpper();

			var infoRun = new Run($"{time} - ") { Foreground = Brushes.Gray };
			var messageRun = new Run($"{severity}: {message.Message}{Environment.NewLine}") { Foreground = GetBrushFor(message.Severity) };

			textBlock.Inlines.Add(infoRun);
			textBlock.Inlines.Add(messageRun);
		}

		private void AppendLogText(ILogText text)
		{
			textBlock.Inlines.Add(new Run($"{text.Text}{Environment.NewLine}"));
		}

		private Brush GetBrushFor(LogLevel severity)
		{
			switch (severity)
			{
				case LogLevel.Debug:
					return Brushes.Gray;
				case LogLevel.Error:
					return Brushes.Red;
				case LogLevel.Warning:
					return Brushes.Orange;
				default:
					return Brushes.Black;
			}
		}
	}
}
