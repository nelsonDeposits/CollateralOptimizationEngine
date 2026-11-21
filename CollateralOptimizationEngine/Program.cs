using System;
using System.Text.Json;

namespace CollateralOptimizationEngine;

class Program
{
    static void Main(string[] args)
    {

        string securitiesFilePath =
            "/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Inputs/V2/securities_inventory.csv";
        string collateralReqFilePath =
            "/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Inputs/V2/collateral_requirements.csv";
        string counterpartiesFilePath =
            "/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Inputs/V2/collateral_eligibility.csv";

            
        
        
        SecurityInventory inventory = CsvDataLoader.LoadSecurities(securitiesFilePath);
        List<Counterparty> counterparties = CsvDataLoader.LoadCounterparties(counterpartiesFilePath);
        List<CollateralRequirement> collateralRequirements = CsvDataLoader.LoadRequirements(collateralReqFilePath, counterpartiesFilePath);
        //CollateralRequirement collateral = collateralRequirements.First();

        
        List<Output> outputs = new List<Output>();
        foreach (CollateralRequirement collateral in collateralRequirements)
        {
            Output output = OptimizationEngine.Solver(inventory, collateral);
            outputs.Add(output);
            inventory = output.UpdatedInventory; 
        }

        
        
        
        
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(outputs, options);
        File.WriteAllText("/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Outputs/V2/output_V2.json", json);
        /*
        output = OptimizationEngine.Solver(inventory, collateral);
        options = new JsonSerializerOptions { WriteIndented = true };
        json = JsonSerializer.Serialize(output, options);
        File.WriteAllText("/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Outputs/V1/output_V1.1.json", json);

        output = OptimizationEngine.Solver(inventory, collateral);
        options = new JsonSerializerOptions { WriteIndented = true };
        json = JsonSerializer.Serialize(output, options);
        File.WriteAllText("/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Outputs/V1/output_V1.2.json", json);
        
        output = OptimizationEngine.Solver(inventory, collateral);
        options = new JsonSerializerOptions { WriteIndented = true };
        json = JsonSerializer.Serialize(output, options);
        File.WriteAllText("/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine/Outputs/V1/output_V1.3.json", json);
        */
    }

}