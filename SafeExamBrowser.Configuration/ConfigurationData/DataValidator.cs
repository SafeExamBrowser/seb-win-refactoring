/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal class DataValidator
	{
		private readonly ILogger logger;

		internal DataValidator(ILogger logger)
		{
			this.logger = logger;
		}

		internal bool Validate(IDictionary<string, object> rawData)
		{
			var valid = true;

			valid &= ValidatePasswords(rawData);
			valid &= ValidateStringValues(rawData);

			if (valid)
			{
				logger.Debug("The configuration data is valid.");
			}
			else
			{
				logger.Error("The configuration data is invalid and can not be applied!");
			}

			return valid;
		}

		private bool ValidatePasswords(IDictionary<string, object> data)
		{
			var valid = true;

			valid &= ValidatePassword(Keys.Security.AdminPasswordHash, data);
			valid &= ValidatePassword(Keys.Security.QuitPasswordHash, data);
			valid &= ValidatePassword(Keys.Server.FallbackPasswordHash, data);

			return valid;
		}

		private bool ValidatePassword(string key, IDictionary<string, object> data)
		{
			var regex = new Regex(@"^$|^[0-9a-fA-F]{64}$");
			var valid = !data.TryGetValue(key, out var value) || value is null || (value is string s && regex.IsMatch(s));

			if (!valid)
			{
				logger.Warn($"The password '{key}' contains an invalid hash value ('{value}', {value?.GetType().FullName})!");
			}

			return valid;
		}

		private bool ValidateStringValues(object value, string key = default)
		{
			var valid = true;

			if (value is IList<object> array)
			{
				foreach (var element in array)
				{
					valid &= ValidateStringValues(element, key);
				}
			}

			if (value is IDictionary<string, object> dictionary)
			{
				foreach (var kvp in dictionary)
				{
					valid &= ValidateString(kvp.Key);
					valid &= ValidateStringValues(kvp.Value, kvp.Key);
				}
			}

			if (value is string @string)
			{
				valid &= ValidateString(key, @string);
			}

			return valid;
		}

		private bool ValidateString(string key, string value = default)
		{
			var regex = new Regex(@"""\s*,");
			var validateKey = value == default;
			var valid = validateKey ? !regex.IsMatch(key) : !regex.IsMatch(value);

			if (!valid && validateKey)
			{
				logger.Warn($"The configuration key '{key}' contains the invalid character sequence '\",'!");
			}
			else if (!valid)
			{
				logger.Warn($"The configuration value with key '{key}' contains the invalid character sequence '\",' ('{value}')!");
			}

			return valid;
		}
	}
}
