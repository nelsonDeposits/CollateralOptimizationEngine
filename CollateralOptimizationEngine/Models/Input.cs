namespace CollateralOptimizationEngine;

public class Input
{
    public SecurityInventory SecurityInventory { get; private set; }
    public List<Double>? Haircuts { get; private set; }
    public List<CollateralRequirement>? CollateralRequirements { get; private set; }
    public ConcentrationLimits? ConcentrationLimits { get; private set; }

    public Input(
        SecurityInventory securityInventory,
        List<Double>? haircuts = null,
        List<CollateralRequirement>? collateralRequirements = null,
        ConcentrationLimits? concentrationLimits = null)
    {
        ArgumentNullException.ThrowIfNull(securityInventory);

        SecurityInventory = securityInventory;
        Haircuts = haircuts;
        CollateralRequirements = collateralRequirements;
        ConcentrationLimits = concentrationLimits;
    }
}