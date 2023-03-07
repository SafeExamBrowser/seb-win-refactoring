/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Communication.UnitTests.Proxies
{
	[TestClass]
	public class RuntimeProxyTests
	{
		private Mock<ILogger> logger;
		private Mock<IProxyObjectFactory> proxyObjectFactory;
		private Mock<IProxyObject> proxy;
		private RuntimeProxy sut;

		[TestInitialize]
		public void Initialize()
		{
			var response = new ConnectionResponse
			{
				CommunicationToken = Guid.NewGuid(),
				ConnectionEstablished = true
			};

			logger = new Mock<ILogger>();
			proxyObjectFactory = new Mock<IProxyObjectFactory>();
			proxy = new Mock<IProxyObject>();

			proxy.Setup(p => p.Connect(It.IsAny<Guid>())).Returns(response);
			proxy.Setup(o => o.State).Returns(CommunicationState.Opened);
			proxyObjectFactory.Setup(f => f.CreateObject(It.IsAny<string>())).Returns(proxy.Object);

			sut = new RuntimeProxy("net.pipe://random/address/here", proxyObjectFactory.Object, logger.Object, default(Interlocutor));
			sut.Connect(Guid.NewGuid());
		}

		[TestMethod]
		public void MustCorrectlyRetrieveConfiguration()
		{
			var response = new ConfigurationResponse
			{
				Configuration = new ClientConfiguration()
			};

			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ConfigurationNeeded))).Returns(response);

			var communication = sut.GetConfiguration();
			var configuration = communication.Value.Configuration;

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ConfigurationNeeded)), Times.Once);

			Assert.IsTrue(communication.Success);
			Assert.IsInstanceOfType(configuration, typeof(ClientConfiguration));
		}

		[TestMethod]
		public void MustFailIfConfigurationNotRetrieved()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ConfigurationNeeded))).Returns<Response>(null);

			var communication = sut.GetConfiguration();

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyInformClientReady()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ClientIsReady))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.InformClientReady();

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ClientIsReady)), Times.Once);
		}

		[TestMethod]
		public void MustFailIfClientReadyNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ClientIsReady))).Returns<Response>(null);

			var communication = sut.InformClientReady();

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyRequestReconfiguration()
		{
			var path = "file:///C:/Some/file/url.seb";
			var url = @"https://www.host.abc/someresource.seb";

			proxy.Setup(p => p.Send(It.Is<ReconfigurationMessage>(m => m.ConfigurationPath == path))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.RequestReconfiguration(path, url);

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.Is<ReconfigurationMessage>(m => m.ConfigurationPath == path && m.ResourceUrl == url)), Times.Once);
		}

		[TestMethod]
		public void MustFailIfReconfigurationRequestNotAcknowledged()
		{
			var path = "file:///C:/Some/file/url.seb";
			var url = @"https://www.host.abc/someresource.seb";

			proxy.Setup(p => p.Send(It.Is<ReconfigurationMessage>(m => m.ConfigurationPath == path))).Returns<Response>(null);

			var communication = sut.RequestReconfiguration(path, url);

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyRequestShutdown()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.RequestShutdown))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.RequestShutdown();

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.RequestShutdown)), Times.Once);
		}

		[TestMethod]
		public void MustFailIfShutdownRequestNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.RequestShutdown))).Returns<Response>(null);

			var communication = sut.RequestShutdown();

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlySubmitExamSelection()
		{
			var examId = "abc123";
			var requestId = Guid.NewGuid();

			proxy.Setup(p => p.Send(It.IsAny<ExamSelectionReplyMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.SubmitExamSelectionResult(requestId, true, examId);

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.Is<ExamSelectionReplyMessage>(m => m.SelectedExamId == examId && m.RequestId == requestId && m.Success)), Times.Once); 
		}

		[TestMethod]
		public void MustFailIfExamSelectionTransmissionNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<ExamSelectionReplyMessage>())).Returns<Response>(null);

			var communication = sut.SubmitExamSelectionResult(default(Guid), false);

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlySubmitPassword()
		{
			var password = "blubb";
			var requestId = Guid.NewGuid();

			proxy.Setup(p => p.Send(It.IsAny<PasswordReplyMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.SubmitPassword(requestId, true, password);

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.Is<PasswordReplyMessage>(m => m.Password == password && m.RequestId == requestId && m.Success)), Times.Once);
		}

		[TestMethod]
		public void MustFailIfPasswordTransmissionNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<PasswordReplyMessage>())).Returns<Response>(null);

			var communication = sut.SubmitPassword(default(Guid), false);

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlySubmitMessageBoxResult()
		{
			var result = 1234;
			var requestId = Guid.NewGuid();

			proxy.Setup(p => p.Send(It.IsAny<MessageBoxReplyMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.SubmitMessageBoxResult(requestId, result);

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.Is<MessageBoxReplyMessage>(m => m.Result == result && m.RequestId == requestId)), Times.Once);
		}

		[TestMethod]
		public void MustFailIfMessageBoxResultTransmissionNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<MessageBoxReplyMessage>())).Returns<Response>(null);

			var communication = sut.SubmitMessageBoxResult(default(Guid), default(int));

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlySubmitServerFailureAction()
		{
			var requestId = Guid.NewGuid();

			proxy.Setup(p => p.Send(It.IsAny<ServerFailureActionReplyMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.SubmitServerFailureActionResult(requestId, true, true, false);

			Assert.IsTrue(communication.Success);
			proxy.Verify(p => p.Send(It.Is<ServerFailureActionReplyMessage>(m => m.RequestId == requestId && m.Abort && m.Fallback && !m.Retry)), Times.Once);
		}

		[TestMethod]
		public void MustFailIfServerFailureActionTransmissionNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<ServerFailureActionReplyMessage>())).Returns<Response>(null);

			var communication = sut.SubmitServerFailureActionResult(default(Guid), default(bool), default(bool), default(bool));

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustExecuteOperationsFailsafe()
		{
			proxy.Setup(p => p.Send(It.IsAny<Message>())).Throws<Exception>();

			var client = sut.InformClientReady();
			var configuration = sut.GetConfiguration();
			var examSelection = sut.SubmitExamSelectionResult(default(Guid), default(bool));
			var message = sut.SubmitMessageBoxResult(default(Guid), default(int));
			var password = sut.SubmitPassword(default(Guid), false);
			var reconfiguration = sut.RequestReconfiguration(null, null);
			var serverFailure = sut.SubmitServerFailureActionResult(default(Guid), default(bool), default(bool), default(bool));
			var shutdown = sut.RequestShutdown();

			Assert.IsFalse(client.Success);
			Assert.IsFalse(configuration.Success);
			Assert.IsFalse(examSelection.Success);
			Assert.IsFalse(message.Success);
			Assert.IsFalse(password.Success);
			Assert.IsFalse(reconfiguration.Success);
			Assert.IsFalse(serverFailure.Success);
			Assert.IsFalse(shutdown.Success);
		}
	}
}
