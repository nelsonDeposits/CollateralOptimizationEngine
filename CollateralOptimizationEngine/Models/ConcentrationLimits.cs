
namespace CollateralOptimizationEngine;

public class ConcentrationLimits
{
    public List<String> LimitIds { get; private set; }
    public List<String> AssetClasses { get; set; }
    public List<Double> MaxMarketValues { get; private set; }
    
    public ConcentrationLimits(List<String> limitIds, List<String> assetClasses, List<Double> maxMarketValues)
    {
        LimitIds = limitIds;
        AssetClasses = assetClasses;
        MaxMarketValues = maxMarketValues;
    }
}