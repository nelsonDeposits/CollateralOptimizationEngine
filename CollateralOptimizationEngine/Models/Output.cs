namespace CollateralOptimizationEngine;

public class Output
{
    public CollateralRequirement CollateralRequirement { get; private set; }
    public AllocationStatus AllocationStatus { get; private set; }
    public Dictionary<String, Double> Allocations { get; private set; }
    public SecurityInventory UpdatedInventory { get; private set; }

    public Output(Dictionary<String, Double> allocations, CollateralRequirement collateralRequirement, SecurityInventory inventory, AllocationStatus allocationStatus)
    {
        Allocations = allocations;
        UpdatedInventory = inventory;
        CollateralRequirement = collateralRequirement;
        AllocationStatus = allocationStatus;
    }

}
