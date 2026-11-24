namespace CollateralOptimizationEngine;

public class Output
{
    public CollateralRequirement CollateralRequirement { get; private set; }
    public AllocationStatus AllocationStatus { get; private set; }
    public Dictionary<String, Double> Allocations { get; private set; }
    public SecurityInventory UpdatedInventory { get; private set; }
    public Double TotalEconomicCost { get; private set; }

    public Output(Dictionary<String, Double> allocations, CollateralRequirement collateralRequirement, SecurityInventory inventory, AllocationStatus allocationStatus)
    {
        Allocations = allocations;
        UpdatedInventory = inventory;
        CollateralRequirement = collateralRequirement;
        AllocationStatus = allocationStatus;
        
    }

    public void CalculateEconomicCost(List<double> costs)
    {
        List<Double> allocationValues = Allocations.Values.ToList();
        for (int i = 0; i < allocationValues.Count; i++)
        {
            TotalEconomicCost += allocationValues[i] * costs[i];
        }
    }

}
