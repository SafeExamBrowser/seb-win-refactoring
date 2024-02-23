/*
* Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
* 
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.Server.Contracts.Events.Proctoring;

namespace SafeExamBrowser.Proctoring
{
	internal abstract class ProctoringImplementation : INotification
	{
		internal abstract string Name { get; }

		public bool CanActivate { get; protected set; }
		public string Tooltip { get; protected set; }
		public IconResource IconResource { get; protected set; }

		public abstract event NotificationChangedEventHandler NotificationChanged;

		void INotification.Activate()
		{
			ActivateNotification();
		}

		void INotification.Terminate()
		{
			TerminateNotification();
		}

		internal abstract void Initialize();
		internal abstract void ProctoringConfigurationReceived(bool allowChat, bool receiveAudio, bool receiveVideo);
		internal abstract void ProctoringInstructionReceived(InstructionEventArgs args);
		internal abstract void Start();
		internal abstract void Stop();
		internal abstract void Terminate();

		protected virtual void ActivateNotification() { }
		protected virtual void TerminateNotification() { }
	}
}
