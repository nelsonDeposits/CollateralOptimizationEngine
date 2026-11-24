namespace CollateralOptimizationEngine;

public class CollateralRequirement
{
    public String CollateralId { get; private set; }
    public Double CollateralValue { get; private set; }
    public Counterparty Counterparty { get; private set; }
    public String Currency { get; private set; }
    public int DurationDays { get; private set; }
    

    public CollateralRequirement(String collateralId, Double collateralValue, int durationDays, Counterparty? counterparty = null)
    {
        CollateralId = collateralId;
        CollateralValue = collateralValue;
        Counterparty = counterparty;
        Currency = "EUR";
        DurationDays = durationDays;
    }
}