namespace CollateralOptimizationEngine;

public class AllocationStatus
{ 
    public String Status { get; private set; }
    public double RequiredCollateral { get; private set; }
    public double AllocatedCollateral { get; private set; }
    public double RemainingRequirement { get; private set; }

    public AllocationStatus(String status, double requiredCollateral, double allocatedCollateral, double collateralRequirement)
    {
    Status = status;
    RequiredCollateral = requiredCollateral;
    AllocatedCollateral = allocatedCollateral;
    RemainingRequirement = collateralRequirement;
    }

}
