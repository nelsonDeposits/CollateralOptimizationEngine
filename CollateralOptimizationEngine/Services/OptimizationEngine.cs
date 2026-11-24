namespace CollateralOptimizationEngine;

public class OptimizationEngine
{
    public static List<Output>? Run(OptimizationVersion version, Input input)
    {
        ICollateralOptimizer optimizer = version switch
        {
            OptimizationVersion.V1 => new CollateralOptimizerV1(),
            OptimizationVersion.V2 => new CollateralOptimizerV2(),
            OptimizationVersion.V3 => new CollateralOptimizerV3(),
            OptimizationVersion.V4 => new CollateralOptimizerV4(),

            _ => throw new ArgumentException("Version pas encore implémentée")
        };

        return optimizer.Solver(input);
    }
}