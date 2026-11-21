namespace CollateralOptimizationEngine;

public class Counterparty
{
    public String CounterpartyId { get; private set; }

    public Dictionary<String, Double> EligibleSecurities { get; private set; }
    public Counterparty(String counterpartyId, Dictionary<String, Double> eligibleSecurities)
    {
        CounterpartyId = counterpartyId;
        EligibleSecurities = eligibleSecurities;
    }
    
    
}