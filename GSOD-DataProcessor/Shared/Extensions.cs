namespace GSOD_DataProcessor.Shared;

internal static class Extensions
{
    internal static double ConvertMissing(this double value)
    {
        double[] missingValues = { 99.99, 999.9, 9999.9 };
        return missingValues.Contains(value) ? value : 0;
    }
}
