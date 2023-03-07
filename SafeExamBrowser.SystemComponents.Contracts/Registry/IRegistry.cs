/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.SystemComponents.Contracts.Registry.Events;

namespace SafeExamBrowser.SystemComponents.Contracts.Registry
{
	/// <summary>
	/// Provides functionality related to the Windows registry.
	/// </summary>
	public interface IRegistry
	{
		/// <summary>
		/// Fired when a registry value previously registred via <see cref="StartMonitoring(string, string)"/> has changed.
		/// </summary>
		event RegistryValueChangedEventHandler ValueChanged;

		/// <summary>
		/// Starts monitoring the specified registry value.
		/// </summary>
		void StartMonitoring(string key, string name);

		/// <summary>
		/// Stops the monitoring of all previously registered registry values.
		/// </summary>
		void StopMonitoring();

		/// <summary>
		/// Attempts to read the value of the given name under the specified registry key.
		/// </summary>
		bool TryRead(string key, string name, out object value);
	}
}
