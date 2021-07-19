using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace HeavyRainAnalysis
{
    public class Parser
    {
        private readonly int[] _fieldWiths = new[] { 21, 20, 48, 20, 20, 20, 20, 20, 20, 20, 20 };
        readonly string _timestampFormat = "yyyy-MM-dd HH:mm:ss";
        private readonly string _format;

        public Parser()
        {
            _format = $"{{0,-{_fieldWiths[0]}:{_timestampFormat}}}" +
                      $"{{1,-{_fieldWiths[1]}}}" +
                      $"{{2,-{_fieldWiths[2]}}}" +
                      $"{{3,-{_fieldWiths[3]}:##.000000}}" +
                      $"{{4,-{_fieldWiths[4]}:##.000000}}" +
                      $"{{5,-{_fieldWiths[5]}}}" +
                      $"{{6,-{_fieldWiths[6]}}}" +
                      $"{{7,-{_fieldWiths[7]}}}" +
                      $"{{8,-{_fieldWiths[8]}}}" +
                      $"{{9,-{_fieldWiths[9]}}}" +
                      $"{{10,-{_fieldWiths[10]}}}";
        }

        public IEnumerable<RainData> Parse(Stream contents)
        {
            var parser = new TextFieldParser(contents);
            parser.CommentTokens = new[] { "#" };
            parser.TextFieldType = FieldType.FixedWidth;
            parser.SetFieldWidths(_fieldWiths);
            parser.TrimWhiteSpace = true;
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                if (fields == null) yield break;
                // DTG Date/time stamp
                //# LOCATION = identifier, NAME = identifier, LATITUDE in degrees (WGS84), LONGITUDE in degrees (WGS84), ALTITUDE in 0.1 m relative to Mean Sea Level (MSL)
                //# DR_PWS_10 is neerslag duur present weather sensor 10' eenheid seconde
                //# DR_REGENM_10 is neerslag duur electrische regenmeter 10' eenheid seconde
                //# WW_COR_10 is weer gecorr. code present weather sensor 10' eenheid code
                //# RI_PWS_10 is neerslag intensiteit present weather sensor 10' eenheid millimeter per uur
                //# RI_REGENM_10 is neerslag intensiteit electrische regenmeter 10' eenheid millimeter per uur
                int field = 0;
                yield return new RainData
                {
                    Aggregation = TimeSpan.FromMinutes(10),
                    Timestamp = DateTime.ParseExact(fields[field++], _timestampFormat, CultureInfo.InvariantCulture),
                    Id = fields[field++],
                    Name = fields[field++],
                    Latitude = ParseDouble(fields[field++]),
                    Longitude = ParseDouble(fields[field++]),
                    Altitude = ParseDouble(fields[field++]),
                    DurationSensor = ParseTimeSpan(fields[field++]),
                    DurationGauge = ParseTimeSpan(fields[field++]),
                    WCor = fields[field++],
                    IntensitySensor = ParseDouble(fields[field++]),
                    IntensityGauge = ParseDouble(fields[field++]),
                };
            }
        }

        private TimeSpan ParseTimeSpan(string input)
        {
            if (string.IsNullOrEmpty(input))
                return TimeSpan.Zero;
            return TimeSpan.FromSeconds(float.Parse(input, CultureInfo.InvariantCulture));
        }

        private double ParseDouble(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0.0;
            return double.Parse(input, CultureInfo.InvariantCulture);
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