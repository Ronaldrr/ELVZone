namespace ELVZone.Core.Utils
{
    public static class UnitConversion
    {
        public const double FeetPerMeter = 3.280839895013123;

        public static double MetersToFeet(double meters)
        {
            return meters * FeetPerMeter;
        }

        public static double FeetToMeters(double feet)
        {
            return feet / FeetPerMeter;
        }
    }
}
