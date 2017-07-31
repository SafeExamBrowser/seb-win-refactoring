/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using CefSharp.WinForms;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface;
using IBrowserSettings = SafeExamBrowser.Contracts.Configuration.Settings.IBrowserSettings;

namespace SafeExamBrowser.Browser
{
	class BrowserControl : ChromiumWebBrowser, IBrowserControl
	{
		private AddressChangedHandler addressChanged;
		private IBrowserSettings settings;
		private TitleChangedHandler titleChanged;
		private IText text;

		event AddressChangedHandler IBrowserControl.AddressChanged
		{
			add { addressChanged += value; }
			remove { addressChanged -= value; }
		}

		event TitleChangedHandler IBrowserControl.TitleChanged
		{
			add { titleChanged += value; }
			remove { titleChanged -= value; }
		}

		public BrowserControl(IBrowserSettings settings, IText text) : base(settings.StartUrl)
		{
			this.settings = settings;
			this.text = text;

			Initialize();
		}
		
		public void NavigateBackwards()
		{
			GetBrowser().GoBack();
		}

		public void NavigateForwards()
		{
			GetBrowser().GoForward();
		}

		public void NavigateTo(string address)
		{
			if (!String.IsNullOrWhiteSpace(address) && Uri.IsWellFormedUriString(address, UriKind.RelativeOrAbsolute))
			{
				Load(address);
			}
		}

		public void Reload()
		{
			GetBrowser().Reload();
		}

		private void Initialize()
		{
			AddressChanged += (o, args) => addressChanged?.Invoke(args.Address);
			TitleChanged += (o, args) => titleChanged?.Invoke(args.Title);

			MenuHandler = new BrowserContextMenuHandler(settings, text);
		}
	}
}
