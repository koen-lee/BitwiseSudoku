using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace HeavyRainAnalysis
{
    public class Parser
    {
        private readonly int[] _fieldWidths = new[] { 19, 22, 48, 20, 20, 20, 20, 20, 20, 20, 20 };
        readonly string _timestampFormat = "yyyy-MM-dd HH:mm:ss";
        private readonly string _format;
        private readonly int[] _offsets;

        public Parser()
        {
            _format = $"{{0,-{_fieldWidths[0]}:{_timestampFormat}}}" +
                      $"{{1,-{_fieldWidths[1]}}}" +
                      $"{{2,-{_fieldWidths[2]}}}" +
                      $"{{3,-{_fieldWidths[3]}:##.000000}}" +
                      $"{{4,-{_fieldWidths[4]}:##.000000}}" +
                      $"{{5,-{_fieldWidths[5]}}}" +
                      $"{{6,-{_fieldWidths[6]}}}" +
                      $"{{7,-{_fieldWidths[7]}}}" +
                      $"{{8,-{_fieldWidths[8]}}}" +
                      $"{{9,-{_fieldWidths[9]}}}" +
                      $"{{10,-{_fieldWidths[10]}}}";
            _offsets = new int[_fieldWidths.Length];
            var offset = 0;
            for (int i = 0; i < _fieldWidths.Length; i++)
            {
                _offsets[i] = offset;
                offset += _fieldWidths[i];
            }
        }

        public IEnumerable<RainData> Parse(Stream contents)
        {
            var textReader = new StreamReader(contents, Encoding.ASCII);
            var line = textReader.ReadLine();
            while (line != null)
            {
                if (!line.StartsWith("#"))
                {
                    yield return Parse(line);
                }
                line = textReader.ReadLine();
            }
        }

        private RainData Parse(string line)
        {
            ReadOnlySpan<char> GetSpan(int index) =>
             line.AsSpan(_offsets[index], _fieldWidths[index]);

            int field = 0;
            return new RainData
            {
                Aggregation = TimeSpan.FromMinutes(10),
                Timestamp = DateTime.ParseExact(GetSpan(field++), _timestampFormat, CultureInfo.InvariantCulture),
                Id = new string(GetSpan(field++)),
                Name = new string(GetSpan(field++)),
                Latitude = ParseDouble(GetSpan(field++)),
                Longitude = ParseDouble(GetSpan(field++)),
                Altitude = ParseDouble(GetSpan(field++)),
                DurationSensor = ParseTimeSpan(GetSpan(field++)),
                DurationGauge = ParseTimeSpan(GetSpan(field++)),
                WCor = new string(GetSpan(field++)),
                IntensitySensor = ParseDouble(GetSpan(field++)),
                IntensityGauge = ParseDouble(GetSpan(field++)),
            };
        }

        private TimeSpan ParseTimeSpan(ReadOnlySpan<char> input)
        {
            if (float.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var seconds))
                return TimeSpan.FromSeconds(seconds);
            return TimeSpan.Zero;
        }

        private double ParseDouble(ReadOnlySpan<char> input)
        {
            if (double.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
                return result;
            return 0.0;
        }

        public string Serialize(RainData input)
        {
            return string.Format(CultureInfo.InvariantCulture, _format,
                input.Timestamp,
                input.Id,
                input.Name,
                input.Latitude,
                input.Longitude,
                input.Altitude,
                input.DurationSensor.TotalSeconds,
                input.DurationGauge.TotalSeconds,
                input.WCor,
                input.IntensitySensor,
                input.IntensityGauge);
        }
    }
}