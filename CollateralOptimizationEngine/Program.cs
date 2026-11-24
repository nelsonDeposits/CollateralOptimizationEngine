using System;
using System.Text.Json;

namespace CollateralOptimizationEngine;

class Program
{
    static void Main(string[] args)
    {
        OptimizationVersion version = OptimizationVersion.V4;
        
        string rootPath = "/home/noslen/CDI/Projects/CollateralOptimizationEngine/CollateralOptimizationEngine";
        
        string inputFolder = Path.Combine(rootPath, "Inputs");
        Input input = InputLoader.Load(version, inputFolder);
        
        List<Output> outputs = OptimizationEngine.Run(version, input);
        Double totalCost = 0.0;
        foreach (Output output in outputs)
        {
            totalCost += output.TotalEconomicCost;
        }
        
        Console.WriteLine($"Version : {version}, Total Economic Cost: {totalCost}");
        
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(outputs, options);
        
        string outputFolder = Path.Combine(rootPath, "Outputs", version.ToString());
        string outputFile = Path.Combine(outputFolder, $"output_{version}.json");
        File.WriteAllText(outputFile, json);
        

    }

}