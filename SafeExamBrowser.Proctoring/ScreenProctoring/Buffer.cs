/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.ScreenProctoring.Data;
using SafeExamBrowser.Proctoring.ScreenProctoring.Imaging;

namespace SafeExamBrowser.Proctoring.ScreenProctoring
{
	internal class Buffer
	{
		private readonly object @lock = new object();

		private readonly List<(MetaData metaData, DateTime schedule, ScreenShot screenShot)> list;
		private readonly ILogger logger;

		internal int Count
		{
			get
			{
				lock (@lock)
				{
					return list.Count;
				}
			}
		}

		internal Buffer(ILogger logger)
		{
			this.list = new List<(MetaData, DateTime, ScreenShot)>();
			this.logger = logger;
		}

		internal bool Any()
		{
			lock (@lock)
			{
				return list.Any();
			}
		}

		internal void Clear()
		{
			lock (@lock)
			{
				list.Clear();
				logger.Debug("Cleared all data.");
			}
		}

		internal void Dequeue()
		{
			lock (@lock)
			{
				if (list.Any())
				{
					var (_, schedule, screenShot) = list.First();

					list.RemoveAt(0);
					logger.Debug($"Removed data for '{screenShot.CaptureTime:HH:mm:ss} -> {schedule:HH:mm:ss}', {Count} item(s) remaining.");
				}
			}
		}

		internal void Enqueue(MetaData metaData, DateTime schedule, ScreenShot screenShot)
		{
			lock (@lock)
			{
				list.Add((metaData, schedule, screenShot));
				list.Sort((a, b) => DateTime.Compare(a.schedule, b.schedule));

				logger.Debug($"Buffered data for '{screenShot.CaptureTime:HH:mm:ss} -> {schedule:HH:mm:ss}', now holding {Count} item(s).");
			}
		}

		internal bool TryPeek(out MetaData metaData, out DateTime schedule, out ScreenShot screenShot)
		{
			lock (@lock)
			{
				metaData = default;
				schedule = default;
				screenShot = default;

				if (list.Any())
				{
					(metaData, schedule, screenShot) = list.First();
				}

				return metaData != default && screenShot != default;
			}
		}
	}
}
