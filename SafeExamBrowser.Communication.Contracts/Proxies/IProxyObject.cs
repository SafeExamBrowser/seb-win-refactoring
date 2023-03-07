/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ServiceModel;

namespace SafeExamBrowser.Communication.Contracts.Proxies
{
	/// <summary>
	/// The communication object to be used in an <see cref="ICommunicationProxy"/>.
	/// </summary>
	public interface IProxyObject : ICommunication, ICommunicationObject
	{
	}
}
