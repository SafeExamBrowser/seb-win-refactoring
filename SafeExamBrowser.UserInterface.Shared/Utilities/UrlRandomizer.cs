/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Text;

namespace SafeExamBrowser.UserInterface.Shared.Utilities
{
	public static class UrlRandomizer
	{
		private const string DIGITS = "0123456789";
		private const string LETTERS = "abcdefghijklmnopqrstuvwxyz";

		public static string Randomize(string url)
		{
			var generator = new Random();
			var result = new StringBuilder();

			foreach (var character in url)
			{
				if (Char.IsDigit(character))
				{
					result.Append(DIGITS[generator.Next(DIGITS.Length)]);
				}
				else if (Char.IsLetter(character))
				{
					result.Append(LETTERS[generator.Next(LETTERS.Length)]);
				}
				else
				{
					result.Append(character);
				}
			}

			return result.ToString();
		}
	}
}
