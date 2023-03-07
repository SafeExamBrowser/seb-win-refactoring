/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown.UnitTests
{
	[TestClass]
	public class AutoRestoreMechanismTests
	{
		private Mock<IFeatureConfigurationBackup> backup;
		private Mock<ILogger> logger;
		private Mock<ISystemConfigurationUpdate> systemConfigurationUpdate;
		private AutoRestoreMechanism sut;

		[TestInitialize]
		public void Initialize()
		{
			backup = new Mock<IFeatureConfigurationBackup>();
			logger = new Mock<ILogger>();
			systemConfigurationUpdate = new Mock<ISystemConfigurationUpdate>();

			sut = new AutoRestoreMechanism(backup.Object, logger.Object, systemConfigurationUpdate.Object, 0);
		}

		[TestMethod]
		public void MustExecuteAsynchronously()
		{
			var sync = new AutoResetEvent(false);
			var threadId = Thread.CurrentThread.ManagedThreadId;

			backup.Setup(b => b.GetAllConfigurations()).Callback(() => { threadId = Thread.CurrentThread.ManagedThreadId; sync.Set(); });

			sut.Start();
			sync.WaitOne();
			sut.Stop();

			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadId);
		}

		[TestMethod]
		public void MustNotStartMultipleTimes()
		{
			var counter = 0;
			var list = new List<IFeatureConfiguration>();
			var sync = new AutoResetEvent(false);

			backup.Setup(b => b.GetAllConfigurations()).Returns(() => { counter++; Thread.Sleep(50); sync.Set(); return list; });

			sut.Start();
			sut.Start();
			sut.Start();
			sut.Start();
			sut.Start();
			sut.Start();
			sut.Start();
			sync.WaitOne();
			sut.Stop();

			Assert.AreEqual(1, counter);
		}

		[TestMethod]
		public void MustNotTerminateUntilAllChangesReverted()
		{
			var configuration = new Mock<IFeatureConfiguration>();
			var counter = 0;
			var limit = new Random().Next(5, 50);
			var list = new List<IFeatureConfiguration> { configuration.Object };
			var sync = new AutoResetEvent(false);

			backup.Setup(b => b.GetAllConfigurations()).Returns(() => new List<IFeatureConfiguration>(list)).Callback(() => counter++);
			backup.Setup(b => b.Delete(It.IsAny<IFeatureConfiguration>())).Callback(() => list.Clear());
			configuration.Setup(c => c.Restore()).Returns(() => counter >= limit);
			systemConfigurationUpdate.Setup(u => u.ExecuteAsync()).Callback(() => sync.Set());

			sut.Start();
			sync.WaitOne();
			sut.Stop();

			backup.Verify(b => b.GetAllConfigurations(), Times.Exactly(limit));
			backup.Verify(b => b.Delete(It.Is<IFeatureConfiguration>(c => c == configuration.Object)), Times.Once);
			configuration.Verify(c => c.Restore(), Times.Exactly(limit));
			systemConfigurationUpdate.Verify(u => u.Execute(), Times.Never);
			systemConfigurationUpdate.Verify(u => u.ExecuteAsync(), Times.Once);
		}

		[TestMethod]
		public void MustRespectTimeout()
		{
			const int TIMEOUT = 50;

			var after = default(DateTime);
			var before = default(DateTime);
			var configuration = new Mock<IFeatureConfiguration>();
			var counter = 0;
			var list = new List<IFeatureConfiguration> { configuration.Object };
			var sync = new AutoResetEvent(false);

			sut = new AutoRestoreMechanism(backup.Object, logger.Object, systemConfigurationUpdate.Object, TIMEOUT);

			backup.Setup(b => b.GetAllConfigurations()).Returns(list).Callback(() =>
			{
				switch (++counter)
				{
					case 1:
						before = DateTime.Now;
						break;
					case 2:
						after = DateTime.Now;
						list.Clear();
						sync.Set();
						break;
				}
			});

			sut.Start();
			sync.WaitOne();
			sut.Stop();

			Assert.IsTrue(after - before >= new TimeSpan(0, 0, 0, 0, TIMEOUT));
		}

		[TestMethod]
		public void MustStop()
		{
			var configuration = new Mock<IFeatureConfiguration>();
			var counter = 0;
			var list = new List<IFeatureConfiguration> { configuration.Object };

			backup.Setup(b => b.GetAllConfigurations()).Returns(list).Callback(() => counter++);

			sut.Start();
			Thread.Sleep(25);
			sut.Stop();

			backup.Verify(b => b.GetAllConfigurations(), Times.Between(counter, counter + 1, Range.Inclusive));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void MustValidateTimeout()
		{
			new AutoRestoreMechanism(backup.Object, logger.Object, systemConfigurationUpdate.Object, new Random().Next(int.MinValue, -1));
		}
	}
}
