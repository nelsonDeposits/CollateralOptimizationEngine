namespace CollateralOptimizationEngine;

public class Security
{
    public String SecurityId { get; private set; }
    public String SecurityType { get; private set; }
    public Double MarketValue { get; set; }
    public String Currency { get; private set; }
    public Double EconomicCost { get; set; }

    public Security(String id, String type, Double marketValue,  Double economicCost)
    {
        SecurityId = id;    
        SecurityType = type;
        MarketValue = marketValue;
        Currency = "EUR";
        EconomicCost = economicCost;
    }

    public Double ComputeCollateral(double haircut)
    {
        return MarketValue * (1 - haircut);
    }
}