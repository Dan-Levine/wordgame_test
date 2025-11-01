using System;
using System.Threading;
using NUnit.Framework;
using BustToBaron.Core;

namespace BustToBaron.Tests
{
    [TestFixture]
    public class GameEngineTests
    {
        private GameEngine _engine;
        
        [SetUp]
        public void Setup()
        {
            _engine = new GameEngine();
        }
        
        [Test]
        public void WorkShift_ShouldIncreaseCashByFiftyCents()
        {
            decimal initialCash = _engine.State.Cash;
            
            _engine.WorkShift();
            
            Assert.That(_engine.State.Cash, Is.EqualTo(initialCash + 0.50m));
        }
        
        [Test]
        public void WorkShift_WithUniform_ShouldIncreaseCashByOneDollar()
        {
            _engine.State.HasUniform = true;
            decimal initialCash = _engine.State.Cash;
            
            _engine.WorkShift();
            
            Assert.That(_engine.State.Cash, Is.EqualTo(initialCash + 1.00m));
        }
        
        [Test]
        public void PurchaseGetUniform_WithoutEnoughCash_ShouldFail()
        {
            _engine.State.Cash = 49m;
            
            bool result = _engine.PurchaseGetUniform();
            
            Assert.That(result, Is.False);
            Assert.That(_engine.State.HasUniform, Is.False);
            Assert.That(_engine.State.Cash, Is.EqualTo(49m));
        }
        
        [Test]
        public void PurchaseGetUniform_WithEnoughCash_ShouldSucceed()
        {
            _engine.State.Cash = 50m;
            
            bool result = _engine.PurchaseGetUniform();
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.HasUniform, Is.True);
            Assert.That(_engine.State.Cash, Is.EqualTo(0));
        }
        
        [Test]
        public void PurchaseGetUniform_AlreadyPurchased_ShouldFail()
        {
            _engine.State.Cash = 100m;
            _engine.State.HasUniform = true;
            
            bool result = _engine.PurchaseGetUniform();
            
            Assert.That(result, Is.False);
            Assert.That(_engine.State.Cash, Is.EqualTo(100m));
        }
        
        [Test]
        public void ManualBet_WithoutEnoughCash_ShouldFail()
        {
            _engine.State.Cash = 9m;
            
            bool result = _engine.ManualBet(10);
            
            Assert.That(result, Is.False);
            Assert.That(_engine.State.Cash, Is.EqualTo(9m));
        }
        
        [Test]
        public void ManualBet_WithEnoughCash_ShouldDeductBet()
        {
            _engine.State.Cash = 100m;
            _engine.State.WinPercentage = 0; // Force loss for testing
            
            // Mock random to always lose
            _engine.ManualBet(10);
            
            // Cash should be reduced by bet amount (win or lose, we deduct first)
            // Since we're testing with 0% win chance, we lose
            Assert.That(_engine.State.Cash, Is.EqualTo(90m));
        }
        
        [Test]
        public void ManualBet_X10_WithoutUpgrade_ShouldFail()
        {
            _engine.State.Cash = 1000m;
            _engine.State.HasRaiseStakes = false;
            
            bool result = _engine.ManualBet(100);
            
            Assert.That(result, Is.False);
        }
        
        [Test]
        public void ManualBet_X10_WithUpgrade_ShouldSucceed()
        {
            _engine.State.Cash = 1000m;
            _engine.State.HasRaiseStakes = true;
            _engine.State.WinPercentage = 0; // Force loss
            
            bool result = _engine.ManualBet(100);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.Cash, Is.EqualTo(900m));
        }
        
        [Test]
        public void PurchaseImproveOdds_ShouldIncreaseWinPercentage()
        {
            _engine.State.Cash = 100m;
            int initialWinPct = _engine.State.WinPercentage;
            int initialLevel = _engine.State.OddsUpgradeLevel;
            
            bool result = _engine.PurchaseImproveOdds();
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.WinPercentage, Is.EqualTo(initialWinPct + 1));
            Assert.That(_engine.State.OddsUpgradeLevel, Is.EqualTo(initialLevel + 1));
        }
        
        [Test]
        public void PurchaseImproveOdds_CostShouldScaleCorrectly()
        {
            _engine.State.Cash = 1000m;
            
            // Level 0: $1 * 1.2^0 = $1
            decimal cost0 = _engine.GetCurrentOddsUpgradeCost();
            Assert.That(cost0, Is.EqualTo(1.0m));
            
            _engine.PurchaseImproveOdds();
            
            // Level 1: $1 * 1.2^1 = $1.2
            decimal cost1 = _engine.GetCurrentOddsUpgradeCost();
            Assert.That(cost1, Is.EqualTo(1.2m));
            
            _engine.PurchaseImproveOdds();
            
            // Level 2: $1 * 1.2^2 = $1.44
            decimal cost2 = _engine.GetCurrentOddsUpgradeCost();
            Assert.That(cost2, Is.EqualTo(1.44m));
        }
        
        [Test]
        public void PurchaseImproveOdds_MaxWinPercentage_ShouldFail()
        {
            _engine.State.Cash = 1000000m;
            _engine.State.WinPercentage = 80;
            _engine.State.OddsUpgradeLevel = 35;
            
            bool result = _engine.PurchaseImproveOdds();
            
            Assert.That(result, Is.False);
            Assert.That(_engine.State.WinPercentage, Is.EqualTo(80));
        }
        
        [Test]
        public void ToggleAutoFlip_WithoutAutoFlipper_ShouldFail()
        {
            _engine.State.HasAutoFlipper = false;
            
            bool result = _engine.ToggleAutoFlip();
            
            Assert.That(result, Is.False);
        }
        
        [Test]
        public void ToggleAutoFlip_WithAutoFlipper_ShouldToggle()
        {
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = false;
            
            bool result = _engine.ToggleAutoFlip();
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.AutoFlipEnabled, Is.True);
            
            result = _engine.ToggleAutoFlip();
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.AutoFlipEnabled, Is.False);
        }
        
        [Test]
        public void PurchaseAutoFlipper_ShouldEnableToggle()
        {
            _engine.State.Cash = 2000m;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.AutoFlipper);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.HasAutoFlipper, Is.True);
            Assert.That(_engine.State.Cash, Is.EqualTo(0));
        }
        
        [Test]
        public void ProcessAutomation_WhenEnabled_ShouldProcessBets()
        {
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = true;
            _engine.State.Cash = 10000m;
            _engine.State.WinPercentage = 50; // 50% win chance for predictable EV
            _engine.State.AutomationSpeed = 1;
            _engine.State.AutomationStakeMultiplier = 1;
            
            // Process 1 second of automation
            _engine.Update(TimeSpan.FromSeconds(1));
            
            // Should have processed 1 bet (10 cash bet)
            // At 50% win chance, EV is 0, but we should see cash change
            Assert.That(_engine.State.Cash, Is.Not.EqualTo(10000m));
        }
        
        [Test]
        public void ProcessAutomation_WhenOutOfCash_ShouldDisable()
        {
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = true;
            _engine.State.Cash = 5m; // Less than bet amount
            _engine.State.AutomationSpeed = 1;
            _engine.State.AutomationStakeMultiplier = 1;
            
            _engine.Update(TimeSpan.FromSeconds(1));
            
            Assert.That(_engine.State.AutoFlipEnabled, Is.False);
        }
        
        [Test]
        public void UpdateHeat_WithAutoFlipOff_ShouldCoolDown()
        {
            _engine.State.Heat = 100m;
            _engine.State.AutoFlipEnabled = false;
            
            _engine.UpdateHeat(TimeSpan.FromSeconds(1));
            
            // Should cool down by 1.5 per second
            Assert.That(_engine.State.Heat, Is.EqualTo(98.5m));
        }
        
        [Test]
        public void UpdateHeat_WithAutoFlipOn_ShouldGenerateHeat()
        {
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = true;
            _engine.State.WinPercentage = 50;
            _engine.State.AutomationSpeed = 1;
            _engine.State.AutomationStakeMultiplier = 1;
            _engine.State.Heat = 100m;
            
            // Heat generation: (1.0 + 0) * 1.0 * 1.0 = 1.0 Hps
            // Net: 1.0 - 1.5 = -0.5 Hps
            _engine.UpdateHeat(TimeSpan.FromSeconds(1));
            
            Assert.That(_engine.State.Heat, Is.EqualTo(99.5m));
        }
        
        [Test]
        public void UpdateHeat_AtMaxHeat_ShouldTriggerCaughtPenalty()
        {
            bool caughtTriggered = false;
            _engine.OnCaught += () => caughtTriggered = true;
            
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = true;
            _engine.State.WinPercentage = 80;
            _engine.State.AutomationSpeed = 2;
            _engine.State.AutomationStakeMultiplier = 1;
            _engine.State.Heat = 999m;
            _engine.State.Cash = 10000m;
            
            // Heat generation: (1.0 + 3.0) * 2.0 * 1.0 = 8.0 Hps
            // Net: 8.0 - 1.5 = 6.5 Hps per second
            // After 1 second: 999 + 6.5 = 1005.5, which exceeds 1000
            _engine.UpdateHeat(TimeSpan.FromSeconds(1));
            
            Assert.That(caughtTriggered, Is.True);
            Assert.That(_engine.State.Heat, Is.EqualTo(0));
            Assert.That(_engine.State.Cash, Is.EqualTo(5000m));
            Assert.That(_engine.State.AutoFlipEnabled, Is.False);
        }
        
        [Test]
        public void PurchaseHighRoller_ShouldUnlockX100Betting()
        {
            _engine.State.Cash = 50000m;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.HighRoller);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.HasHighRoller, Is.True);
            
            _engine.State.Cash = 10000m;
            bool canBet = _engine.ManualBet(1000);
            
            Assert.That(canBet, Is.True);
        }
        
        [Test]
        public void PurchaseRaiseStakes_ShouldUnlockX10Betting()
        {
            _engine.State.Cash = 5000m;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.RaiseStakes);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.HasRaiseStakes, Is.True);
            
            _engine.State.Cash = 1000m;
            bool canBet = _engine.ManualBet(100);
            
            Assert.That(canBet, Is.True);
        }
        
        [Test]
        public void PurchaseInconspicuous_ShouldIncreaseCooldownRate()
        {
            _engine.State.Cash = 50000m;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.Inconspicuous);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.HeatCooldownRate, Is.EqualTo(-2.0m));
        }
        
        [Test]
        public void PurchaseGreasePalms_ShouldIncreaseCooldownRate()
        {
            _engine.State.Cash = 750000m;
            _engine.State.HasInconspicuous = true;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.GreasePalms);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.HeatCooldownRate, Is.EqualTo(-3.0m));
        }
        
        [Test]
        public void PurchaseBribeFloorManager_ShouldIncreaseMaxHeat()
        {
            _engine.State.Cash = 5000000m;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.BribeFloorManager);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.MaxHeat, Is.EqualTo(1500));
        }
        
        [Test]
        public void PurchaseSleightOfHand_ShouldReduceHeatGeneration()
        {
            _engine.State.Cash = 20000000m;
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = true;
            _engine.State.WinPercentage = 80;
            _engine.State.AutomationSpeed = 2;
            _engine.State.AutomationStakeMultiplier = 1;
            
            // Before: (1.0 + 3.0) * 2.0 * 1.0 = 8.0
            decimal hpsBefore = _engine.State.CalculateHeatGeneration();
            Assert.That(hpsBefore, Is.EqualTo(8.0m));
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.SleightOfHand);
            
            Assert.That(result, Is.True);
            
            // After: 8.0 * 0.9 = 7.2
            decimal hpsAfter = _engine.State.CalculateHeatGeneration();
            Assert.That(hpsAfter, Is.EqualTo(7.2m));
        }
        
        [Test]
        public void PurchaseBuyDiceTableLicense_ShouldCompleteMVP()
        {
            _engine.State.Cash = 129600000m;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.BuyDiceTableLicense);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.HasDiceTableLicense, Is.True);
            Assert.That(_engine.State.Cash, Is.EqualTo(0));
        }
        
        [Test]
        public void PurchaseFasterFlipper_ShouldDoubleSpeed()
        {
            _engine.State.Cash = 25000m;
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutomationSpeed = 1;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.FasterFlipper);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.AutomationSpeed, Is.EqualTo(2));
        }
        
        [Test]
        public void PurchaseAutomateX10_ShouldSetStakeMultiplierTo10()
        {
            _engine.State.Cash = 100000m;
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutomationStakeMultiplier = 1;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.AutomateX10);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.AutomationStakeMultiplier, Is.EqualTo(10));
        }
        
        [Test]
        public void PurchaseAutomateX100_ShouldSetStakeMultiplierTo100()
        {
            _engine.State.Cash = 216000m;
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutomationStakeMultiplier = 10;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.AutomateX100);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.AutomationStakeMultiplier, Is.EqualTo(100));
        }
        
        [Test]
        public void PurchaseAutomateX1000_ShouldSetStakeMultiplierTo1000()
        {
            _engine.State.Cash = 8640000m;
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutomationStakeMultiplier = 100;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.AutomateX1000);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.AutomationStakeMultiplier, Is.EqualTo(1000));
        }
        
        [Test]
        public void PurchaseFasterX10Flipper_ShouldSetSpeedAndStake()
        {
            _engine.State.Cash = 54000m;
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutomationSpeed = 1;
            _engine.State.AutomationStakeMultiplier = 1;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.FasterX10Flipper);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.AutomationSpeed, Is.EqualTo(2));
            Assert.That(_engine.State.AutomationStakeMultiplier, Is.EqualTo(10));
        }
        
        [Test]
        public void CanAffordBet_WithEnoughCash_ShouldReturnTrue()
        {
            _engine.State.Cash = 100m;
            
            Assert.That(_engine.CanAffordBet(50), Is.True);
        }
        
        [Test]
        public void CanAffordBet_WithoutEnoughCash_ShouldReturnFalse()
        {
            _engine.State.Cash = 10m;
            
            Assert.That(_engine.CanAffordBet(50), Is.False);
        }
        
        [Test]
        public void CanAffordUpgrade_WithEnoughCash_ShouldReturnTrue()
        {
            _engine.State.Cash = 100m;
            
            Assert.That(_engine.CanAffordUpgrade(UpgradeType.GetUniform), Is.True);
        }
        
        [Test]
        public void CanAffordUpgrade_AlreadyPurchased_ShouldReturnFalse()
        {
            _engine.State.Cash = 100m;
            _engine.State.HasUniform = true;
            
            Assert.That(_engine.CanAffordUpgrade(UpgradeType.GetUniform), Is.False);
        }
    }
}
