/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Logging;

namespace SafeExamBrowser.UserInterface.Mobile.ViewModels
{
	internal class LogViewModel : ILogObserver
	{
		private IText text;
		private ScrollViewer scrollViewer;
		private TextBlock textBlock;

		public string WindowTitle => text.Get(TextKey.LogWindow_Title);

		public LogViewModel(IText text, ScrollViewer scrollViewer, TextBlock textBlock)
		{
			this.text = text;
			this.scrollViewer = scrollViewer;
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
					throw new NotImplementedException($"The log window is not yet implemented for log content of type {content.GetType()}!");
			}

			scrollViewer.Dispatcher.Invoke(scrollViewer.ScrollToEnd);
		}

		private void AppendLogText(ILogText logText)
		{
			textBlock.Dispatcher.Invoke(() =>
			{
				var isHeader = logText.Text.StartsWith("/* ");
				var isComment = logText.Text.StartsWith("# ");
				var brush = isHeader || isComment ? Brushes.LimeGreen : textBlock.Foreground;

				textBlock.Inlines.Add(new Run($"{logText.Text}{Environment.NewLine}")
				{
					FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
					Foreground = brush
				});
			});
		}

		private void AppendLogMessage(ILogMessage message)
		{
			textBlock.Dispatcher.Invoke(() =>
			{
				var date = message.DateTime.ToString("HH:mm:ss.fff");
				var severity = message.Severity.ToString().ToUpper();
				var threadId = message.ThreadInfo.Id < 10 ? $"0{message.ThreadInfo.Id}" : message.ThreadInfo.Id.ToString();
				var threadName = message.ThreadInfo.HasName ? ": " + message.ThreadInfo.Name : string.Empty;
				var threadInfo = $"[{threadId}{threadName}]";

				var infoRun = new Run($"{date} {threadInfo} - ") { Foreground = Brushes.DarkGray };
				var messageRun = new Run($"{severity}: {message.Message}{Environment.NewLine}") { Foreground = GetBrushFor(message.Severity) };

				textBlock.Inlines.Add(infoRun);
				textBlock.Inlines.Add(messageRun);
			});
		}

		private Brush GetBrushFor(LogLevel severity)
		{
			switch (severity)
			{
				case LogLevel.Debug:
					return Brushes.DarkGray;
				case LogLevel.Error:
					return Brushes.Red;
				case LogLevel.Warning:
					return Brushes.Orange;
				default:
					return Brushes.White;
			}
		}
	}
}
