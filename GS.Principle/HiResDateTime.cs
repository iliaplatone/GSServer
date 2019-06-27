﻿/* Copyright(C) 2019  Rob Morgan (robert.morgan.e@gmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Diagnostics;
using System.Threading;

namespace GS.Principles
{
    /// <summary>
    /// Windows 8 or Server 2012 and higher. All others return using System.Diagnostics.Stopwatch />.
    /// </summary>
    public static class HiResDateTime
    {
        private static readonly long maxidle = TimeSpan.FromSeconds(10).Ticks;
        private const long TicksMultiplier = 1000 * TimeSpan.TicksPerMillisecond;
        private static readonly ThreadLocal<DateTime> starttime = new ThreadLocal<DateTime>(() => DateTime.UtcNow, false);
        private static readonly ThreadLocal<double> starttimestamp = new ThreadLocal<double>(() => Stopwatch.GetTimestamp(), false);

        /// <summary>
        /// High resolution supported
        /// Returns True on Windows 8 and Server 2012 and higher.
        /// </summary>
        private static bool IsPrecise { get; }

        /// <summary>
        /// Gets the datetime in UTC.
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                if (IsPrecise)
                {
                    NativeMethods.GetSystemTimePreciseAsFileTime(out var preciseTime);
                    return DateTime.FromFileTimeUtc(preciseTime);
                }
                double endTimestamp = Stopwatch.GetTimestamp();
                var durationInTicks = (endTimestamp - starttimestamp.Value) / Stopwatch.Frequency * TicksMultiplier;
                if (!(durationInTicks >= maxidle)) return starttime.Value.AddTicks((long) durationInTicks);
                starttimestamp.Value = Stopwatch.GetTimestamp();
                starttime.Value = DateTime.UtcNow;
                return starttime.Value;
            }
        }

        /// <summary>
        /// Creates an instance
        /// </summary>
        static HiResDateTime()
        {
            try
            {
                NativeMethods.GetSystemTimePreciseAsFileTime(out _);
                IsPrecise = true;
            }
            catch (EntryPointNotFoundException)
            {
                IsPrecise = false;
            }
        }
    }
}