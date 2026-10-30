/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.WindowsApi.Contracts
{
	/// <summary>
	/// Identifies an input profile of the Text Services Framework, see <see cref="ITextService"/>.
	/// </summary>
	public interface IInputProfile : IEquatable<IInputProfile>
	{
		/// <summary>
		/// The CLSID of the text service, <see cref="Guid.Empty"/> for keyboard layouts.
		/// </summary>
		Guid ClassId { get; set; }

		/// <summary>
		/// The keyboard layout handle, <see cref="IntPtr.Zero"/> for text services.
		/// </summary>
		IntPtr Hkl { get; set; }

		/// <summary>
		/// Indicates whether the profile is currently active.
		/// </summary>
		bool IsActive { get; set; }

		/// <summary>
		/// Indicates whether the profile is enabled, i.e. installed for the current user.
		/// </summary>
		bool IsEnabled { get; set; }

		/// <summary>
		/// Indicates whether the profile is a keyboard layout or a text service of the keyboard category (as opposed to
		/// e.g. speech or handwriting text services).
		/// </summary>
		bool IsKeyboard { get; set; }

		/// <summary>
		/// Indicates whether this keyboard layout is substituted by a text service, i.e. it only serves as the underlying
		/// layout of a text service and is thus not shown by Windows as a layout of its own.
		/// </summary>
		bool IsSubstituted { get; set; }

		/// <summary>
		/// Indicates whether the profile is a text service (as opposed to a keyboard layout).
		/// </summary>
		bool IsTextService { get; }

		/// <summary>
		/// The input language identifier (LANGID) of the profile.
		/// </summary>
		ushort LanguageId { get; set; }

		/// <summary>
		/// The GUID of the text service profile, <see cref="Guid.Empty"/> for keyboard layouts.
		/// </summary>
		Guid ProfileId { get; set; }

		/// <summary>
		/// The type of the profile, either a keyboard layout or an input processor.
		/// </summary>
		uint ProfileType { get; set; }
	}
}
