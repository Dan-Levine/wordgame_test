using System;
using System.Collections.Generic;
using System.Linq;

namespace BustToBaron.Core
{
    public class GameEngine
    {
        private readonly Random _random;
        private DateTime _lastUpdateTime;
        
        public GameState State { get; }
        
        public event Action<string> OnCashChanged;
        public event Action<string> OnHeatChanged;
        public event Action<string> OnMessage;
        public event Action OnCaught;
        
        public GameEngine()
        {
            State = new GameState();
            _random = new Random();
            _lastUpdateTime = DateTime.Now;
        }
        
        // Grind System
        public bool WorkShift()
        {
            State.Cash += State.WorkShiftValue;
            OnCashChanged?.Invoke(FormatCash(State.Cash));
            return true;
        }
        
        public bool PurchaseGetUniform()
        {
            if (State.HasUniform)
                return false;
            
            if (State.Cash < 50)
                return false;
            
            State.Cash -= 50;
            State.HasUniform = true;
            OnCashChanged?.Invoke(FormatCash(State.Cash));
            OnMessage?.Invoke("You got a uniform! Work Shift is now worth $1.00 per click.");
            return true;
        }
        
        // Coin Flip Manual System
        public bool ManualBet(decimal betAmount)
        {
            // Check if bet is valid
            if (State.Cash < betAmount)
                return false;
            
            // Check if bet level is unlocked
            if (betAmount == 100 && !State.HasRaiseStakes)
                return false;
            if (betAmount == 1000 && !State.HasHighRoller)
                return false;
            
            // Place bet
            State.Cash -= betAmount;
            
            // Determine win/loss (45% base, +1% per upgrade)
            int winChance = State.WinPercentage;
            bool won = _random.Next(100) < winChance;
            
            if (won)
            {
                decimal winnings = betAmount * 2;
                State.Cash += winnings;
                OnMessage?.Invoke($"You won ${winnings:F2}!");
            }
            else
            {
                OnMessage?.Invoke($"You lost ${betAmount:F2}.");
            }
            
            OnCashChanged?.Invoke(FormatCash(State.Cash));
            return true;
        }
        
        public bool PurchaseImproveOdds()
        {
            // Max 80% win chance (level 35, 45% + 35%)
            if (State.WinPercentage >= 80)
                return false;
            
            decimal cost = Upgrade.CalculateOddsUpgradeCost(State.OddsUpgradeLevel);
            
            if (State.Cash < cost)
                return false;
            
            State.Cash -= cost;
            State.OddsUpgradeLevel++;
            State.WinPercentage++;
            OnCashChanged?.Invoke(FormatCash(State.Cash));
            OnMessage?.Invoke($"Odds improved! Win chance is now {State.WinPercentage}%.");
            return true;
        }
        
        // Automation System
        public bool ToggleAutoFlip()
        {
            if (!State.HasAutoFlipper)
                return false;
            
            State.AutoFlipEnabled = !State.AutoFlipEnabled;
            OnMessage?.Invoke($"Auto-Flip is now {(State.AutoFlipEnabled ? "ON" : "OFF")}.");
            return true;
        }
        
        public void ProcessAutomation(TimeSpan deltaTime)
        {
            if (!State.AutoFlipEnabled)
                return;
            
            // Calculate how many bets to process based on speed
            double betsPerSecond = State.AutomationSpeed;
            double betInterval = 1.0 / betsPerSecond;
            int betCount = (int)(deltaTime.TotalSeconds / betInterval);
            
            decimal betAmount = GetBaseBetAmount() * State.AutomationStakeMultiplier;
            
            for (int i = 0; i < betCount; i++)
            {
                if (State.Cash < betAmount)
                {
                    State.AutoFlipEnabled = false;
                    OnMessage?.Invoke("Auto-Flip disabled: Not enough cash!");
                    return;
                }
                
                ProcessAutomatedBet(betAmount);
            }
        }
        
        private void ProcessAutomatedBet(decimal betAmount)
        {
            State.Cash -= betAmount;
            
            int winChance = State.WinPercentage;
            bool won = _random.Next(100) < winChance;
            
            if (won)
            {
                decimal winnings = betAmount * 2;
                State.Cash += winnings;
            }
        }
        
        private decimal GetBaseBetAmount()
        {
            return 10m; // Base bet is $10
        }
        
        // Heat System
        public void UpdateHeat(TimeSpan deltaTime)
        {
            // Calculate heat generation
            decimal heatGeneration = State.CalculateHeatGeneration();
            
            // Apply cooldown
            decimal netHeatChange = heatGeneration + State.HeatCooldownRate;
            
            // Update heat per second
            State.Heat += netHeatChange * (decimal)deltaTime.TotalSeconds;
            
            // Clamp heat
            if (State.Heat < 0)
                State.Heat = 0;
            
            // Check for penalty
            if (State.Heat >= State.MaxHeat)
            {
                TriggerCaughtPenalty();
            }
            
            OnHeatChanged?.Invoke(FormatHeat(State.Heat, State.MaxHeat));
        }
        
        private void TriggerCaughtPenalty()
        {
            // Lose 50% of cash
            State.Cash *= 0.5m;
            
            // Reset heat
            State.Heat = 0;
            
            // Turn off automation
            State.AutoFlipEnabled = false;
            
            OnCaught?.Invoke();
            OnMessage?.Invoke("CAUGHT! You lost 50% of your cash, heat reset, and automation disabled.");
            OnCashChanged?.Invoke(FormatCash(State.Cash));
            OnHeatChanged?.Invoke(FormatHeat(State.Heat, State.MaxHeat));
        }
        
        // Upgrade System
        public bool PurchaseUpgrade(UpgradeType upgradeType)
        {
            var upgrade = GetUpgrade(upgradeType);
            if (upgrade == null)
                return false;
            
            if (State.Cash < upgrade.Cost)
                return false;
            
            if (IsUpgradeAlreadyPurchased(upgradeType))
                return false;
            
            State.Cash -= upgrade.Cost;
            
            ApplyUpgrade(upgradeType);
            
            OnCashChanged?.Invoke(FormatCash(State.Cash));
            OnMessage?.Invoke($"Purchased: {upgrade.Name}");
            
            return true;
        }
        
        private bool IsUpgradeAlreadyPurchased(UpgradeType upgradeType)
        {
            return upgradeType switch
            {
                UpgradeType.GetUniform => State.HasUniform,
                UpgradeType.RaiseStakes => State.HasRaiseStakes,
                UpgradeType.HighRoller => State.HasHighRoller,
                UpgradeType.AutoFlipper => State.HasAutoFlipper,
                UpgradeType.FasterFlipper => State.AutomationSpeed >= 2 && State.AutomationStakeMultiplier == 1,
                UpgradeType.AutomateX10 => State.AutomationStakeMultiplier >= 10 && State.AutomationSpeed == 1,
                UpgradeType.AutomateX100 => State.AutomationStakeMultiplier >= 100 && State.AutomationSpeed == 1,
                UpgradeType.AutomateX1000 => State.AutomationStakeMultiplier >= 1000 && State.AutomationSpeed == 1,
                UpgradeType.FasterX10Flipper => State.AutomationSpeed >= 2 && State.AutomationStakeMultiplier >= 10,
                UpgradeType.FasterX100Flipper => State.AutomationSpeed >= 2 && State.AutomationStakeMultiplier >= 100,
                UpgradeType.FasterX1000Flipper => State.AutomationSpeed >= 2 && State.AutomationStakeMultiplier >= 1000,
                UpgradeType.Inconspicuous => State.HasInconspicuous,
                UpgradeType.GreasePalms => State.HasGreasePalms,
                UpgradeType.BribeFloorManager => State.HasBribeFloorManager,
                UpgradeType.SleightOfHand => State.HasSleightOfHand,
                UpgradeType.BuyDiceTableLicense => State.HasDiceTableLicense,
                _ => false
            };
        }
        
        private void ApplyUpgrade(UpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case UpgradeType.GetUniform:
                    State.HasUniform = true;
                    break;
                case UpgradeType.RaiseStakes:
                    State.HasRaiseStakes = true;
                    break;
                case UpgradeType.HighRoller:
                    State.HasHighRoller = true;
                    break;
                case UpgradeType.AutoFlipper:
                    State.HasAutoFlipper = true;
                    break;
                case UpgradeType.FasterFlipper:
                    State.AutomationSpeed = 2;
                    break;
                case UpgradeType.AutomateX10:
                    State.AutomationStakeMultiplier = 10;
                    break;
                case UpgradeType.AutomateX100:
                    State.AutomationStakeMultiplier = 100;
                    break;
                case UpgradeType.AutomateX1000:
                    State.AutomationStakeMultiplier = 1000;
                    break;
                case UpgradeType.FasterX10Flipper:
                    State.AutomationSpeed = 2;
                    State.AutomationStakeMultiplier = 10;
                    break;
                case UpgradeType.FasterX100Flipper:
                    State.AutomationSpeed = 2;
                    State.AutomationStakeMultiplier = 100;
                    break;
                case UpgradeType.FasterX1000Flipper:
                    State.AutomationSpeed = 2;
                    State.AutomationStakeMultiplier = 1000;
                    break;
                case UpgradeType.Inconspicuous:
                    State.HasInconspicuous = true;
                    State.UpdateHeatCooldown();
                    break;
                case UpgradeType.GreasePalms:
                    State.HasGreasePalms = true;
                    State.UpdateHeatCooldown();
                    break;
                case UpgradeType.BribeFloorManager:
                    State.HasBribeFloorManager = true;
                    State.UpdateMaxHeat();
                    break;
                case UpgradeType.SleightOfHand:
                    State.HasSleightOfHand = true;
                    break;
                case UpgradeType.BuyDiceTableLicense:
                    State.HasDiceTableLicense = true;
                    break;
            }
        }
        
        private Upgrade GetUpgrade(UpgradeType upgradeType)
        {
            return upgradeType switch
            {
                UpgradeType.GetUniform => new Upgrade(UpgradeType.GetUniform, "Get a Uniform", 50, "Sets Work Shift value to $1.00"),
                UpgradeType.ImproveOdds => new Upgrade(UpgradeType.ImproveOdds, "Improve Odds (+1%)", Upgrade.CalculateOddsUpgradeCost(State.OddsUpgradeLevel), "Increases Win % by 1"),
                UpgradeType.RaiseStakes => new Upgrade(UpgradeType.RaiseStakes, "Raise the Stakes (x10)", 5000, "Unlocks manual x10 betting"),
                UpgradeType.HighRoller => new Upgrade(UpgradeType.HighRoller, "High Roller (x100)", 50000, "Unlocks manual x100 betting"),
                UpgradeType.AutoFlipper => new Upgrade(UpgradeType.AutoFlipper, "Auto-Flipper (1x/s @ x1)", 2000, "Enables Auto-Flip toggle"),
                UpgradeType.FasterFlipper => new Upgrade(UpgradeType.FasterFlipper, "Faster Flipper (2x/s @ x1)", 25000, "Doubles automation speed"),
                UpgradeType.AutomateX10 => new Upgrade(UpgradeType.AutomateX10, "Automate x10 Bets (1x/s @ x10)", 100000, "Automates x10 betting"),
                UpgradeType.AutomateX100 => new Upgrade(UpgradeType.AutomateX100, "Automate x100 Bets", 216000, "Automates x100 betting"),
                UpgradeType.AutomateX1000 => new Upgrade(UpgradeType.AutomateX1000, "Automate x1000 Bets", 8640000, "Automates x1000 betting"),
                UpgradeType.FasterX10Flipper => new Upgrade(UpgradeType.FasterX10Flipper, "Faster x10 Flipper", 54000, "Doubles x10 automation speed"),
                UpgradeType.FasterX100Flipper => new Upgrade(UpgradeType.FasterX100Flipper, "Faster x100 Flipper", 1000000, "Doubles x100 automation speed"),
                UpgradeType.FasterX1000Flipper => new Upgrade(UpgradeType.FasterX1000Flipper, "Faster x1000 Flipper", 86400000, "Doubles x1000 automation speed"),
                UpgradeType.Inconspicuous => new Upgrade(UpgradeType.Inconspicuous, "Inconspicuous", 50000, "Base Cooldown -> -2.0 Hps"),
                UpgradeType.GreasePalms => new Upgrade(UpgradeType.GreasePalms, "Grease the Palms", 750000, "Base Cooldown -> -3.0 Hps"),
                UpgradeType.BribeFloorManager => new Upgrade(UpgradeType.BribeFloorManager, "Bribe Floor Manager", 5000000, "Max Heat Bar -> 1,500"),
                UpgradeType.SleightOfHand => new Upgrade(UpgradeType.SleightOfHand, "Sleight of Hand", 20000000, "All Heat generation -10%"),
                UpgradeType.BuyDiceTableLicense => new Upgrade(UpgradeType.BuyDiceTableLicense, "Buy Dice Table License", 129600000, "Win condition for MVP"),
                _ => null
            };
        }
        
        // Main update loop
        public void Update(TimeSpan deltaTime)
        {
            ProcessAutomation(deltaTime);
            UpdateHeat(deltaTime);
        }
        
        // Helper methods
        private string FormatCash(decimal cash)
        {
            return $"${cash:F2}";
        }
        
        private string FormatHeat(decimal heat, decimal maxHeat)
        {
            return $"{heat:F1} / {maxHeat:F0}";
        }
        
        public decimal GetCurrentOddsUpgradeCost()
        {
            return Upgrade.CalculateOddsUpgradeCost(State.OddsUpgradeLevel);
        }
        
        public bool CanAffordBet(decimal betAmount)
        {
            return State.Cash >= betAmount;
        }
        
        public bool CanAffordUpgrade(UpgradeType upgradeType)
        {
            var upgrade = GetUpgrade(upgradeType);
            if (upgrade == null)
                return false;
            
            return State.Cash >= upgrade.Cost && !IsUpgradeAlreadyPurchased(upgradeType);
        }
    }
}
