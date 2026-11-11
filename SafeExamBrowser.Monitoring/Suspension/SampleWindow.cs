/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace SafeExamBrowser.Monitoring.Suspension
{
	internal class SampleWindow
	{
		private readonly TimeSpan duration;
		private readonly Queue<Sample> samples;

		internal int Count => samples.Count;
		internal TimeSpan Duration => Count == 0 ? TimeSpan.Zero : samples.First().Time - samples.Peek().Time;
		internal double Ratio => Count == 0 ? 0 : (double) Suspended / Count;
		internal int Suspended => samples.Where(s => s.Suspended).Count();

		internal SampleWindow(TimeSpan duration)
		{
			this.duration = duration;
			this.samples = new Queue<Sample>();
		}

		internal void Add(TimeSpan time, bool suspended)
		{
			samples.Enqueue(new Sample(time, suspended));

			while (Count > 0 && time - samples.Peek().Time > duration)
			{
				samples.Dequeue();
			}
		}

		internal void Clear()
		{
			samples.Clear();
		}

		private readonly struct Sample
		{
			internal readonly TimeSpan Time;
			internal readonly bool Suspended;

			internal Sample(TimeSpan time, bool suspended)
			{
				Time = time;
				Suspended = suspended;
			}
		}
	}
}
