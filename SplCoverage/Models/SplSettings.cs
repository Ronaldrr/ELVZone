using ELVZone.SplCoverage.Services;

namespace ELVZone.SplCoverage.Models
{
    public class SplSettings
    {
        public string SpeakerLibraryPath { get; set; }
        public double CellSizeMeters { get; set; }
        public double RequiredMinSpl { get; set; }
        public int CalculationFrequencyHz { get; set; }
        public bool UseDirectivity { get; set; }
        public string SpeakerMountingHeightParameterName { get; set; }
        public double CalculationPlaneHeightMeters { get; set; }
        public string CellFamilyName { get; set; }
        public string GreyTypeName { get; set; }
        public string YellowTypeName { get; set; }
        public string OrangeTypeName { get; set; }
        public string RedTypeName { get; set; }

        public static SplSettings CreateDefault()
        {
            return new SplSettings
            {
                SpeakerLibraryPath = SpeakerLibraryPathResolver.GetDefaultLibraryPath(),
                CellSizeMeters = 0.5,
                RequiredMinSpl = 75,
                CalculationFrequencyHz = 1000,
                UseDirectivity = true,
                SpeakerMountingHeightParameterName = string.Empty,
                CalculationPlaneHeightMeters = 1.5,
                CellFamilyName = "SPL_Cell",
                GreyTypeName = "SPL_Grey",
                YellowTypeName = "SPL_Yellow",
                OrangeTypeName = "SPL_Orange",
                RedTypeName = "SPL_Red"
            };
        }

        public void EnsureShape()
        {
            var defaults = CreateDefault();
            if (string.IsNullOrWhiteSpace(SpeakerLibraryPath)) SpeakerLibraryPath = defaults.SpeakerLibraryPath;
            if (CellSizeMeters <= 0) CellSizeMeters = defaults.CellSizeMeters;
            if (RequiredMinSpl <= 0) RequiredMinSpl = defaults.RequiredMinSpl;
            if (CalculationFrequencyHz <= 0) CalculationFrequencyHz = defaults.CalculationFrequencyHz;
            if (CalculationPlaneHeightMeters < 0) CalculationPlaneHeightMeters = defaults.CalculationPlaneHeightMeters;
            if (string.IsNullOrWhiteSpace(CellFamilyName)) CellFamilyName = defaults.CellFamilyName;
            if (string.IsNullOrWhiteSpace(GreyTypeName)) GreyTypeName = defaults.GreyTypeName;
            if (string.IsNullOrWhiteSpace(YellowTypeName)) YellowTypeName = defaults.YellowTypeName;
            if (string.IsNullOrWhiteSpace(OrangeTypeName)) OrangeTypeName = defaults.OrangeTypeName;
            if (string.IsNullOrWhiteSpace(RedTypeName)) RedTypeName = defaults.RedTypeName;
        }
    }
}
