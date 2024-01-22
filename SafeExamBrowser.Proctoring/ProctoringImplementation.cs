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
using SafeExamBrowser.Server.Contracts.Events;

namespace SafeExamBrowser.Proctoring
{
	internal abstract class ProctoringImplementation : INotification
	{
		internal abstract string Name { get; }

		public abstract string Tooltip { get; protected set; }
		public abstract IconResource IconResource { get; protected set; }

		public abstract event NotificationChangedEventHandler NotificationChanged;

		public abstract void Activate();
		void INotification.Terminate() { }

		internal abstract void Initialize();
		internal abstract void ProctoringConfigurationReceived(bool allowChat, bool receiveAudio, bool receiveVideo);
		internal abstract void ProctoringInstructionReceived(ProctoringInstructionEventArgs args);
		internal abstract void StartProctoring();
		internal abstract void StopProctoring();
		internal abstract void Terminate();
	}
}
