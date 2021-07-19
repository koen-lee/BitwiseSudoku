using System;
using System.ComponentModel.DataAnnotations;

namespace HeavyRainAnalysis
{
    public class RainData
    {
        public TimeSpan Aggregation { get; set; }
        /// DTG Date/time stamp
        public DateTime Timestamp { get; set; }
        ///# LOCATION = identifier 
        public string Id { get; set; }
        ///NAME = identifier, 
        public string Name { get; set; }
        /// LATITUDE in degrees (WGS84)
        public double Latitude { get; set; }
        /// <summary>
        ///  LONGITUDE in degrees (WGS84)
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// ALTITUDE in 0.1 m relative to Mean Sea Level (MSL)
        /// </summary>
        public double Altitude { get; set; }
        ///# DR_PWS_10 is neerslag duur present weather sensor 10' eenheid seconde

        public TimeSpan DurationSensor { get; set; }
        ///# DR_REGENM_10 is neerslag duur electrische regenmeter 10' eenheid seconde
        public TimeSpan DurationGauge { get; set; }
        ///# WW_COR_10 is weer gecorr. code present weather sensor 10' eenheid code
        public string WCor { get; set; }
        ///# RI_PWS_10 is neerslag intensiteit present weather sensor 10' eenheid millimeter per uur
        public double IntensitySensor { get; set; }
        ///# RI_REGENM_10 is neerslag intensiteit electrische regenmeter 10' eenheid millimeter per uur
        public double IntensityGauge { get; set; }

        public override string ToString()
        {
            return $"{nameof(Timestamp)}: {Timestamp}, {nameof(Name)}: {Name}, {nameof(DurationGauge)}: {DurationGauge}, {nameof(IntensityGauge)}: {IntensityGauge}";
        }
    }
}