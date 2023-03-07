/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown.UnitTests
{
	[TestClass]
	public class FeatureConfigurationBackupTests
	{
		private string filePath;
		private Mock<IModuleLogger> logger;
		private FeatureConfigurationBackup sut;

		[TestInitialize]
		public void Initialize()
		{
			filePath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Backup-{Guid.NewGuid()}.bin";
			logger = new Mock<IModuleLogger>();

			sut = new FeatureConfigurationBackup(filePath, logger.Object);
		}

		[TestMethod]
		public void Delete_MustOnlyRemoveGivenData()
		{
			var configuration1 = new FeatureConfigurationStub();
			var configuration2 = new FeatureConfigurationStub();
			var configuration3 = new FeatureConfigurationStub();
			var toDelete = new FeatureConfigurationStub();

			sut.Save(configuration1);
			sut.Save(configuration2);
			sut.Save(toDelete);
			sut.Save(configuration3);
			sut.Delete(toDelete);

			var configurations = sut.GetAllConfigurations();

			Assert.AreEqual(3, configurations.Count);
			Assert.IsFalse(configurations.Any(c => c.Id == toDelete.Id));
			Assert.IsTrue(configurations.Any(c => c.Id == configuration1.Id));
			Assert.IsTrue(configurations.Any(c => c.Id == configuration2.Id));
			Assert.IsTrue(configurations.Any(c => c.Id == configuration3.Id));
		}

		[TestMethod]
		public void Delete_MustDeleteFileIfEmpty()
		{
			var configuration = new FeatureConfigurationStub();

			sut.Save(configuration);
			Assert.IsTrue(File.Exists(filePath));

			sut.Delete(configuration);
			Assert.IsFalse(File.Exists(filePath));
		}

		[TestMethod]
		public void Delete_MustNotFailIfDataNotInBackup()
		{
			var configuration = new FeatureConfigurationStub();

			sut.Delete(configuration);

			sut.Save(new FeatureConfigurationStub());
			sut.Save(new FeatureConfigurationStub());
			sut.Save(new FeatureConfigurationStub());

			sut.Delete(configuration);
		}

		[TestMethod]
		public void GetAll_MustReturnAllConfigurationData()
		{
			var configuration1 = new FeatureConfigurationStub { GroupId = Guid.NewGuid() };
			var configuration2 = new FeatureConfigurationStub { GroupId = Guid.Empty };
			var configuration3 = new FeatureConfigurationStub { GroupId = Guid.Empty };
			var configuration4 = new FeatureConfigurationStub { GroupId = Guid.NewGuid() };

			sut.Save(configuration1);
			sut.Save(configuration2);
			sut.Save(configuration3);
			sut.Save(configuration4);

			var configurations = sut.GetAllConfigurations();

			Assert.AreEqual(4, configurations.Count);
			Assert.AreEqual(configurations[0].GroupId, configuration1.GroupId);
			Assert.AreEqual(configurations[1].GroupId, configuration2.GroupId);
			Assert.AreEqual(configurations[2].GroupId, configuration3.GroupId);
			Assert.AreEqual(configurations[3].GroupId, configuration4.GroupId);
			Assert.IsTrue(configurations.Any(c => c.Id == configuration1.Id));
			Assert.IsTrue(configurations.Any(c => c.Id == configuration2.Id));
			Assert.IsTrue(configurations.Any(c => c.Id == configuration3.Id));
			Assert.IsTrue(configurations.Any(c => c.Id == configuration4.Id));
		}

		[TestMethod]
		public void GetAll_MustReturnEmptyListIfNoDataAvailable()
		{
			var configurations = sut.GetAllConfigurations();

			Assert.IsInstanceOfType(configurations, typeof(IList<IFeatureConfiguration>));
			Assert.AreEqual(0, configurations.Count);
		}

		[TestMethod]
		public void GetBy_MustOnlyReturnConfigurationDataBelongingToGroup()
		{
			var groupId = Guid.NewGuid();
			var configuration1 = new FeatureConfigurationStub { GroupId = groupId };
			var configuration2 = new FeatureConfigurationStub { GroupId = Guid.NewGuid() };
			var configuration3 = new FeatureConfigurationStub { GroupId = groupId };
			var configuration4 = new FeatureConfigurationStub { GroupId = Guid.NewGuid() };

			sut.Save(configuration1);
			sut.Save(configuration2);
			sut.Save(configuration3);
			sut.Save(configuration4);

			var configurations = sut.GetBy(groupId);

			Assert.AreEqual(2, configurations.Count);
			Assert.AreEqual(configurations[0].GroupId, groupId);
			Assert.AreEqual(configurations[1].GroupId, groupId);
			Assert.IsTrue(configurations.Any(c => c.Id == configuration1.Id));
			Assert.IsFalse(configurations.Any(c => c.Id == configuration2.Id));
			Assert.IsTrue(configurations.Any(c => c.Id == configuration3.Id));
			Assert.IsFalse(configurations.Any(c => c.Id == configuration4.Id));
		}

		[TestMethod]
		public void GetBy_MustReturnEmptyListIfNoDataAvailable()
		{
			var configurations = sut.GetBy(Guid.NewGuid());

			Assert.IsInstanceOfType(configurations, typeof(IList<IFeatureConfiguration>));
			Assert.AreEqual(0, configurations.Count);
		}

		[TestMethod]
		public void Save_MustSaveGivenData()
		{
			var configuration = new FeatureConfigurationStub();

			sut.Save(configuration);

			Assert.IsTrue(File.Exists(filePath));

			using (var stream = File.Open(filePath, FileMode.Open))
			{
				Assert.IsInstanceOfType(new BinaryFormatter().Deserialize(stream), typeof(IList<IFeatureConfiguration>));
			}
		}

		[TestMethod]
		public void Save_MustNotOverwriteExistingData()
		{
			var configuration1 = new FeatureConfigurationStub();
			var configuration2 = new FeatureConfigurationStub();
			var configuration3 = new FeatureConfigurationStub();
			var configuration4 = new FeatureConfigurationStub();
			var configuration5 = new FeatureConfigurationStub();

			sut.Save(configuration1);
			sut.Save(configuration2);
			sut.Save(configuration3);

			Assert.AreEqual(3, sut.GetAllConfigurations().Count);

			sut.Save(configuration4);
			sut.Save(configuration5);

			Assert.AreEqual(5, sut.GetAllConfigurations().Count);
		}

		[TestCleanup]
		public void Cleanup()
		{
			File.Delete(filePath);
		}
	}
}
