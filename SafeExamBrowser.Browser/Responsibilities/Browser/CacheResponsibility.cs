/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using CefSharp;

namespace SafeExamBrowser.Browser.Responsibilities.Browser
{
	internal class CacheResponsibility : BrowserResponsibility
	{
		public CacheResponsibility(BrowserApplicationContext context) : base(context)
		{
		}

		public override void Assume(BrowserTask task)
		{
			switch (task)
			{
				case BrowserTask.InitializeCookies:
					InitializeCookies();
					break;
				case BrowserTask.FinalizeCache:
					FinalizeCache();
					break;
				case BrowserTask.FinalizeCookies:
					FinalizeCookies();
					break;
			}
		}

		private void DeleteCookies()
		{
			var callback = new TaskDeleteCookiesCallback();
			var cookieManager = Cef.GetGlobalCookieManager();

			callback.Task.ContinueWith(task =>
			{
				if (!task.IsCompleted || task.Result == TaskDeleteCookiesCallback.InvalidNoOfCookiesDeleted)
				{
					Logger.Warn("Failed to delete cookies!");
				}
				else
				{
					Logger.Debug($"Deleted {task.Result} cookies.");
				}
			});

			if (cookieManager != default && cookieManager.DeleteCookies(callback: callback))
			{
				Logger.Debug("Successfully initiated cookie deletion.");
			}
			else
			{
				Logger.Warn("Failed to initiate cookie deletion!");
			}
		}

		private void FinalizeCache()
		{
			if (Settings.DeleteCacheOnShutdown && Settings.DeleteCookiesOnShutdown)
			{
				try
				{
					Directory.Delete(AppConfig.BrowserCachePath, true);
					Logger.Info("Deleted browser cache.");
				}
				catch (Exception e)
				{
					Logger.Error("Failed to delete browser cache!", e);
				}
			}
			else
			{
				Logger.Info("Retained browser cache.");
			}
		}

		private void FinalizeCookies()
		{
			if (Settings.DeleteCookiesOnShutdown)
			{
				DeleteCookies();
			}
		}

		private void InitializeCookies()
		{
			if (Settings.DeleteCookiesOnStartup)
			{
				DeleteCookies();
			}
		}

	}
}
