using Google.OrTools.LinearSolver;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace CollateralOptimizationEngine;

public class OptimizationEngine
{
    public static Output? Solver(SecurityInventory inventory, CollateralRequirement collateralRequirement)
    {
        Solver solver = Google.OrTools.LinearSolver.Solver.CreateSolver("CBC");

        if (solver == null)
            throw new InvalidOperationException("CBC solver is not available.");

        int durationsDays = collateralRequirement.DurationDays;
        Counterparty counterparty = collateralRequirement.Counterparty;
        
        SecurityInventory eligibleInventory = new SecurityInventory(inventory.Securities
            .Where(s => counterparty.EligibleSecurities.ContainsKey(s.SecurityId))
            .ToList());
        
        List<string> securityIds = eligibleInventory.GetSecurityIds(); 
        List<double> marketValues = eligibleInventory.GetMarketValues(); // Q_i
        
        /*List<double> marketValues = inventory.Securities.Where(s => securityIds.Contains(s.SecurityId)).Select(s => s.MarketValue).ToList(); // Q_i

        Dictionary<String, Double> marketValues = securityIds.Zip(inventory.Securities.Where(s => securityIds.Contains(s.SecurityId)).Select(s => s.MarketValue).ToList(), (id, sol) => (id, sol)).ToDictionary(k => k.id, v => v.sol);

        Dictionary<string, double> marketValues = inventory.Securities
            .Where(s => counterparty.EligibleSecurities.ContainsKey(s.SecurityId))
            .ToDictionary(s => s.SecurityId, s => s.MarketValue);*/
        
        List<double> costs = eligibleInventory.GetEconomicCosts(durationsDays); // c_i
        List<double> haircuts = counterparty.EligibleSecurities.Values.ToList(); // h_i

        int n = marketValues.Count;

        if (n == 0)
            return new Output (null, collateralRequirement, inventory, new AllocationStatus("NOT_SATISFIED", collateralRequirement.CollateralValue, 0.0, 0.0));
        
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
        
        allocatedCollateral = new CollateralRequirement(collateralRequirement.CollateralId, collateralValues.Sum(), null, durationsDays);
        
        AllocationStatus allocationStatus = new AllocationStatus(status, collateralRequirement.CollateralValue, allocatedCollateral.CollateralValue, 
            collateralRequirement.CollateralValue - allocatedCollateral.CollateralValue);
            
        return new Output (allocations, collateralRequirement, inventory, allocationStatus);
    }
}