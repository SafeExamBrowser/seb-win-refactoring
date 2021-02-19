/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Proctoring;

namespace SafeExamBrowser.Proctoring
{
	public class ProctoringController : IProctoringController
	{
		private readonly IUserInterfaceFactory uiFactory;

		private IProctoringWindow window;

		public ProctoringController(IUserInterfaceFactory uiFactory)
		{
			this.uiFactory = uiFactory;
		}

		public void Initialize(ProctoringSettings settings)
		{
			var control = new ProctoringControl();

			window = uiFactory.CreateProctoringWindow(control);
			window.Show();
		}

		public void Terminate()
		{
			window?.Close();
		}
	}
}
