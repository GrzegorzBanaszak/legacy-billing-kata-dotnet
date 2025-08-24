record Root
{
    public string BillingPeriod { get; init; } = string.Empty;
    public List<Customer> Customers { get; init; } = new();
}
