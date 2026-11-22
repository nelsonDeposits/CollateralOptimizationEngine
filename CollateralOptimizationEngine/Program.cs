using System;
using System.Text.Json;

namespace CollateralOptimizationEngine;

class Program
{
    static void Main(string[] args)
    {

        string securitiesFilePath =
            "/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Inputs/V3/securities_inventory.csv";
        string collateralReqFilePath =
            "/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Inputs/V3/collateral_requirements.csv";
        string counterpartiesFilePath =
            "/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Inputs/V3/collateral_eligibility.csv";
        string concentrationLimitsFileCsv =
            "/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Inputs/V3/concentration_limits.csv";

            
        
        
        SecurityInventory inventory = CsvDataLoader.LoadSecurities(securitiesFilePath);
        
        List<CollateralRequirement> collateralRequirements = CsvDataLoader.LoadRequirements(collateralReqFilePath, counterpartiesFilePath);
        //CollateralRequirement collateral = collateralRequirements.First();
        ConcentrationLimits concentrationLimits = CsvDataLoader.LoadConcentrationLimits(concentrationLimitsFileCsv);

        
        List<Output> outputs = new List<Output>();
        foreach (CollateralRequirement collateral in collateralRequirements)
        {
            Output output = OptimizationEngine.Solver(inventory, collateral, concentrationLimits);
            outputs.Add(output);
            inventory = output.UpdatedInventory; 
        }

        
        
        
        //Output output = OptimizationEngine.Solver(inventory, collateral, concentrationLimits);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(outputs, options);
        File.WriteAllText("/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Outputs/V3/output_V3.json", json);
        
    }

}