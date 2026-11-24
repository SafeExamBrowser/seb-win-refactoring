/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Display;
using SafeExamBrowser.Settings.Monitoring;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Client.UnitTests
{
	[TestClass]
	public class DisplayMonitorTests
	{
		[TestMethod]
		[DataRow(15, true, 2, false, true, false)]
		[DataRow(17, true, 2, false, true, false)]
		[DataRow(17, false, 1, true, false, true)]
		[DataRow(16, true, 2, false, true, false)]
		[DataRow(5, true, 2, false, true, false)]
		[DataRow(5, true, 1, false, true, false)]
		[DataRow(5, true, 2, true, true, false)]
		public void ValidateConfiguration_ClassifiesActiveOutputs(int technology, bool active, int limit, bool internalOnly, bool hasProhibitedDisplay, bool allowed)
		{
			var displays = new[]
			{
				new Display { IsActive = true, Technology = VideoOutputTechnology.Internal },
				new Display { IsActive = active, Technology = (VideoOutputTechnology) technology }
			};
			var monitor = new TestDisplayMonitor(displays);
			var settings = new DisplaySettings { AllowedDisplays = limit, InternalDisplayOnly = internalOnly };

			var result = monitor.ValidateConfiguration(settings);

			Assert.AreEqual(hasProhibitedDisplay, result.HasProhibitedDisplay);
			Assert.AreEqual(allowed, result.IsAllowed);
			Assert.AreEqual(1, result.InternalDisplays);
			Assert.AreEqual(active ? 1 : 0, result.ExternalDisplays);
		}

		[TestMethod]
		public void ValidateConfiguration_RejectsMirroredPathWithOnlyInternalMonitorInWmi()
		{
			var displays = new[] { new Display { IsActive = true, Technology = VideoOutputTechnology.Internal } };
			var nativeMethods = Mock.Of<INativeMethods>(m => m.HasProhibitedDisplay());
			var monitor = new TestDisplayMonitor(displays, nativeMethods);

			var result = monitor.ValidateConfiguration(new DisplaySettings { AllowedDisplays = 2, IgnoreError = true });

			Assert.IsTrue(result.HasProhibitedDisplay);
			Assert.IsFalse(result.IsAllowed);
		}

		[TestMethod]
		public void ValidateConfiguration_RejectsSoftwareAdapterEvenWhenMonitorQueryFails()
		{
			var monitor = new TestDisplayMonitor(new Display[0]) { HasSoftwareDisplay = true, MonitorQuerySucceeded = false };

			var result = monitor.ValidateConfiguration(new DisplaySettings { AllowedDisplays = 2, IgnoreError = true });

			Assert.IsTrue(result.HasProhibitedDisplay);
			Assert.IsFalse(result.IsAllowed);
		}

		private sealed class TestDisplayMonitor : DisplayMonitor
		{
			private readonly IList<Display> displays;

			internal TestDisplayMonitor(IList<Display> displays, INativeMethods nativeMethods = null) : base(Mock.Of<ILogger>(), nativeMethods ?? Mock.Of<INativeMethods>(), null)
			{
				this.displays = displays;
			}

			internal bool HasSoftwareDisplay { get; set; }
			internal bool MonitorQuerySucceeded { get; set; } = true;

			internal override bool TryLoadDisplays(out IList<Display> result, out bool? hasSoftwareDisplay)
			{
				result = displays;
				hasSoftwareDisplay = HasSoftwareDisplay;

				return MonitorQuerySucceeded;
			}
		}
	}
}
