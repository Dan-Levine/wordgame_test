using System;

namespace BustToBaron.Core
{
    public class GameState
    {
        public decimal Cash { get; set; } = 0;
        public decimal Heat { get; set; } = 0;
        public decimal MaxHeat { get; set; } = 1000;
        
        // Grind system
        public bool HasUniform { get; set; } = false;
        public decimal WorkShiftValue => HasUniform ? 1.00m : 0.50m;
        
        // Coin Flip system
        public int WinPercentage { get; set; } = 45;
        public int OddsUpgradeLevel { get; set; } = 0;
        public bool HasRaiseStakes { get; set; } = false;
        public bool HasHighRoller { get; set; } = false;
        
        // Automation system
        public bool HasAutoFlipper { get; set; } = false;
        public bool AutoFlipEnabled { get; set; } = false;
        public int AutomationSpeed { get; set; } = 1; // 1 = 1x/sec, 2 = 2x/sec
        public int AutomationStakeMultiplier { get; set; } = 1; // 1, 10, 100, 1000
        
        // Heat management upgrades
        public decimal HeatCooldownRate { get; set; } = -1.5m;
        public bool HasInconspicuous { get; set; } = false;
        public bool HasGreasePalms { get; set; } = false;
        public bool HasBribeFloorManager { get; set; } = false;
        public bool HasSleightOfHand { get; set; } = false;
        
        // Win condition
        public bool HasDiceTableLicense { get; set; } = false;
        
        public void UpdateHeatCooldown()
        {
            HeatCooldownRate = -1.5m;
            if (HasInconspicuous)
                HeatCooldownRate = -2.0m;
            if (HasGreasePalms)
                HeatCooldownRate = -3.0m;
        }
        
        public void UpdateMaxHeat()
        {
            MaxHeat = HasBribeFloorManager ? 1500 : 1000;
        }
        
        public decimal CalculateHeatGeneration()
        {
            if (!AutoFlipEnabled)
                return 0;
            
            // OddsHps = 0.1 * (Win % - 50)
            decimal oddsHps = 0.1m * (WinPercentage - 50);
            
            // SpeedMultiplier: 1.0 for 1x/sec, 2.0 for 2x/sec
            decimal speedMultiplier = AutomationSpeed == 1 ? 1.0m : 2.0m;
            
            // StakeMultiplier: 1.0 (x1), 1.5 (x10), 2.0 (x100), 2.5 (x1000)
            decimal stakeMultiplier = AutomationStakeMultiplier switch
            {
                1 => 1.0m,
                10 => 1.5m,
                100 => 2.0m,
                1000 => 2.5m,
                _ => 1.0m
            };
            
            // Hps = (1.0 + OddsHps) * SpeedMultiplier * StakeMultiplier
            decimal hps = (1.0m + oddsHps) * speedMultiplier * stakeMultiplier;
            
            // Sleight of Hand: -10% to all heat generation
            if (HasSleightOfHand)
                hps *= 0.9m;
            
            return hps;
        }
        
        public decimal CalculateCoinFlipEV(decimal betAmount)
        {
            // EV = (Win% * WinAmount) + ((1 - Win%) * -BetAmount)
            // WinAmount = betAmount (2x payout)
            decimal winAmount = betAmount;
            decimal lossAmount = -betAmount;
            
            decimal winProb = WinPercentage / 100.0m;
            decimal lossProb = 1 - winProb;
            
            return (winProb * winAmount) + (lossProb * lossAmount);
        }
    }
}
