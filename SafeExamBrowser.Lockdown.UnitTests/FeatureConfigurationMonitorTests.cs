/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown.UnitTests
{
	[TestClass]
	public class FeatureConfigurationMonitorTests
	{
		private Mock<ILogger> logger;
		private FeatureConfigurationMonitor sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			sut = new FeatureConfigurationMonitor(logger.Object, 0);
		}

		[TestMethod]
		public void MustEnforceConfigurations()
		{
			var configuration1 = new Mock<IFeatureConfiguration>();
			var configuration2 = new Mock<IFeatureConfiguration>();
			var configuration3 = new Mock<IFeatureConfiguration>();
			var counter = 0;
			var limit = new Random().Next(5, 50);
			var sync = new AutoResetEvent(false);

			configuration1.Setup(c => c.GetStatus()).Returns(FeatureConfigurationStatus.Disabled);
			configuration2.Setup(c => c.GetStatus()).Returns(FeatureConfigurationStatus.Enabled);
			configuration3.Setup(c => c.GetStatus()).Returns(FeatureConfigurationStatus.Disabled).Callback(() =>
			{
				if (++counter >= limit)
				{
					sync.Set();
				}
			});

			sut = new FeatureConfigurationMonitor(logger.Object, 2);

			sut.Observe(configuration1.Object, FeatureConfigurationStatus.Enabled);
			sut.Observe(configuration2.Object, FeatureConfigurationStatus.Disabled);
			sut.Observe(configuration3.Object, FeatureConfigurationStatus.Undefined);
			sut.Start();
			sync.WaitOne();
			sut.Reset();

			configuration1.Verify(c => c.EnableFeature(), Times.Exactly(limit));
			configuration2.Verify(c => c.DisableFeature(), Times.Exactly(limit));
			configuration3.Verify(c => c.DisableFeature(), Times.Never);
			configuration3.Verify(c => c.EnableFeature(), Times.Never);
		}

		[TestMethod]
		public void MustExecuteAsynchronously()
		{
			var configuration = new Mock<IFeatureConfiguration>();
			var sync = new AutoResetEvent(false);
			var threadId = Thread.CurrentThread.ManagedThreadId;

			configuration.Setup(c => c.GetStatus()).Callback(() => { threadId = Thread.CurrentThread.ManagedThreadId; sync.Set(); });

			sut.Observe(configuration.Object, FeatureConfigurationStatus.Disabled);
			sut.Start();
			sync.WaitOne();
			sut.Reset();

			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId);
		}

		[TestMethod]
		public void MustNotStartMultipleTimes()
		{
			var configuration = new Mock<IFeatureConfiguration>();
			var counter = 0;
			var sync = new AutoResetEvent(false);

			configuration.Setup(c => c.GetStatus()).Returns(() =>
			{
				counter++;
				Thread.Sleep(50);
				sync.Set();
				sut.Reset();

				return FeatureConfigurationStatus.Disabled;
			});

			sut.Observe(configuration.Object, FeatureConfigurationStatus.Enabled);
			sut.Start();
			sut.Start();
			sut.Start();
			sut.Start();
			sut.Start();
			sut.Start();
			sut.Start();
			sync.WaitOne();
			sut.Reset();

			Assert.AreEqual(1, counter);
		}

		[TestMethod]
		public void MustRespectTimeout()
		{
			const int TIMEOUT = 50;

			var after = default(DateTime);
			var before = default(DateTime);
			var configuration = new Mock<IFeatureConfiguration>();
			var counter = 0;
			var sync = new AutoResetEvent(false);

			sut = new FeatureConfigurationMonitor(logger.Object, TIMEOUT);

			configuration.Setup(c => c.GetStatus()).Returns(FeatureConfigurationStatus.Undefined).Callback(() =>
			{
				switch (++counter)
				{
					case 1:
						before = DateTime.Now;
						break;
					case 2:
						after = DateTime.Now;
						sync.Set();
						break;
				}
			});

			sut.Observe(configuration.Object, FeatureConfigurationStatus.Disabled);
			sut.Start();
			sync.WaitOne();
			sut.Reset();

			Assert.IsTrue(after - before >= new TimeSpan(0, 0, 0, 0, TIMEOUT));
		}

		[TestMethod]
		public void MustStopWhenReset()
		{
			var configuration = new Mock<IFeatureConfiguration>();
			var counter = 0;

			configuration.Setup(c => c.GetStatus()).Returns(FeatureConfigurationStatus.Disabled).Callback(() => counter++);

			sut.Observe(configuration.Object, FeatureConfigurationStatus.Enabled);
			sut.Start();
			Thread.Sleep(10);
			sut.Reset();
			Thread.Sleep(10);

			configuration.Verify(c => c.GetStatus(), Times.Exactly(counter));
		}

		[TestMethod]
		public void MustRemoveConfigurationsWhenReset()
		{
			var configuration = new Mock<IFeatureConfiguration>();
			var counter = 0;

			configuration.Setup(c => c.GetStatus()).Returns(FeatureConfigurationStatus.Disabled).Callback(() => counter++);

			sut.Observe(configuration.Object, FeatureConfigurationStatus.Enabled);
			sut.Start();
			Thread.Sleep(10);
			sut.Reset();

			configuration.Verify(c => c.GetStatus(), Times.Exactly(counter));
			configuration.Reset();

			sut.Start();
			Thread.Sleep(10);
			sut.Reset();

			configuration.Verify(c => c.GetStatus(), Times.Never);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void MustValidateTimeout()
		{
			new FeatureConfigurationMonitor(logger.Object, new Random().Next(int.MinValue, -1));
		}
	}
}
