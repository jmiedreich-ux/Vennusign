namespace Vennu.Api.Services;

public static class HeroDwellSeconds
{
    public const int Default = 8;
    public const int Minimum = 4;
    public const int Maximum = 30;

    public static int Normalize(int? value)
    {
        var normalized = value ?? Default;
        return normalized is >= Minimum and <= Maximum
            ? normalized
            : throw new ArgumentOutOfRangeException(nameof(value), $"Hero dwell must be between {Minimum} and {Maximum} seconds.");
    }
}
