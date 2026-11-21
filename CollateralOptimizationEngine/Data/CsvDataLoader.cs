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
    public static SecurityInventory LoadSecurities(string fileNameCsv)
    {
        CsvConfiguration configuration = new CsvConfiguration(
            CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true
        };

        using StreamReader reader = new StreamReader(fileNameCsv);
        using CsvReader csvReader = new CsvReader(reader, configuration);

        SecurityInventory inventory = new SecurityInventory();

        foreach (var row in csvReader.GetRecords<dynamic>())
        {
            Security security = new Security(
                (string)row.SecurityId,
                (string)row.SecurityType,
                double.Parse((string)row.MarketValue, CultureInfo.InvariantCulture),
                double.Parse((string)row.EconomicCost, CultureInfo.InvariantCulture)
            );
            inventory.AddSecurity(security);
        }

        return inventory;
    }
    
    public static List<Counterparty> LoadCounterparties(string fileNameCsv)
    {
        CsvConfiguration configuration = new CsvConfiguration(
            CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true
        };

        using StreamReader reader = new StreamReader(fileNameCsv);
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
    
    public static List<CollateralRequirement> LoadRequirements(string requirementsFileCsv, string counterpartiesFileCsv)
    {
        List<Counterparty> counterparties = LoadCounterparties(counterpartiesFileCsv);
        
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
            requirements.Add(new CollateralRequirement(
                (string)row.CollateralId,
                double.Parse((string)row.CollateralValue, CultureInfo.InvariantCulture),
                counterparties.First(c=> c.CounterpartyId == (string)row.CounterpartyId),
                int.Parse((string)row.DurationDays, CultureInfo.InvariantCulture))
            );
            
        }
        
        /*List<CollateralRequirement> requirements = csvReader
            .GetRecords<CollateralRequirement>()
            .Select(r => new CollateralRequirement(
                r.CollateralId, r.CollateralValue, r.CounterpartyId, r.DurationDays))
            .ToList();*/

        return requirements;
    }
    
    
}