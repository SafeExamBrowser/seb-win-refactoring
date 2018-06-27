/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface.Windows;

namespace SafeExamBrowser.Runtime.UnitTests.Behaviour.Operations
{
	internal class PasswordDialogResultStub : IPasswordDialogResult
	{
		public string Password { get; set; }
		public bool Success { get; set; }
	}
}
