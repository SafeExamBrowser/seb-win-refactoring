/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour.Operations
{
	public class DelegateOperation : IOperation
	{
		private Action perform;
		private Action repeat;
		private Action revert;

		public bool Abort { get; private set; }
		public IProgressIndicator ProgressIndicator { private get; set; }

		public DelegateOperation(Action perform, Action repeat = null, Action revert = null)
		{
			this.perform = perform;
			this.repeat = repeat;
			this.revert = revert;
		}

		public void Perform()
		{
			perform?.Invoke();
		}

		public void Repeat()
		{
			repeat?.Invoke();
		}

		public void Revert()
		{
			revert?.Invoke();
		}
	}
}
