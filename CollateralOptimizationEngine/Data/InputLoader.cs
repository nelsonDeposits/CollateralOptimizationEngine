namespace CollateralOptimizationEngine;

public class InputLoader
{
    public static  Input Load(OptimizationVersion version, string rootPath)
    {
        string folderPath = Path.Combine(rootPath, version.ToString());
        
        string securitiesFileCsv = Path.Combine(folderPath, "securities_inventory.csv");
        SecurityInventory securityInventory = CsvDataLoader.LoadSecurities(securitiesFileCsv);

        switch (version)
        {
            case OptimizationVersion.V1:
            {
                string requirementsFileCsv = Path.Combine(folderPath, "collateral_requirements.csv");

                List<CollateralRequirement> collateralRequirements = CsvDataLoader.LoadRequirements(requirementsFileCsv);
                List<double> haircuts = CsvDataLoader.LoadHaircuts(securitiesFileCsv);

                return new Input(securityInventory, haircuts: haircuts, collateralRequirements: collateralRequirements);
            }

            case OptimizationVersion.V2:
            {
                string requirementsFileCsv = Path.Combine(folderPath, "collateral_requirements.csv");
                string counterpartiesFileCsv = Path.Combine(folderPath, "collateral_eligibility.csv");

                List<CollateralRequirement> collateralRequirements = CsvDataLoader.LoadRequirements(requirementsFileCsv, counterpartiesFileCsv);

                return new Input(securityInventory, collateralRequirements: collateralRequirements);
            }

            case OptimizationVersion.V3 or OptimizationVersion.V4:
            {
                string requirementsFileCsv = Path.Combine(folderPath, "collateral_requirements.csv");
                string counterpartiesFileCsv = Path.Combine(folderPath, "collateral_eligibility.csv");
                string concentrationLimitsFileCsv = Path.Combine(folderPath, "concentration_limits.csv");

                List<CollateralRequirement> collateralRequirements = CsvDataLoader.LoadRequirements(requirementsFileCsv, counterpartiesFileCsv);
                ConcentrationLimits concentrationLimits = CsvDataLoader.LoadConcentrationLimits(concentrationLimitsFileCsv);

                return new Input(securityInventory, collateralRequirements: collateralRequirements, concentrationLimits: concentrationLimits);
            }

            default:
                throw new ArgumentException(
                    "Version d'optimisation inconnue.",
                    nameof(version)
                );
        }
    }
}
