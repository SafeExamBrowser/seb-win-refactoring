/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.System;
using SafeExamBrowser.Monitoring.Contracts.System.Events;
using SafeExamBrowser.Monitoring.System.Components;
using SafeExamBrowser.SystemComponents.Contracts.Registry;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.System
{
	public class SystemSentinel : ISystemSentinel
	{
		private readonly Cursors cursors;
		private readonly EaseOfAccess easeOfAccess;
		private readonly StickyKeys stickyKeys;
		private readonly SystemEvents systemEvents;

		public event SentinelEventHandler CursorChanged;
		public event SentinelEventHandler EaseOfAccessChanged;
		public event SentinelEventHandler StickyKeysChanged;
		public event SessionChangedEventHandler SessionChanged;

		public SystemSentinel(ILogger logger, INativeMethods nativeMethods, IRegistry registry)
		{
			cursors = new Cursors(logger, registry);
			easeOfAccess = new EaseOfAccess(logger, registry);
			stickyKeys = new StickyKeys(logger, nativeMethods);
			systemEvents = new SystemEvents(logger);
		}

		public bool DisableStickyKeys()
		{
			return stickyKeys.Disable();
		}

		public bool EnableStickyKeys()
		{
			return stickyKeys.Enable();
		}

		public bool RevertStickyKeys()
		{
			return stickyKeys.Revert();
		}

		public void StartMonitoringCursors()
		{
			cursors.CursorChanged += (args) => CursorChanged?.Invoke(args);
			cursors.StartMonitoring();
		}

		public void StartMonitoringEaseOfAccess()
		{
			easeOfAccess.EaseOfAccessChanged += (args) => EaseOfAccessChanged?.Invoke(args);
			easeOfAccess.StartMonitoring();
		}

		public void StartMonitoringStickyKeys()
		{
			stickyKeys.Changed += (args) => StickyKeysChanged?.Invoke(args);
			stickyKeys.StartMonitoring();
		}

		public void StartMonitoringSystemEvents()
		{
			systemEvents.SessionChanged += () => SessionChanged?.Invoke();
			systemEvents.StartMonitoring();
		}

		public void StopMonitoring()
		{
			cursors.StopMonitoring();
			easeOfAccess.StopMonitoring();
			stickyKeys.StopMonitoring();
			systemEvents.StopMonitoring();
		}

		public bool VerifyCursors()
		{
			return cursors.Verify();
		}

		public bool VerifyEaseOfAccess()
		{
			return easeOfAccess.Verify();
		}
	}
}
