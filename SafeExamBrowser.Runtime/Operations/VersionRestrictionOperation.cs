/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.Settings.Security;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class VersionRestrictionOperation : SessionOperation
	{
		private readonly ILogger logger;
		private readonly IText text;

		private IList<VersionRestriction> Restrictions => Context.Next.Settings.Security.VersionRestrictions;

		public override event ActionRequiredEventHandler ActionRequired;
		public override event StatusChangedEventHandler StatusChanged;

		public VersionRestrictionOperation(ILogger logger, SessionContext context, IText text) : base(context)
		{
			this.logger = logger;
			this.text = text;
		}

		public override OperationResult Perform()
		{
			return ValidateRestrictions();
		}

		public override OperationResult Repeat()
		{
			return ValidateRestrictions();
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private OperationResult ValidateRestrictions()
		{
			var result = OperationResult.Success;

			logger.Info("Validating version restrictions...");
			StatusChanged?.Invoke(TextKey.OperationStatus_ValidateVersionRestrictions);

			if (Restrictions.Any())
			{
				var requiredVersions = $"'{string.Join("', '", Restrictions)}'";
				var version = Context.Next.AppConfig.ProgramInformationalVersion;

				if (Restrictions.Any(r => IsFulfilled(r)))
				{
					logger.Info($"The installed SEB version '{version}' complies with the version restrictions: {requiredVersions}.");
				}
				else
				{
					result = OperationResult.Aborted;
					logger.Error($"The installed SEB version '{version}' does not comply with the version restrictions: {requiredVersions}.");

					ActionRequired?.Invoke(new VersionRestrictionMessageArgs(version, BuildRequiredVersions()));
				}
			}
			else
			{
				logger.Info($"There are no version restrictions for the configuration.");
			}

			return result;
		}

		private bool IsFulfilled(VersionRestriction restriction)
		{
			var isFulfilled = true;
			var (major, minor, patch, build, isAllianceEdition) = GetVersion();

			if (restriction.IsMinimumRestriction)
			{
				isFulfilled &= restriction.Major <= major;

				if (restriction.Major == major)
				{
					isFulfilled &= restriction.Minor <= minor;

					if (restriction.Minor == minor)
					{
						isFulfilled &= !restriction.Patch.HasValue || restriction.Patch <= patch;

						if (restriction.Patch == patch)
						{
							isFulfilled &= !restriction.Build.HasValue || restriction.Build <= build;
						}
					}
				}

				isFulfilled &= !restriction.RequiresAllianceEdition || isAllianceEdition;
			}
			else
			{
				isFulfilled &= restriction.Major == major;
				isFulfilled &= restriction.Minor == minor;
				isFulfilled &= !restriction.Patch.HasValue || restriction.Patch == patch;
				isFulfilled &= !restriction.Build.HasValue || restriction.Build == build;
				isFulfilled &= !restriction.RequiresAllianceEdition || isAllianceEdition;
			}

			return isFulfilled;
		}

		private (int major, int minor, int patch, int build, bool isAllianceEdition) GetVersion()
		{
			var parts = Context.Next.AppConfig.ProgramBuildVersion.Split('.');
			var major = int.Parse(parts[0]);
			var minor = int.Parse(parts[1]);
			var patch = int.Parse(parts[2]);
			var build = int.Parse(parts[3]);
			var isAllianceEdition = Context.Next.AppConfig.ProgramInformationalVersion.Contains("Alliance Edition");

			return (major, minor, patch, build, isAllianceEdition);
		}

		private string BuildRequiredVersions()
		{
			var info = new StringBuilder();
			var minimumVersionText = text.Get(TextKey.MessageBox_VersionRestrictionMinimum);

			info.AppendLine();
			info.AppendLine();

			foreach (var restriction in Restrictions)
			{
				var build = restriction.Build.HasValue ? $".{restriction.Build}" : "";
				var patch = restriction.Patch.HasValue ? $".{restriction.Patch}" : "";
				var allianceEdition = restriction.RequiresAllianceEdition ? " Alliance Edition" : "";
				var version = $"{restriction.Major}.{restriction.Minor}{patch}{build}{allianceEdition}";

				if (restriction.IsMinimumRestriction)
				{
					info.AppendLine(minimumVersionText.Replace("%%_VERSION_%%", version));
				}
				else
				{
					info.AppendLine($"SEB {version}");
				}
			}

			info.AppendLine();

			return info.ToString();
		}
	}
}
