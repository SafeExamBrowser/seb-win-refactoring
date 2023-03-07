/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts.Events;
using SafeExamBrowser.WindowsApi.Delegates;

namespace SafeExamBrowser.WindowsApi.Hooks
{
	internal class SystemHook
	{
		private SystemEventCallback callback;
		private AutoResetEvent detachEvent, detachResultAvailableEvent;
		private bool detachSuccess;
		private EventDelegate eventDelegate;
		private uint eventId;
		private IntPtr handle;

		internal Guid Id { get; private set; }

		public SystemHook(SystemEventCallback callback, uint eventId)
		{
			this.callback = callback;
			this.detachEvent = new AutoResetEvent(false);
			this.detachResultAvailableEvent = new AutoResetEvent(false);
			this.eventId = eventId;
			this.Id = Guid.NewGuid();
		}

		internal void Attach()
		{
			// IMPORTANT:
			// Ensures that the hook delegate does not get garbage collected prematurely, as it will be passed to unmanaged code.
			// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
			eventDelegate = new EventDelegate(LowLevelSystemProc);
			handle = User32.SetWinEventHook(eventId, eventId, IntPtr.Zero, eventDelegate, 0, 0, Constant.WINEVENT_OUTOFCONTEXT);
		}

		internal void AwaitDetach()
		{
			detachEvent.WaitOne();
			detachSuccess = User32.UnhookWinEvent(handle);
			detachResultAvailableEvent.Set();
		}

		internal bool Detach()
		{
			detachEvent.Set();
			detachResultAvailableEvent.WaitOne();

			return detachSuccess;
		}

		private void LowLevelSystemProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			callback(hwnd);
		}
	}
}
