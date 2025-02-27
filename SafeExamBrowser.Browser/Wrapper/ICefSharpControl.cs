/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CefSharp;
using CefSharp.Enums;
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
		event BeforeUnloadDialogEventHandler BeforeUnloadDialog;
		event CanDownloadEventHandler CanDownload;
		event ContextCreatedEventHandler ContextCreated;
		event ContextReleasedEventHandler ContextReleased;
		event DialogClosedEventHandler DialogClosed;
		event DownloadUpdatedEventHandler DownloadUpdated;
		event DragEnterEventHandler DragEnterCefSharp;
		event DraggableRegionsChangedEventHandler DraggableRegionsChanged;
		event FaviconUrlChangedEventHandler FaviconUrlChanged;
		event FileDialogRequestedEventHandler FileDialogRequested;
		event FocusedNodeChangedEventHandler FocusedNodeChanged;
		event GotFocusEventHandler GotFocusCefSharp;
		event JavaScriptDialogEventHandler JavaScriptDialog;
		event KeyEventHandler KeyEvent;
		event LoadingProgressChangedEventHandler LoadingProgressChanged;
		event OpenUrlFromTabEventHandler OpenUrlFromTab;
		event PreKeyEventHandler PreKeyEvent;
		event ResetDialogStateEventHandler ResetDialogState;
		event ResourceRequestEventHandler ResourceRequestHandlerRequired;
		event SetFocusEventHandler SetFocus;
		event TakeFocusEventHandler TakeFocus;
		event UncaughtExceptionEventHandler UncaughtExceptionEvent;

		void Dispose(bool disposing);
		void GetAuthCredentials(IWebBrowser webBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback, GenericEventArgs args);
		void GetResourceRequestHandler(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling, ResourceRequestEventArgs args);
		void Load(string address);
		void OnBeforeBrowse(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect, GenericEventArgs args);
		void OnBeforeDownload(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback, GenericEventArgs args);
		void OnBeforeUnloadDialog(IWebBrowser webBrowser, IBrowser browser, string message, bool isReload, IJsDialogCallback callback, GenericEventArgs args);
		void OnCanDownload(IWebBrowser webBrowser, IBrowser browser, string url, string requestMethod, GenericEventArgs args);
		void OnContextCreated(IWebBrowser webBrowser, IBrowser browser, IFrame frame);
		void OnContextReleased(IWebBrowser webBrowser, IBrowser browser, IFrame frame);
		void OnDialogClosed(IWebBrowser webBrowser, IBrowser browser);
		void OnDownloadUpdated(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback);
		void OnDragEnter(IWebBrowser webBrowser, IBrowser browser, IDragData dragData, DragOperationsMask mask, GenericEventArgs args);
		void OnDraggableRegionsChanged(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IList<DraggableRegion> regions);
		void OnFaviconUrlChange(IWebBrowser webBrowser, IBrowser browser, IList<string> urls);
		void OnFileDialog(IWebBrowser webBrowser, IBrowser browser, CefFileDialogMode mode, string title, string defaultFilePath, IReadOnlyCollection<string> acceptFilters, IReadOnlyCollection<string> acceptExtensions, IReadOnlyCollection<string> acceptDescriptions, IFileDialogCallback callback);
		void OnFocusedNodeChanged(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IDomNode node);
		void OnGotFocus(IWebBrowser webBrowser, IBrowser browser);
		void OnJavaScriptDialog(IWebBrowser webBrowser, IBrowser browser, string originUrl, CefJsDialogType type, string message, string promptText, IJsDialogCallback callback, ref bool suppress, GenericEventArgs args);
		void OnKeyEvent(IWebBrowser webBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey);
		void OnLoadingProgressChange(IWebBrowser webBrowser, IBrowser browser, double progress);
		void OnOpenUrlFromTab(IWebBrowser webBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture, GenericEventArgs args);
		void OnPreKeyEvent(IWebBrowser webBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut, GenericEventArgs args);
		void OnResetDialogState(IWebBrowser webBrowser, IBrowser browser);
		void OnSetFocus(IWebBrowser webBrowser, IBrowser browser, CefFocusSource source, GenericEventArgs args);
		void OnTakeFocus(IWebBrowser webBrowser, IBrowser browser, bool next);
		void OnUncaughtException(IWebBrowser webBrowser, IBrowser browser, IFrame frame, JavascriptException exception);
	}
}
