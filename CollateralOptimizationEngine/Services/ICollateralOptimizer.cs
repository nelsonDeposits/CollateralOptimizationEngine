namespace CollateralOptimizationEngine;

public interface ICollateralOptimizer
{
    List<Output>? Solver (Input input);
}