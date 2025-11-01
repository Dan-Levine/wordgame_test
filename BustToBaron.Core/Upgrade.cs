using System;

namespace BustToBaron.Core
{
    public enum UpgradeType
    {
        GetUniform,
        ImproveOdds,
        RaiseStakes,
        HighRoller,
        AutoFlipper,
        FasterFlipper,
        AutomateX10,
        AutomateX100,
        AutomateX1000,
        FasterX10Flipper,
        FasterX100Flipper,
        FasterX1000Flipper,
        Inconspicuous,
        GreasePalms,
        BribeFloorManager,
        SleightOfHand,
        BuyDiceTableLicense
    }
    
    public class Upgrade
    {
        public UpgradeType Type { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }
        public string Description { get; set; }
        
        public Upgrade(UpgradeType type, string name, decimal cost, string description)
        {
            Type = type;
            Name = name;
            Cost = cost;
            Description = description;
        }
        
        public static decimal CalculateOddsUpgradeCost(int level)
        {
            // Cost Formula: $1 * (1.2 ^ Level)
            return 1.0m * (decimal)Math.Pow(1.2, level);
        }
    }
}
