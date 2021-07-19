using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace HeavyRainAnalysis
{
    public class ApiKey
    {
        public const string Default =
            "eyJvcmciOiI1ZTU1NGUxOTI3NGE5NjAwMDEyYTNlYjEiLCJpZCI6ImFiNTkzYTA1OGFiOTRiNGRhZTY4YzIzZmE0OWIzYmJkIiwiaCI6Im11cm11cjEyOCJ9";
    }

    public static class Program
    {
        public static void Main(
            string filename =
                @"C:\Users\koen.lee\Documents\Arduino\wakeuplight\BitwiseSudoku\HeavyRainAnalysis\kis_tor_200304.gz",
            bool summary = false)
        {
            var stopwatch = Stopwatch.StartNew();
            using (var stream = new GZipStream(new FileStream(filename, FileMode.Open), CompressionMode.Decompress))
            {
                var parser = new Parser();
                var maxPerStationForFile = from rainData in parser.Parse(stream)
                                           group rainData by (rainData.Id, new DateTime(rainData.Timestamp.Year, rainData.Timestamp.Month, 1))
                    into rainByStation
                                           select Aggregate(rainByStation);
                foreach (var rainData in maxPerStationForFile)
                {
                    Console.WriteLine(parser.Serialize(rainData));

                }
            }
            if (summary)
                Console.WriteLine(stopwatch.Elapsed.TotalSeconds);
        }

        private static RainData Aggregate(IGrouping<(string Id, DateTime Date), RainData> rainByStation)
        {
            var prototype = rainByStation.First();
            return new RainData
            {
                Id = rainByStation.Key.Id,
                Timestamp = rainByStation.Key.Date,
                Aggregation = rainByStation.Sum(r => r.Aggregation),
                Name = prototype.Name,
                Altitude = prototype.Altitude,
                Latitude = prototype.Latitude,
                Longitude = prototype.Longitude,
                DurationGauge = rainByStation.Sum(s => s.DurationGauge),
                DurationSensor = rainByStation.Sum(s => s.DurationSensor),
                IntensityGauge = rainByStation.Max(r => r.IntensityGauge),
                IntensitySensor = rainByStation.Max(r => r.IntensitySensor),
                WCor = prototype.WCor,
            };
        }

        static TimeSpan Sum<TInput>(this IEnumerable<TInput> input, Func<TInput, TimeSpan> selector)
        {
            return TimeSpan.FromTicks(input.Select(selector).Sum(t => t.Ticks));
        }
    }
}

