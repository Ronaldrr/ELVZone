using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ELVZone.Models
{
    [DataContract]
    public class ViewZoneSettings
    {
        [DataMember(Order = 1)]
        public ParameterSetting HorizontalAngleParameter { get; set; }

        [DataMember(Order = 2)]
        public ParameterSetting VerticalAngleParameter { get; set; }

        [DataMember(Order = 3)]
        public ParameterSetting MountingHeightParameter { get; set; }

        [DataMember(Order = 4)]
        public ParameterSetting Zone1LengthParameter { get; set; }

        [DataMember(Order = 5)]
        public ParameterSetting Zone2LengthParameter { get; set; }

        [DataMember(Order = 6)]
        public ParameterSetting Zone3LengthParameter { get; set; }

        [DataMember(Order = 7)]
        public ParameterSetting Zone4LengthParameter { get; set; }

        [DataMember(Order = 8)]
        public ParameterSetting TotalLengthParameter { get; set; }

        [DataMember(Order = 9)]
        public List<ZoneStyleSetting> ZoneStyles { get; set; }

        [DataMember(Order = 10)]
        public double DefaultHorizontalAngleDegrees { get; set; }

        [DataMember(Order = 11)]
        public double DefaultVerticalAngleDegrees { get; set; }

        [DataMember(Order = 12)]
        public double DefaultMountingHeightMeters { get; set; }

        [DataMember(Order = 13)]
        public double DefaultZone1LengthMeters { get; set; }

        [DataMember(Order = 14)]
        public double DefaultZone2LengthMeters { get; set; }

        [DataMember(Order = 15)]
        public double DefaultZone3LengthMeters { get; set; }

        [DataMember(Order = 16)]
        public double DefaultZone4LengthMeters { get; set; }

        [DataMember(Order = 17)]
        public double DefaultTotalLengthMeters { get; set; }

        public static ViewZoneSettings CreateDefault()
        {
            return new ViewZoneSettings
            {
                HorizontalAngleParameter = new ParameterSetting(),
                VerticalAngleParameter = new ParameterSetting(),
                MountingHeightParameter = new ParameterSetting(),
                Zone1LengthParameter = new ParameterSetting(),
                Zone2LengthParameter = new ParameterSetting(),
                Zone3LengthParameter = new ParameterSetting(),
                Zone4LengthParameter = new ParameterSetting(),
                TotalLengthParameter = new ParameterSetting(),
                ZoneStyles = new List<ZoneStyleSetting>
                {
                    new ZoneStyleSetting(),
                    new ZoneStyleSetting(),
                    new ZoneStyleSetting(),
                    new ZoneStyleSetting()
                },
                DefaultHorizontalAngleDegrees = 60,
                DefaultVerticalAngleDegrees = 45,
                DefaultMountingHeightMeters = 3,
                DefaultZone1LengthMeters = 5,
                DefaultZone2LengthMeters = 5,
                DefaultZone3LengthMeters = 5,
                DefaultZone4LengthMeters = 5,
                DefaultTotalLengthMeters = 20
            };
        }

        public void EnsureShape()
        {
            HorizontalAngleParameter = HorizontalAngleParameter ?? new ParameterSetting();
            VerticalAngleParameter = VerticalAngleParameter ?? new ParameterSetting();
            MountingHeightParameter = MountingHeightParameter ?? new ParameterSetting();
            Zone1LengthParameter = Zone1LengthParameter ?? new ParameterSetting();
            Zone2LengthParameter = Zone2LengthParameter ?? new ParameterSetting();
            Zone3LengthParameter = Zone3LengthParameter ?? new ParameterSetting();
            Zone4LengthParameter = Zone4LengthParameter ?? new ParameterSetting();
            TotalLengthParameter = TotalLengthParameter ?? new ParameterSetting();
            ZoneStyles = ZoneStyles ?? new List<ZoneStyleSetting>();

            while (ZoneStyles.Count < 4)
            {
                ZoneStyles.Add(new ZoneStyleSetting());
            }
        }
    }
}
