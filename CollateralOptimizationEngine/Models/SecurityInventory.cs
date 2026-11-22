namespace CollateralOptimizationEngine;

public class SecurityInventory
{
    public List<Security> Securities  { get; private set; }

    public SecurityInventory()
    {
        Securities = new List<Security>();
    }
    public SecurityInventory(List<Security> securities)
    {
        Securities = securities;
    }
    public void AddSecurity(Security security)
    {
        Securities.Add(security);
    }

    public Double GetTotalMarketValue()
    {
        return Securities.Sum(s => s.MarketValue);
    }
    public List<double> GetMarketValues()
    {
        return Securities.Select(s => s.MarketValue).ToList();
    }
    public List<String> GetSecurityIds()
    {
        return Securities.Select(s => s.SecurityId).ToList();
    }
    public List<String> GetAssetClasses()
    {
        return Securities.Select(s => s.AssetClass).ToList();
    }
    public List<double> GetEconomicCosts(int durationsDays)
    {
        return Securities.Select(s => s.EconomicCost * durationsDays / 360.0).ToList();
    }
    
    public List<double> GetCollateralValues()
    {
        //return Securities.Select(s => s.ComputeCollateral()).ToList();
        return null;
    }
    

    public SecurityInventory UpdateInventory(Dictionary<string, double> allocations)
    {
        List<Security> updated = new List<Security>();

        foreach (Security s in Securities)
        {
            double newValue = s.MarketValue;

            if (allocations.TryGetValue(s.SecurityId, out double alloc))
                newValue -= alloc;
                updated.Add(new Security(s.SecurityId, s.SecurityType, newValue, s.AssetClass,s.EconomicCost));
        }

        return new SecurityInventory(updated);
    }
}