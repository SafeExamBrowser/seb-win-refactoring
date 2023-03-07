/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using SafeExamBrowser.Browser.Wrapper.Events;
using KeyEventHandler = SafeExamBrowser.Browser.Wrapper.Events.KeyEventHandler;

namespace SafeExamBrowser.Browser.Wrapper
{
	internal interface ICefSharpControl : IChromiumWebBrowserBase, IWinFormsChromiumWebBrowser, IWin32Window, IComponent, ISynchronizeInvoke
	{
		event AuthCredentialsEventHandler AuthCredentialsRequired;
		event BeforeBrowseEventHandler BeforeBrowse;
		event BeforeDownloadEventHandler BeforeDownload;
		event CanDownloadEventHandler CanDownload;
		event ContextCreatedEventHandler ContextCreated;
		event ContextReleasedEventHandler ContextReleased;
		event DownloadUpdatedEventHandler DownloadUpdated;
		event FaviconUrlChangedEventHandler FaviconUrlChanged;
		event FileDialogRequestedEventHandler FileDialogRequested;
		event FocusedNodeChangedEventHandler FocusedNodeChanged;
		event KeyEventHandler KeyEvent;
		event LoadingProgressChangedEventHandler LoadingProgressChanged;
		event OpenUrlFromTabEventHandler OpenUrlFromTab;
		event PreKeyEventHandler PreKeyEvent;
		event ResourceRequestEventHandler ResourceRequestHandlerRequired;
		event UncaughtExceptionEventHandler UncaughtExceptionEvent;

		void Dispose(bool disposing);
		void GetAuthCredentials(IWebBrowser webBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback, GenericEventArgs args);
		void GetResourceRequestHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling, ResourceRequestEventArgs args);
		void Load(string address);
		void OnBeforeBrowse(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect, GenericEventArgs args);
		void OnBeforeDownload(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback);
		void OnCanDownload(IWebBrowser webBrowser, IBrowser browser, string url, string requestMethod, GenericEventArgs args);
		void OnContextCreated(IWebBrowser webBrowser, IBrowser browser, IFrame frame);
		void OnContextReleased(IWebBrowser webBrowser, IBrowser browser, IFrame frame);
		void OnDownloadUpdated(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback);
		void OnFaviconUrlChange(IWebBrowser webBrowser, IBrowser browser, IList<string> urls);
		void OnFileDialog(IWebBrowser webBrowser, IBrowser browser, CefFileDialogMode mode, string title, string defaultFilePath, List<string> acceptFilters, IFileDialogCallback callback);
		void OnFocusedNodeChanged(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IDomNode node);
		void OnKeyEvent(IWebBrowser webBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey);
		void OnLoadingProgressChange(IWebBrowser webBrowser, IBrowser browser, double progress);
		void OnOpenUrlFromTab(IWebBrowser webBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture, GenericEventArgs args);
		void OnPreKeyEvent(IWebBrowser webBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut, GenericEventArgs args);
		void OnUncaughtException(IWebBrowser webBrowser, IBrowser browser, IFrame frame, JavascriptException exception);
	}
}
