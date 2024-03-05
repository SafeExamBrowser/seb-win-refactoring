/*
* Copyright (c) 2022 ETH Zürich, IT Services
* 
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.Proctoring.Contracts.Events;
using SafeExamBrowser.Server.Contracts.Events.Proctoring;

namespace SafeExamBrowser.Proctoring
{
	internal abstract class ProctoringImplementation : INotification
	{
		internal abstract string Name { get; }

		public bool CanActivate { get; protected set; }
		public string Tooltip { get; protected set; }
		public IconResource IconResource { get; protected set; }

		internal event RemainingWorkUpdatedEventHandler RemainingWorkUpdated;
		public event NotificationChangedEventHandler NotificationChanged;

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

		internal virtual void ExecuteRemainingWork() { }
		internal virtual bool HasRemainingWork() => false;

		protected virtual void ActivateNotification() { }
		protected virtual void TerminateNotification() { }

		protected void InvokeNotificationChanged()
		{
			NotificationChanged?.Invoke();
		}

		protected void InvokeRemainingWorkUpdated(RemainingWorkUpdatedEventArgs args)
		{
			RemainingWorkUpdated?.Invoke(args);
		}
	}
}
