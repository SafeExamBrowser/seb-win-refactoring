/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Service.Operations;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Service.UnitTests.Operations
{
	[TestClass]
	public class LockdownOperationTests
	{
		private Mock<IFeatureConfigurationBackup> backup;
		private Mock<IFeatureConfigurationFactory> factory;
		private Mock<IFeatureConfigurationMonitor> monitor;
		private Mock<ILogger> logger;
		private AppSettings settings;
		private SessionContext sessionContext;
		private LockdownOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			backup = new Mock<IFeatureConfigurationBackup>();
			factory = new Mock<IFeatureConfigurationFactory>();
			monitor = new Mock<IFeatureConfigurationMonitor>();
			logger = new Mock<ILogger>();
			settings = new AppSettings();
			sessionContext = new SessionContext
			{
				Configuration = new ServiceConfiguration { Settings = settings, UserName = "TestName", UserSid = "S-1-234-TEST" }
			};

			sut = new LockdownOperation(backup.Object, factory.Object, monitor.Object, logger.Object, sessionContext);
		}

		[TestMethod]
		public void Perform_MustSetConfigurationsCorrectly()
		{
			var configuration = new Mock<IFeatureConfiguration>();
			var count = typeof(IFeatureConfigurationFactory).GetMethods().Where(m => m.Name.StartsWith("Create") && m.Name != nameof(IFeatureConfigurationFactory.CreateAll)).Count();

			configuration.SetReturnsDefault(true);
			factory.SetReturnsDefault(configuration.Object);
			settings.Service.DisableChromeNotifications = true;
			settings.Service.DisableEaseOfAccessOptions = true;
			settings.Service.DisableSignout = true;
			settings.Service.SetVmwareConfiguration = true;

			var result = sut.Perform();

			backup.Verify(b => b.Save(It.Is<IFeatureConfiguration>(c => c == configuration.Object)), Times.Exactly(count));
			configuration.Verify(c => c.Initialize(), Times.Exactly(count));
			configuration.Verify(c => c.DisableFeature(), Times.Exactly(3));
			configuration.Verify(c => c.EnableFeature(), Times.Exactly(count - 3));
			monitor.Verify(m => m.Observe(It.Is<IFeatureConfiguration>(c => c == configuration.Object), It.Is<FeatureConfigurationStatus>(s => s == FeatureConfigurationStatus.Disabled)), Times.Exactly(3));
			monitor.Verify(m => m.Observe(It.Is<IFeatureConfiguration>(c => c == configuration.Object), It.Is<FeatureConfigurationStatus>(s => s == FeatureConfigurationStatus.Enabled)), Times.Exactly(count - 3));
			monitor.Verify(m => m.Start(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Perform_MustOnlySetVmwareConfigurationIfEnabled()
		{
			var configuration = new Mock<IFeatureConfiguration>();

			configuration.SetReturnsDefault(true);
			factory.SetReturnsDefault(configuration.Object);
			settings.Service.SetVmwareConfiguration = true;

			sut.Perform();

			factory.Verify(f => f.CreateVmwareOverlayConfiguration(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

			factory.Reset();
			factory.SetReturnsDefault(configuration.Object);
			settings.Service.SetVmwareConfiguration = false;

			sut.Perform();

			factory.Verify(f => f.CreateVmwareOverlayConfiguration(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		[TestMethod]
		public void Perform_MustUseSameGroupForAllConfigurations()
		{
			var configuration = new Mock<IFeatureConfiguration>();
			var groupId = default(Guid);

			configuration.SetReturnsDefault(true);
			factory
				.Setup(f => f.CreateChromeNotificationConfiguration(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(configuration.Object)
				.Callback<Guid, string, string>((id, name, sid) => groupId = id);
			factory.SetReturnsDefault(configuration.Object);
			settings.Service.SetVmwareConfiguration = true;

			sut.Perform();

			factory.Verify(f => f.CreateChangePasswordConfiguration(It.Is<Guid>(id => id == groupId), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			factory.Verify(f => f.CreateChromeNotificationConfiguration(It.Is<Guid>(id => id == groupId), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			factory.Verify(f => f.CreateEaseOfAccessConfiguration(It.Is<Guid>(id => id == groupId)), Times.Once);
			factory.Verify(f => f.CreateFindPrinterConfiguration(It.Is<Guid>(id => id == groupId), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			factory.Verify(f => f.CreateLockWorkstationConfiguration(It.Is<Guid>(id => id == groupId), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			factory.Verify(f => f.CreateMachinePowerOptionsConfiguration(It.Is<Guid>(id => id == groupId)), Times.Once);
			factory.Verify(f => f.CreateNetworkOptionsConfiguration(It.Is<Guid>(id => id == groupId)), Times.Once);
			factory.Verify(f => f.CreateRemoteConnectionConfiguration(It.Is<Guid>(id => id == groupId)), Times.Once);
			factory.Verify(f => f.CreateSignoutConfiguration(It.Is<Guid>(id => id == groupId), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			factory.Verify(f => f.CreateSwitchUserConfiguration(It.Is<Guid>(id => id == groupId)), Times.Once);
			factory.Verify(f => f.CreateTaskManagerConfiguration(It.Is<Guid>(id => id == groupId), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			factory.Verify(f => f.CreateUserPowerOptionsConfiguration(It.Is<Guid>(id => id == groupId), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			factory.Verify(f => f.CreateVmwareOverlayConfiguration(It.Is<Guid>(id => id == groupId), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			factory.Verify(f => f.CreateWindowsUpdateConfiguration(It.Is<Guid>(id => id == groupId)), Times.Once);
		}

		[TestMethod]
		public void Perform_MustImmediatelyAbortOnFailure()
		{
			var configuration = new Mock<IFeatureConfiguration>();
			var count = typeof(IFeatureConfigurationFactory).GetMethods().Where(m => m.Name.StartsWith("Create") && m.Name != nameof(IFeatureConfigurationFactory.CreateAll)).Count();
			var counter = 0;
			var offset = 3;

			configuration.Setup(c => c.EnableFeature()).Returns(() => ++counter < count - offset);
			factory.SetReturnsDefault(configuration.Object);

			var result = sut.Perform();

			backup.Verify(b => b.Save(It.Is<IFeatureConfiguration>(c => c == configuration.Object)), Times.Exactly(count - offset));
			configuration.Verify(c => c.Initialize(), Times.Exactly(count - offset));
			configuration.Verify(c => c.DisableFeature(), Times.Never);
			configuration.Verify(c => c.EnableFeature(), Times.Exactly(count - offset));
			monitor.Verify(m => m.Observe(It.Is<IFeatureConfiguration>(c => c == configuration.Object), It.Is<FeatureConfigurationStatus>(s => s == FeatureConfigurationStatus.Enabled)), Times.Exactly(count - offset - 1));
			monitor.Verify(m => m.Start(), Times.Never);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void Revert_MustRestoreConfigurationsCorrectly()
		{
			var configuration1 = new Mock<IFeatureConfiguration>();
			var configuration2 = new Mock<IFeatureConfiguration>();
			var configuration3 = new Mock<IFeatureConfiguration>();
			var configuration4 = new Mock<IFeatureConfiguration>();
			var configurations = new List<IFeatureConfiguration>
			{
				configuration1.Object,
				configuration2.Object,
				configuration3.Object,
				configuration4.Object
			};

			backup.Setup(b => b.GetBy(It.IsAny<Guid>())).Returns(configurations);
			configuration1.Setup(c => c.Restore()).Returns(true);
			configuration2.Setup(c => c.Restore()).Returns(true);
			configuration3.Setup(c => c.Restore()).Returns(true);
			configuration4.Setup(c => c.Restore()).Returns(true);

			var result = sut.Revert();

			backup.Verify(b => b.Delete(It.Is<IFeatureConfiguration>(c => c == configuration1.Object)), Times.Once);
			backup.Verify(b => b.Delete(It.Is<IFeatureConfiguration>(c => c == configuration2.Object)), Times.Once);
			backup.Verify(b => b.Delete(It.Is<IFeatureConfiguration>(c => c == configuration3.Object)), Times.Once);
			backup.Verify(b => b.Delete(It.Is<IFeatureConfiguration>(c => c == configuration4.Object)), Times.Once);

			configuration1.Verify(c => c.DisableFeature(), Times.Never);
			configuration1.Verify(c => c.EnableFeature(), Times.Never);
			configuration1.Verify(c => c.Initialize(), Times.Never);
			configuration1.Verify(c => c.Restore(), Times.Once);

			configuration2.Verify(c => c.DisableFeature(), Times.Never);
			configuration2.Verify(c => c.EnableFeature(), Times.Never);
			configuration2.Verify(c => c.Initialize(), Times.Never);
			configuration2.Verify(c => c.Restore(), Times.Once);

			configuration3.Verify(c => c.DisableFeature(), Times.Never);
			configuration3.Verify(c => c.EnableFeature(), Times.Never);
			configuration3.Verify(c => c.Initialize(), Times.Never);
			configuration3.Verify(c => c.Restore(), Times.Once);

			configuration4.Verify(c => c.DisableFeature(), Times.Never);
			configuration4.Verify(c => c.EnableFeature(), Times.Never);
			configuration4.Verify(c => c.Initialize(), Times.Never);
			configuration4.Verify(c => c.Restore(), Times.Once);

			monitor.Verify(m => m.Reset(), Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void Revert_MustContinueToRevertOnFailure()
		{
			var configuration1 = new Mock<IFeatureConfiguration>();
			var configuration2 = new Mock<IFeatureConfiguration>();
			var configuration3 = new Mock<IFeatureConfiguration>();
			var configuration4 = new Mock<IFeatureConfiguration>();
			var configurations = new List<IFeatureConfiguration>
			{
				configuration1.Object,
				configuration2.Object,
				configuration3.Object,
				configuration4.Object
			};

			backup.Setup(b => b.GetBy(It.IsAny<Guid>())).Returns(configurations);
			configuration1.Setup(c => c.Restore()).Returns(true);
			configuration2.Setup(c => c.Restore()).Returns(false);
			configuration3.Setup(c => c.Restore()).Returns(false);
			configuration4.Setup(c => c.Restore()).Returns(true);

			var result = sut.Revert();

			backup.Verify(b => b.Delete(It.Is<IFeatureConfiguration>(c => c == configuration1.Object)), Times.Once);
			backup.Verify(b => b.Delete(It.Is<IFeatureConfiguration>(c => c == configuration2.Object)), Times.Never);
			backup.Verify(b => b.Delete(It.Is<IFeatureConfiguration>(c => c == configuration3.Object)), Times.Never);
			backup.Verify(b => b.Delete(It.Is<IFeatureConfiguration>(c => c == configuration4.Object)), Times.Once);

			configuration1.Verify(c => c.DisableFeature(), Times.Never);
			configuration1.Verify(c => c.EnableFeature(), Times.Never);
			configuration1.Verify(c => c.Initialize(), Times.Never);
			configuration1.Verify(c => c.Restore(), Times.Once);

			configuration2.Verify(c => c.DisableFeature(), Times.Never);
			configuration2.Verify(c => c.EnableFeature(), Times.Never);
			configuration2.Verify(c => c.Initialize(), Times.Never);
			configuration2.Verify(c => c.Restore(), Times.Once);

			configuration3.Verify(c => c.DisableFeature(), Times.Never);
			configuration3.Verify(c => c.EnableFeature(), Times.Never);
			configuration3.Verify(c => c.Initialize(), Times.Never);
			configuration3.Verify(c => c.Restore(), Times.Once);

			configuration4.Verify(c => c.DisableFeature(), Times.Never);
			configuration4.Verify(c => c.EnableFeature(), Times.Never);
			configuration4.Verify(c => c.Initialize(), Times.Never);
			configuration4.Verify(c => c.Restore(), Times.Once);

			monitor.Verify(m => m.Reset(), Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}
	}
}
