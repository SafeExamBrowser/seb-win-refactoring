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
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;
using SafeExamBrowser.SystemComponents.PreExamCheck;
using SafeExamBrowser.SystemComponents.PreExamCheck.Checkers;
using SafeExamBrowser.SystemComponents.PreExamCheck.Reporting;
using OperatingSystem = SafeExamBrowser.SystemComponents.Contracts.OperatingSystem;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class PreExamCheckTests
	{
		[TestMethod]
		public void OSChecker_PassesForWindows10()
		{
			var systemInfoMock = new Mock<ISystemInfo>();
			systemInfoMock.SetupGet(s => s.OperatingSystem).Returns(OperatingSystem.Windows10);
			systemInfoMock.SetupGet(s => s.OperatingSystemInfo).Returns("Windows 10 Pro 64-bit");

			var checker = new OSChecker(systemInfoMock.Object);
			var result = checker.Check();

			Assert.AreEqual(CheckStatus.Passed, result.Status);
			Assert.IsTrue(result.IsCritical);
		}

		[TestMethod]
		public void OSChecker_FailsForUnsupportedOS()
		{
			var systemInfoMock = new Mock<ISystemInfo>();
			systemInfoMock.SetupGet(s => s.OperatingSystem).Returns(OperatingSystem.Windows7);
			systemInfoMock.SetupGet(s => s.OperatingSystemInfo).Returns("Windows 7 Ultimate");

			var checker = new OSChecker(systemInfoMock.Object);
			var result = checker.Check();

			Assert.AreEqual(CheckStatus.Failed, result.Status);
		}

		[TestMethod]
		public void ReportGenerator_GeneratesValidCsv()
		{
			var report = new PreExamCheckReport
			{
				MachineName = "TEST-PC",
				Results = new List<CheckResult>
				{
					new CheckResult
					{
						Category = "RAM",
						Title = "RAM Capacity",
						Status = CheckStatus.Passed,
						ActualValue = "8.00 GB",
						RequiredValue = "≥ 4.00 GB",
						IsCritical = true,
						Message = "RAM capacity meets requirements."
					}
				}
			};

			var generator = new ReportGenerator();
			var csv = generator.GenerateCsv(report);

			Assert.IsTrue(csv.Contains("# Machine Name: TEST-PC"));
			Assert.IsTrue(csv.Contains("RAM,RAM Capacity,Passed,8.00 GB"));
		}

		[TestMethod]
		public void PreExamCheckService_AggregatesResults()
		{
			var systemInfoMock = new Mock<ISystemInfo>();
			systemInfoMock.SetupGet(s => s.Name).Returns("TEST-HOST");

			var checkerMock = new Mock<IChecker>();
			checkerMock.SetupGet(c => c.Name).Returns("Mock Checker");
			checkerMock.Setup(c => c.Check()).Returns(new CheckResult
			{
				Category = "Test",
				Title = "Mock Checker",
				Status = CheckStatus.Passed,
				ActualValue = "OK",
				RequiredValue = "OK",
				IsCritical = true
			});

			var service = new PreExamCheckService(systemInfoMock.Object, new[] { checkerMock.Object });
			var report = service.RunAllChecks();

			Assert.AreEqual("TEST-HOST", report.MachineName);
			Assert.AreEqual(1, report.Results.Count);
			Assert.IsTrue(report.PassedAllCritical);
		}
	}
}
