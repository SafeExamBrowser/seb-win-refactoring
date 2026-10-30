/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.WindowsApi.TextServices
{
	internal class InputProfile : IInputProfile
	{
		public Guid ClassId { get; set; }
		public IntPtr Hkl { get; set; }
		public bool IsActive { get; set; }
		public bool IsEnabled { get; set; }
		public bool IsKeyboard { get; set; }
		public bool IsSubstituted { get; set; }
		public bool IsTextService => ProfileType == TextServicesFramework.TF_PROFILETYPE_INPUTPROCESSOR;
		public ushort LanguageId { get; set; }
		public Guid ProfileId { get; set; }
		public uint ProfileType { get; set; }

		public override bool Equals(object obj)
		{
			return obj is InputProfile other && Equals(other);
		}

		public bool Equals(IInputProfile other)
		{
			var equals = ProfileType == other.ProfileType && LanguageId == other.LanguageId;

			if (IsTextService)
			{
				equals &= ClassId == other.ClassId && ProfileId == other.ProfileId;
			}
			else
			{
				equals &= Hkl == other.Hkl;
			}

			return equals;
		}

		public override int GetHashCode()
		{
			return IsTextService ? ClassId.GetHashCode() ^ ProfileId.GetHashCode() : Hkl.GetHashCode() ^ LanguageId;
		}

		public override string ToString()
		{
			var identifier = IsTextService ? $"{ClassId:B} {ProfileId:B}" : $"0x{(long) Hkl:X8}";
			var language = $"0x{LanguageId:X4}";
			var type = IsTextService ? "TIP" : "HKL";

			return $"{type} {language} {identifier}";
		}
	}
}
