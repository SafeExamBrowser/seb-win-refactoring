/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.UserInterface.Classic.ViewModels
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
			if (content is ILogText)
			{
				AppendLogText(content as ILogText);
			}
			else if (content is ILogMessage)
			{
				AppendLogMessage(content as ILogMessage);
			}
			else
			{
				throw new NotImplementedException($"The default formatter is not yet implemented for log content of type {content.GetType()}!");
			}

			scrollViewer.Dispatcher.Invoke(scrollViewer.ScrollToEnd);
		}

		private void AppendLogText(ILogText logText)
		{
			textBlock.Dispatcher.Invoke(() =>
			{
				var isHeader = logText.Text.StartsWith("/* ");
				var isComment = logText.Text.StartsWith("# ");
				var brush = isHeader || isComment ? Brushes.ForestGreen : textBlock.Foreground;

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
				var date = message.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
				var severity = message.Severity.ToString().ToUpper();
				var threadInfo = $"{message.ThreadInfo.Id}{(message.ThreadInfo.HasName ? ": " + message.ThreadInfo.Name : string.Empty)}";

				var infoRun = new Run($"{date} [{threadInfo}] - ") { Foreground = Brushes.Gray };
				var messageRun = new Run($"{severity}: {message.Message}{Environment.NewLine}") { Foreground = GetBrushFor(message.Severity) };

				textBlock.Inlines.Add(infoRun);
				textBlock.Inlines.Add(messageRun);
			});
		}

		private Brush GetBrushFor(LogLevel severity)
		{
			switch (severity)
			{
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
