using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace CollateralOptimizationEngine;

internal static class CsvDataLoader
{
    public static SecurityInventory LoadSecurities(string securitiesFileCsv)
    {
        CsvConfiguration configuration = new CsvConfiguration(
            CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true
        };

        using StreamReader reader = new StreamReader(securitiesFileCsv);
        using CsvReader csvReader = new CsvReader(reader, configuration);
        
        csvReader.Read();
        csvReader.ReadHeader();

        SecurityInventory inventory = new SecurityInventory();
        
        bool hasAssetClassColumn = csvReader.HeaderRecord?.Contains("AssetClass", StringComparer.OrdinalIgnoreCase) == true;

        foreach (var row in csvReader.GetRecords<dynamic>())
        { 
            String assetClass = null;
            if (hasAssetClassColumn)
            {
                assetClass = (string)row.AssetClass;
            }
            Security security = new Security(
                (string)row.SecurityId,
                (string)row.SecurityType,
                double.Parse((string)row.MarketValue, CultureInfo.InvariantCulture),
                double.Parse((string)row.EconomicCost, CultureInfo.InvariantCulture),
                assetClass);
            inventory.AddSecurity(security);
        }

        return inventory;
    }
    
    public static List<Double> LoadHaircuts(string securitiesFileCsv)
    {
        CsvConfiguration configuration = new CsvConfiguration(
            CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true
        };

        using StreamReader reader = new StreamReader(securitiesFileCsv);
        using CsvReader csvReader = new CsvReader(reader, configuration);

        List<Double> haircuts = new List<Double>();

        foreach (var row in csvReader.GetRecords<dynamic>())
        {
            
            haircuts.Add(double.Parse((string)row.Haircut, CultureInfo.InvariantCulture));
        }

        return haircuts;
    }
    
    public static List<Counterparty> LoadCounterparties(string counterpartiesFileCsv)
    {
        CsvConfiguration configuration = new CsvConfiguration(
            CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true
        };

        using StreamReader reader = new StreamReader(counterpartiesFileCsv);
        using CsvReader csvReader = new CsvReader(reader, configuration);
        
        List<Counterparty> counterparties = csvReader.GetRecords<dynamic>()
            .GroupBy(row => (string)row.CounterpartyId)
            .Select(grp => new Counterparty(
                grp.Key,
                grp.Where(row => bool.Parse((string)row.IsEligible))
                    .ToDictionary(
                        row => (string)row.SecurityId,
                        row => double.Parse((string)row.Haircut, CultureInfo.InvariantCulture))))
            .ToList();
       
        return counterparties;
    }
    
    public static List<CollateralRequirement> LoadRequirements(string requirementsFileCsv, string? counterpartiesFileCsv = null)
    {
        List<Counterparty>? counterparties = counterpartiesFileCsv is not null
            ? LoadCounterparties(counterpartiesFileCsv)
            : null;
        
        
        CsvConfiguration configuration = new CsvConfiguration(
            CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true
        };

        using StreamReader reader = new StreamReader(requirementsFileCsv);
        using CsvReader csvReader = new CsvReader(reader, configuration);

        List<CollateralRequirement> requirements = new List<CollateralRequirement>();

        foreach (var row in csvReader.GetRecords<dynamic>())
        {
            Counterparty counterparty = null;
            if (counterparties is not null)
            {
                string counterpartyId = (string)row.CounterpartyId;
                counterparty = counterparties?.FirstOrDefault(
                    c => c.CounterpartyId == counterpartyId);
            }
            
            
            requirements.Add(new CollateralRequirement(
                (string)row.CollateralId,
                double.Parse((string)row.CollateralValue, CultureInfo.InvariantCulture),
                int.Parse((string)row.DurationDays, CultureInfo.InvariantCulture), counterparty)
            );
            
        }

        return requirements;
    }
    
    public static ConcentrationLimits LoadConcentrationLimits(string concentrationLimitsFileCsv)
    {
        CsvConfiguration configuration = new CsvConfiguration(
            CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true
        };

        using StreamReader reader = new StreamReader(concentrationLimitsFileCsv);
        using CsvReader csvReader = new CsvReader(reader, configuration);
        csvReader.Read();
        csvReader.ReadHeader();
        
        var limitIds = new List<string>();
        var assetClasses = new List<string>();
        var maxMarketValues = new List<double>();

        while (csvReader.Read())
        {
            limitIds.Add(csvReader.GetField("LimitId")!);
            assetClasses.Add(csvReader.GetField("AssetClass")!);
            maxMarketValues.Add(csvReader.GetField<double>("MaxMarketValue"));
        }

        return new ConcentrationLimits(limitIds, assetClasses, maxMarketValues);
    }

    
}