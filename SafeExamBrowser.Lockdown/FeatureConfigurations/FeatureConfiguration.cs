/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.Serialization;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown.FeatureConfigurations
{
	[Serializable]
	internal abstract class FeatureConfiguration : IFeatureConfiguration
	{
		[NonSerialized]
		protected ILogger logger;

		public Guid Id { get; }
		public Guid GroupId { get; }

		public FeatureConfiguration(Guid groupId, ILogger logger)
		{
			this.GroupId = groupId;
			this.Id = Guid.NewGuid();
			this.logger = logger;
		}

		public abstract bool DisableFeature();
		public abstract bool EnableFeature();
		public abstract FeatureConfigurationStatus GetStatus();
		public abstract void Initialize();
		public abstract bool Reset();
		public abstract bool Restore();

		public override string ToString()
		{
			return $"{GetType().Name} ({Id})";
		}

		[OnDeserialized]
		private void OnDeserializedMethod(StreamingContext context)
		{
			logger = (context.Context as IModuleLogger).CloneFor(GetType().Name);
		}
	}
}
