using Google.OrTools.LinearSolver;

namespace CollateralOptimizationEngine;

public class CollateralOptimizerV1 : ICollateralOptimizer
{
    public List<Output>? Solver (Input input)
    {
        List<Output> outputs = new List<Output>();
        SecurityInventory inventory = input.SecurityInventory;
        List<CollateralRequirement> collateralRequirements = input.CollateralRequirements;
        

        foreach (CollateralRequirement collateralRequirement in collateralRequirements)
        {
            Solver solver = Google.OrTools.LinearSolver.Solver.CreateSolver("CBC");

            if (solver == null)
                throw new InvalidOperationException("CBC solver is not available.");

            int durationsDays = collateralRequirement.DurationDays;
            
            
            List<string> securityIds = inventory.GetSecurityIds(); 
            List<double> marketValues = inventory.GetMarketValues(); // Q_i
            List<double> costs = inventory.GetEconomicCosts(durationsDays); // c_i
            List<double> haircuts = input.Haircuts; // h_i

            int n = marketValues.Count;

            if (n == 0)
                outputs.Add(new Output (null, collateralRequirement, inventory, new AllocationStatus("NOT_SATISFIED", collateralRequirement.CollateralValue, 0.0, 0.0)));
            else
            {
                Variable[] x = solver.MakeNumVarArray(n, 0.0, double.PositiveInfinity, "x");
                Objective objective = solver.Objective();
                Constraint collateralConstraint = solver.MakeConstraint(collateralRequirement.CollateralValue,
                    double.PositiveInfinity, "CollateralConstraint");

                for (int i = 0; i < n; i++)
                {
                    objective.SetCoefficient(x[i], costs[i]);
                    collateralConstraint.SetCoefficient(x[i], 1.0 - haircuts[i]);
                    solver.Add(x[i] <= marketValues[i]);
                }

                objective.SetMinimization();

                Solver.ResultStatus resultStatus = solver.Solve();
                List<double> solutions = new List<double>();
                CollateralRequirement allocatedCollateral;
                String status = null;

                // Si une solution est trouvée
                if (resultStatus == Google.OrTools.LinearSolver.Solver.ResultStatus.OPTIMAL ||
                    resultStatus == Google.OrTools.LinearSolver.Solver.ResultStatus.FEASIBLE)
                {
                    for (int i = 0; i < n; i++)
                    {
                        solutions.Add(x[i].SolutionValue());
                    }
                    status = "SATISFIED";
                }
                
                // Sinon, on alloue tout ce qu'il y'a de disponible
                else if (resultStatus == Google.OrTools.LinearSolver.Solver.ResultStatus.INFEASIBLE)
                {
                    solutions = marketValues;
                    status = "PARTIALLY_SATISFIED";
                }
                
                
                Dictionary<String, Double> allocations =
                    securityIds.Zip(solutions, (id, sol) => (id, sol)).ToDictionary(k => k.id, v => v.sol);
                inventory = inventory.UpdateInventory(allocations);
                
                List<double> collateralValues = solutions.Zip(haircuts, (x, y) => x * (1-y)).ToList();
                
                allocatedCollateral = new CollateralRequirement(collateralRequirement.CollateralId, collateralValues.Sum(), durationsDays, null);
                
                AllocationStatus allocationStatus = new AllocationStatus(status, collateralRequirement.CollateralValue, allocatedCollateral.CollateralValue, 
                    collateralRequirement.CollateralValue - allocatedCollateral.CollateralValue);
                    
                outputs.Add(new Output (allocations, collateralRequirement, inventory, allocationStatus));
            }
            
        }
            
        return outputs;
    }
    
}