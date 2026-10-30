/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.WindowsApi.Contracts
{
	/// <summary>
	/// Provides access to the input profiles of the current user via the Text Services Framework (TSF). An input profile is
	/// either a classic keyboard layout (identified by its HKL) or a text service, i.e. an input method editor like
	/// "Vietnamese Telex" or "Microsoft IME" (identified by its CLSID and profile GUID). Text services do not have an HKL
	/// of their own, they only share the HKL of their substitute layout (e.g. "US" for the Vietnamese IMEs). This is why
	/// they can neither be enumerated nor activated via the legacy Win32 keyboard layout API (<c>GetKeyboardLayoutList</c>,
	/// <c>ActivateKeyboardLayout</c>) which is used by <c>System.Windows.Forms.InputLanguage</c>.
	/// 
	/// IMPORTANT: TSF is apartment-threaded, all members of an instance must be used on the (STA) thread which created it,
	/// and that thread must run a message loop in order for the activation notifications to be delivered.
	/// </summary>
	public interface ITextService : IDisposable
	{
		/// <summary>
		/// Fired when an input profile has been activated, e.g. by the user via <c>Win+Space</c> or <c>Alt+Shift</c>.
		/// </summary>
		event InputProfileActivatedEventHandler ProfileActivated;

		/// <summary>
		/// Activates the given profile for the calling thread (Windows propagates the change to the other windows of the
		/// user session unless the user has enabled "Let me set a different input method for each app window").
		/// </summary>
		void ActivateProfile(IInputProfile profile);

		/// <summary>
		/// Retrieves the description of a text service profile, e.g. "Vietnamese Telex". Only valid for text services, the
		/// name of a keyboard layout must be retrieved via <see cref="GetKeyboardLayoutName(IntPtr)"/>.
		/// </summary>
		string GetDescription(IInputProfile profile);

		/// <summary>
		/// Retrieves the localized display name of the given keyboard layout handle (HKL) as shown in the Windows settings,
		/// e.g. "Arabic (101)". Returns the KLID if no display name could be determined.
		/// </summary>
		string GetKeyboardLayoutName(IntPtr hkl);

		/// <summary>
		/// Retrieves all input profiles of the current user, including disabled ones and ones which are not keyboards.
		/// </summary>
		IList<IInputProfile> GetProfiles();

		/// <summary>
		/// Starts monitoring activation changes, see <see cref="ProfileActivated"/>.
		/// </summary>
		void StartMonitoring();

		/// <summary>
		/// Stops monitoring activation changes.
		/// </summary>
		void StopMonitoring();

		/// <summary>
		/// Attempts to retrieve the keyboard profile which is currently active for the calling thread.
		/// </summary>
		bool TryGetActiveProfile(out IInputProfile profile);

		/// <summary>
		/// Retrieves the culture of the given input language identifier.
		/// </summary>
		bool TryGetCulture(ushort languageId, out CultureInfo culture);
	}
}
