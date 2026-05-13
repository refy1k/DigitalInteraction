namespace DigitalInteraction.Helpers;

public static class NumberGenerator
{
    private static int _appealCounter;
    private static int _applicationCounter;

    public static string GenerateAppealNumber() =>
        $"ОБР-{DateTime.Now.Year}-{++_appealCounter:D5}";

    public static string GenerateApplicationNumber() =>
        $"ЗАЯ-{DateTime.Now.Year}-{++_applicationCounter:D5}";
}