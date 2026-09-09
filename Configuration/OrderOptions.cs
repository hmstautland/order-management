namespace OrderManagement.Api.Configuration;

public sealed class OrderOptions
{
    public const string SectionName = "Orders";

    public int ProcessingDelayMilliseconds { get; init; } = 100;
}
