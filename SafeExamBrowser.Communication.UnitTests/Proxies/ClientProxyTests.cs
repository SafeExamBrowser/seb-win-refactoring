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
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Communication.UnitTests.Proxies
{
	[TestClass]
	public class ClientProxyTests
	{
		private Mock<ILogger> logger;
		private Mock<IProxyObjectFactory> proxyObjectFactory;
		private Mock<IProxyObject> proxy;
		private ClientProxy sut;

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

			sut = new ClientProxy("net.pipe://random/address/here", proxyObjectFactory.Object, logger.Object, default(Interlocutor));
			sut.Connect(Guid.NewGuid());
		}

		[TestMethod]
		public void MustCorrectlyInitiateShutdown()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Shutdown))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.InitiateShutdown();

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Shutdown)), Times.Once);
			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustFailIfShutdownCommandNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Shutdown))).Returns<Response>(null);

			var communication = sut.InitiateShutdown();

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyRequestAuthentication()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Authenticate))).Returns(new AuthenticationResponse());

			var communication = sut.RequestAuthentication();
			var response = communication.Value;

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Authenticate)), Times.Once);

			Assert.IsTrue(communication.Success);
			Assert.IsInstanceOfType(response, typeof(AuthenticationResponse));
		}

		[TestMethod]
		public void MustFailIfAuthenticationCommandNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.Authenticate))).Returns<Response>(null);

			var communication = sut.RequestAuthentication();

			Assert.AreEqual(default(AuthenticationResponse), communication.Value);
			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyInformAboutReconfigurationAbortion()
		{
			proxy.Setup(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ReconfigurationAborted))).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.InformReconfigurationAborted();

			proxy.Verify(p => p.Send(It.Is<SimpleMessage>(m => m.Purport == SimpleMessagePurport.ReconfigurationAborted)), Times.Once);
			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustFailIfReconfigurationAbortionNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<SimpleMessage>())).Returns<Response>(null);

			var communication = sut.InformReconfigurationAborted();

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyInformAboutReconfigurationDenial()
		{
			proxy.Setup(p => p.Send(It.IsAny<ReconfigurationDeniedMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.InformReconfigurationDenied(null);

			proxy.Verify(p => p.Send(It.IsAny<ReconfigurationDeniedMessage>()), Times.Once);
			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustFailIfReconfigurationDenialNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<ReconfigurationDeniedMessage>())).Returns<Response>(null);

			var communication = sut.InformReconfigurationDenied(null);

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyRequestExamSelection()
		{
			proxy.Setup(p => p.Send(It.IsAny<ExamSelectionRequestMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.RequestExamSelection(null, default(Guid));

			proxy.Verify(p => p.Send(It.IsAny<ExamSelectionRequestMessage>()), Times.Once);
			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustFailIfExamSelectionRequestNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<ExamSelectionRequestMessage>())).Returns<Response>(null);

			var communication = sut.RequestExamSelection(null, default(Guid));

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyRequestPassword()
		{
			proxy.Setup(p => p.Send(It.IsAny<PasswordRequestMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.RequestPassword(default(PasswordRequestPurpose), default(Guid));

			proxy.Verify(p => p.Send(It.IsAny<PasswordRequestMessage>()), Times.Once);
			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustFailIfPasswordRequestNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<PasswordRequestMessage>())).Returns<Response>(null);

			var communication = sut.RequestPassword(default(PasswordRequestPurpose), default(Guid));

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyRequestServerFailureAction()
		{
			proxy.Setup(p => p.Send(It.IsAny<ServerFailureActionRequestMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.RequestServerFailureAction(default(string), default(bool), default(Guid));

			proxy.Verify(p => p.Send(It.IsAny<ServerFailureActionRequestMessage>()), Times.Once);
			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustFailIfServerFailureActionRequestNotAcknowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<ServerFailureActionRequestMessage>())).Returns<Response>(null);

			var communication = sut.RequestServerFailureAction(default(string), default(bool), default(Guid));

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustCorrectlyShowMessage()
		{
			proxy.Setup(p => p.Send(It.IsAny<MessageBoxRequestMessage>())).Returns(new SimpleResponse(SimpleResponsePurport.Acknowledged));

			var communication = sut.ShowMessage(default(string), default(string), default(int), default(int), default(Guid));

			proxy.Verify(p => p.Send(It.IsAny<MessageBoxRequestMessage>()), Times.Once);
			Assert.IsTrue(communication.Success);
		}

		[TestMethod]
		public void MustFailIfMessageBoxRequestNotAchnowledged()
		{
			proxy.Setup(p => p.Send(It.IsAny<MessageBoxRequestMessage>())).Returns<Response>(null);

			var communication = sut.ShowMessage(default(string), default(string), default(int), default(int), default(Guid));

			Assert.IsFalse(communication.Success);
		}

		[TestMethod]
		public void MustExecuteAllOperationsFailsafe()
		{
			proxy.Setup(p => p.Send(It.IsAny<Message>())).Throws<Exception>();

			var authenticate = sut.RequestAuthentication();
			var examSelection = sut.RequestExamSelection(null, default(Guid));
			var message = sut.ShowMessage(default(string), default(string), default(int), default(int), default(Guid));
			var password = sut.RequestPassword(default(PasswordRequestPurpose), default(Guid));
			var reconfigurationAborted = sut.InformReconfigurationAborted();
			var reconfigurationDenied = sut.InformReconfigurationDenied(null);
			var serverFailure = sut.RequestServerFailureAction(default(string), default(bool), default(Guid));
			var shutdown = sut.InitiateShutdown();

			Assert.IsFalse(authenticate.Success);
			Assert.IsFalse(examSelection.Success);
			Assert.IsFalse(message.Success);
			Assert.IsFalse(password.Success);
			Assert.IsFalse(reconfigurationAborted.Success);
			Assert.IsFalse(reconfigurationDenied.Success);
			Assert.IsFalse(serverFailure.Success);
			Assert.IsFalse(shutdown.Success);
		}
	}
}
