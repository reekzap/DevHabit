namespace DevHabit.Api.Settings;

public sealed class CorsSettings
{
    public const string PolicyName = "DevHabitCorsPolicy";
    public const string SectionName = "CorsSettings";

    public string[] AllowedOrigins { get; init; } = [];
}
