/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.UserInterface.ViewModels
{
	class LogViewModel : INotifyPropertyChanged, ILogObserver
	{
		private ILogContentFormatter formatter;
		private IText text;
		private StringBuilder builder = new StringBuilder();

		public string Text
		{
			get { return builder.ToString(); }
		}

		public string WindowTitle => text.Get(TextKey.LogWindow_Title);

		public event PropertyChangedEventHandler PropertyChanged;

		public LogViewModel(IList<ILogContent> initial, ILogContentFormatter formatter, IText text)
		{
			this.formatter = formatter;
			this.text = text;

			foreach (var content in initial)
			{
				Notify(content);
			}
		}

		public void Notify(ILogContent content)
		{
			builder.AppendLine(formatter.Format(content));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
		}
	}
}
