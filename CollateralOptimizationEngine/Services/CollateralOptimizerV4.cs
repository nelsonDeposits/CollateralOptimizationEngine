using Google.OrTools.LinearSolver;

namespace CollateralOptimizationEngine;

public class CollateralOptimizerV4 : ICollateralOptimizer
{
    public List<Output>? Solver(Input input)
    {
        SecurityInventory inventory = input.SecurityInventory;
        List<CollateralRequirement> collateralRequirements = input.CollateralRequirements;
        ConcentrationLimits concentrationLimits = input.ConcentrationLimits;

        int n = inventory.Securities.Count;
        int m = collateralRequirements.Count;

        List<string> securityIds = inventory.GetSecurityIds();
        List<string> assetClasses = inventory.GetAssetClasses();
        List<double> marketValues = inventory.GetMarketValues();
        
        Solver solver = Google.OrTools.LinearSolver.Solver.CreateSolver("CBC");

        if (solver == null)
            throw new InvalidOperationException("CBC solver is not available.");

        Variable[,] x = new Variable[n, m];
        Double[,] haircuts = new Double[n, m];
        Double[,] costs = new Double[n, m];

        Objective objective = solver.Objective();

        for (int j = 0; j < m; j++)
        {
            CollateralRequirement requirement = collateralRequirements[j];
            Counterparty counterparty = requirement.Counterparty;

            List<double> requirementCosts = inventory.GetEconomicCosts(requirement.DurationDays);

            for (int i = 0; i < n; i++)
            {
                bool eligible = counterparty.EligibleSecurities.TryGetValue(securityIds[i], out double haircut);
                haircuts[i, j] = eligible ? haircut : 0.0;
                costs[i, j] = requirementCosts[i];

                double upperBound = eligible ? marketValues[i] : 0.0;
                x[i, j] = solver.MakeNumVar(0.0, upperBound, $"x_{i}_{j}");
                
                objective.SetCoefficient(x[i, j], costs[i, j]);
            }
        }

        for (int j = 0; j < m; j++)
        {
            CollateralRequirement collateralRequirement = collateralRequirements[j];
            Constraint collateralConstraint = solver.MakeConstraint(collateralRequirement.CollateralValue,
                double.PositiveInfinity,  $"CollateralConstraint_{j}");

            for (int i = 0; i < n; i++)
            {
                collateralConstraint.SetCoefficient(x[i, j], 1.0 - haircuts[i, j]);
            }
        }

        for (int i = 0; i < n; i++)
        {
            Constraint inventoryConstraint = solver.MakeConstraint(0.0,marketValues[i], $"InventoryConstraint_{i}");
            for (int j = 0; j < m; j++)
            {
                inventoryConstraint.SetCoefficient(x[i, j], 1.0);
            }
        }

        // 6. Contraintes de concentration
        foreach (var (assetClass, maxValue) in concentrationLimits.AssetClasses.Zip(concentrationLimits.MaxMarketValues))
        {
            for (int j = 0; j < m; j++)
            {
                Constraint concentrationConstraint = solver.MakeConstraint(0.0,maxValue,$"ConcentrationConstraint_{assetClass}_{j}");
                for (int i = 0; i < n; i++)
                {
                    if (assetClasses[i] == assetClass)
                    {
                        concentrationConstraint.SetCoefficient(x[i, j],1.0);
                    }
                }
            }
        }

        objective.SetMinimization();

        Solver.ResultStatus resultStatus = solver.Solve();
        List<Output> outputs = new List<Output>();
        Output output;

        if (resultStatus == Google.OrTools.LinearSolver.Solver.ResultStatus.OPTIMAL ||
            resultStatus == Google.OrTools.LinearSolver.Solver.ResultStatus.FEASIBLE)
        {
            Dictionary<string, double> totalAllocations = securityIds.ToDictionary(id => id, id => 0.0);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    double allocation = x[i, j].SolutionValue();
                    totalAllocations[securityIds[i]] += allocation;
                }
            }

            SecurityInventory updatedInventory =  inventory.UpdateInventory(totalAllocations);

            for (int j = 0; j < m; j++)
            {
                CollateralRequirement requirement = collateralRequirements[j];
                Dictionary<string, double> allocations = new Dictionary<string, double>();
                double allocatedCollateralValue = 0.0;

                for (int i = 0; i < n; i++)
                {
                    double allocation = x[i, j].SolutionValue();
                    allocations[securityIds[i]] = allocation;
                    allocatedCollateralValue += allocation * (1.0 - haircuts[i, j]);
                }
                
                List<Double> economicCost = Enumerable
                    .Range(0, costs.GetLength(0))
                    .Select(i => costs[i, j])
                    .ToList();

                AllocationStatus allocationStatus = new AllocationStatus("SATISFIED", requirement.CollateralValue, allocatedCollateralValue,requirement.CollateralValue - allocatedCollateralValue);
                output = new Output(allocations, requirement, inventory, allocationStatus);
                output.CalculateEconomicCost(economicCost);
                outputs.Add(output);
            }
        }
        else
        {
            foreach (CollateralRequirement requirement in collateralRequirements)
            {
                Dictionary<string, double> allocations = securityIds.ToDictionary(id => id, id => 0.0);
                AllocationStatus allocationStatus = new AllocationStatus("NOT_SATISFIED", requirement.CollateralValue, 0.0, requirement.CollateralValue);
                output = new Output(allocations, requirement, inventory, allocationStatus);
                //output.CalculateEconomicCost(costs);
                outputs.Add(output);
            }
        }

        return outputs;
    }
}