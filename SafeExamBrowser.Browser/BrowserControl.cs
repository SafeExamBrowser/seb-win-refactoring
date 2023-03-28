/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using CefSharp;
using SafeExamBrowser.Browser.Wrapper;
using SafeExamBrowser.Browser.Wrapper.Events;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Browser.Data;
using SafeExamBrowser.UserInterface.Contracts.Browser.Events;

namespace SafeExamBrowser.Browser
{
	internal class BrowserControl : IBrowserControl
	{
		private readonly ICefSharpControl control;
		private readonly IDialogHandler dialogHandler;
		private readonly IDisplayHandler displayHandler;
		private readonly IDownloadHandler downloadHandler;
		private readonly IKeyboardHandler keyboardHandler;
		private readonly IRenderProcessMessageHandler renderProcessMessageHandler;
		private readonly IRequestHandler requestHandler;

		public string Address => control.Address;
		public bool CanNavigateBackwards => control.IsBrowserInitialized && control.BrowserCore.CanGoBack;
		public bool CanNavigateForwards => control.IsBrowserInitialized && control.BrowserCore.CanGoForward;
		public object EmbeddableControl => control;

		public event AddressChangedEventHandler AddressChanged;
		public event LoadFailedEventHandler LoadFailed;
		public event LoadingStateChangedEventHandler LoadingStateChanged;
		public event TitleChangedEventHandler TitleChanged;

		public BrowserControl(
			ICefSharpControl control,
			IDialogHandler dialogHandler,
			IDisplayHandler displayHandler,
			IDownloadHandler downloadHandler,
			IKeyboardHandler keyboardHandler,
			IRenderProcessMessageHandler renderProcessMessageHandler,
			IRequestHandler requestHandler)
		{
			this.control = control;
			this.dialogHandler = dialogHandler;
			this.displayHandler = displayHandler;
			this.downloadHandler = downloadHandler;
			this.keyboardHandler = keyboardHandler;
			this.renderProcessMessageHandler = renderProcessMessageHandler;
			this.requestHandler = requestHandler;
		}

		public void Destroy()
		{
			if (!control.IsDisposed)
			{
				control.Dispose(true);
			}
		}

		public void ExecuteJavascript(string javascript, Action<JavascriptResult> callback)
		{
			if ((control as IWebBrowser)?.CanExecuteJavascriptInMainFrame == true)
			{
				control.EvaluateScriptAsync(javascript).ContinueWith(t =>
				{
					callback(new JavascriptResult
					{
						Message = t.Result.Message,
						Result = t.Result.Result,
						Success = t.Result.Success
					});
				});
			}
			else
			{
				Task.Run(() => callback(new JavascriptResult
				{
					Message = "JavaScript can't be executed in main frame!",
					Success = false
				}));
			}
		}

		public void Find(string term, bool isInitial, bool caseSensitive, bool forward = true)
		{
			control.Find(term, forward, caseSensitive, !isInitial);
		}

		public void Initialize()
		{
			control.AddressChanged += (o, e) => AddressChanged?.Invoke(e.Address);
			control.AuthCredentialsRequired += (w, b, o, i, h, p, r, s, c, a) => a.Value = requestHandler.GetAuthCredentials(w, b, o, i, h, p, r, s, c);
			control.BeforeBrowse += (w, b, f, r, u, i, a) => a.Value = requestHandler.OnBeforeBrowse(w, b, f, r, u, i);
			control.BeforeDownload += (w, b, d, c) => downloadHandler.OnBeforeDownload(w, b, d, c);
			control.CanDownload += (w, b, u, r, a) => a.Value = downloadHandler.CanDownload(w, b, u, r);
			control.ContextCreated += (w, b, f) => renderProcessMessageHandler.OnContextCreated(w, b, f);
			control.ContextReleased += (w, b, f) => renderProcessMessageHandler.OnContextReleased(w, b, f);
			control.DownloadUpdated += (w, b, d, c) => downloadHandler.OnDownloadUpdated(w, b, d, c);
			control.FaviconUrlChanged += (w, b, u) => displayHandler.OnFaviconUrlChange(w, b, u);
			control.FileDialogRequested += (w, b, m, t, d, f, c) => dialogHandler.OnFileDialog(w, b, m, t, d, f, c);
			control.FocusedNodeChanged += (w, b, f, n) => renderProcessMessageHandler.OnFocusedNodeChanged(w, b, f, n);
			control.IsBrowserInitializedChanged += Control_IsBrowserInitializedChanged;
			control.KeyEvent += (w, b, t, k, n, m, s) => keyboardHandler.OnKeyEvent(w, b, t, k, n, m, s);
			control.LoadError += (o, e) => LoadFailed?.Invoke((int) e.ErrorCode, e.ErrorText, e.Frame.IsMain, e.FailedUrl);
			control.LoadingProgressChanged += (w, b, p) => displayHandler.OnLoadingProgressChange(w, b, p);
			control.LoadingStateChanged += (o, e) => LoadingStateChanged?.Invoke(e.IsLoading);
			control.OpenUrlFromTab += (w, b, f, u, t, g, a) => a.Value = requestHandler.OnOpenUrlFromTab(w, b, f, u, t, g);
			control.PreKeyEvent += (IWebBrowser w, IBrowser b, KeyType t, int k, int n, CefEventFlags m, bool i, ref bool s, GenericEventArgs a) => a.Value = keyboardHandler.OnPreKeyEvent(w, b, t, k, n, m, i, ref s);
			control.ResourceRequestHandlerRequired += (IWebBrowser w, IBrowser b, IFrame f, IRequest r, bool n, bool d, string i, ref bool h, ResourceRequestEventArgs a) => a.Handler = requestHandler.GetResourceRequestHandler(w, b, f, r, n, d, i, ref h);
			control.TitleChanged += (o, e) => TitleChanged?.Invoke(e.Title);
			control.UncaughtExceptionEvent += (w, b, f, e) => renderProcessMessageHandler.OnUncaughtException(w, b, f, e);
		}

		public void NavigateBackwards()
		{
			control.BrowserCore.GoBack();
		}

		public void NavigateForwards()
		{
			control.BrowserCore.GoForward();
		}

		public void NavigateTo(string address)
		{
			control.Load(address);
		}

		public void ShowDeveloperConsole()
		{
			control.BrowserCore.ShowDevTools();
		}

		public void Reload()
		{
			control.BrowserCore.Reload();
		}

		public void Zoom(double level)
		{
			control.BrowserCore.SetZoomLevel(level);
		}

		private void Control_IsBrowserInitializedChanged(object sender, EventArgs e)
		{
			if (control.IsBrowserInitialized)
			{
				control.BrowserCore.GetHost().SetFocus(true);
			}
		}
	}
}
