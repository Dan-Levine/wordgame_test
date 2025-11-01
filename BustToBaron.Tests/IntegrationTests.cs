using System;
using System.Threading;
using NUnit.Framework;
using BustToBaron.Core;

namespace BustToBaron.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private GameEngine _engine;
        
        [SetUp]
        public void Setup()
        {
            _engine = new GameEngine();
        }
        
        [Test]
        public void FullGameFlow_GrindToUniform_ShouldWork()
        {
            // Start with no cash
            Assert.That(_engine.State.Cash, Is.EqualTo(0));
            Assert.That(_engine.State.HasUniform, Is.False);
            
            // Click work shift 100 times to get $50
            for (int i = 0; i < 100; i++)
            {
                _engine.WorkShift();
            }
            
            Assert.That(_engine.State.Cash, Is.EqualTo(50m));
            
            // Purchase uniform
            bool result = _engine.PurchaseGetUniform();
            Assert.That(result, Is.True);
            Assert.That(_engine.State.HasUniform, Is.True);
            Assert.That(_engine.State.Cash, Is.EqualTo(0));
            
            // Work shift should now give $1
            _engine.WorkShift();
            Assert.That(_engine.State.Cash, Is.EqualTo(1.00m));
        }
        
        [Test]
        public void FullGameFlow_CoinFlipProgression_ShouldWork()
        {
            // Start with enough cash for betting
            _engine.State.Cash = 1000m;
            _engine.State.WinPercentage = 45;
            
            // Place several bets
            for (int i = 0; i < 10; i++)
            {
                _engine.ManualBet(10);
            }
            
            // Cash should have changed (could be more or less due to randomness)
            Assert.That(_engine.State.Cash, Is.Not.EqualTo(1000m));
            
            // Improve odds
            decimal initialCash = _engine.State.Cash;
            _engine.PurchaseImproveOdds();
            
            Assert.That(_engine.State.WinPercentage, Is.EqualTo(46));
            Assert.That(_engine.State.Cash, Is.LessThan(initialCash));
        }
        
        [Test]
        public void FullGameFlow_AutomationWithHeat_ShouldWork()
        {
            // Set up automation
            _engine.State.Cash = 100000m;
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = true;
            _engine.State.WinPercentage = 50;
            _engine.State.AutomationSpeed = 1;
            _engine.State.AutomationStakeMultiplier = 1;
            _engine.State.Heat = 0;
            
            // Process 10 seconds of automation
            for (int i = 0; i < 10; i++)
            {
                _engine.Update(TimeSpan.FromSeconds(1));
            }
            
            // Heat should have changed (net should be negative due to cooldown)
            // At 50% win, heat generation is 1.0 Hps, net is -0.5 Hps
            Assert.That(_engine.State.Heat, Is.LessThanOrEqualTo(0)); // Should be negative or zero
        }
        
        [Test]
        public void FullGameFlow_HeatManagementUpgrades_ShouldReduceHeat()
        {
            _engine.State.Cash = 1000000m;
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = true;
            _engine.State.WinPercentage = 80;
            _engine.State.AutomationSpeed = 2;
            _engine.State.AutomationStakeMultiplier = 1;
            
            // Before upgrade: Heat generation = 8.0 Hps, cooldown = -1.5, net = 6.5 Hps
            decimal heatBefore = _engine.State.CalculateHeatGeneration();
            Assert.That(heatBefore, Is.EqualTo(8.0m));
            
            // Purchase Inconspicuous
            _engine.PurchaseUpgrade(UpgradeType.Inconspicuous);
            
            // Cooldown should now be -2.0
            Assert.That(_engine.State.HeatCooldownRate, Is.EqualTo(-2.0m));
            
            // Purchase Grease Palms
            _engine.PurchaseUpgrade(UpgradeType.GreasePalms);
            
            // Cooldown should now be -3.0
            Assert.That(_engine.State.HeatCooldownRate, Is.EqualTo(-3.0m));
        }
        
        [Test]
        public void FullGameFlow_OddsUpgradeCostScaling_ShouldMatchFormula()
        {
            _engine.State.Cash = 1000000m;
            
            // Level 0: $1 * 1.2^0 = $1.00
            decimal cost0 = _engine.GetCurrentOddsUpgradeCost();
            Assert.That(cost0, Is.EqualTo(1.0m).Within(0.01m));
            
            _engine.PurchaseImproveOdds();
            
            // Level 1: $1 * 1.2^1 = $1.20
            decimal cost1 = _engine.GetCurrentOddsUpgradeCost();
            Assert.That(cost1, Is.EqualTo(1.2m).Within(0.01m));
            
            // Purchase a few more
            for (int i = 0; i < 4; i++)
            {
                _engine.PurchaseImproveOdds();
            }
            
            // Level 5: $1 * 1.2^5 = $2.48832 ? $2.07 (spec says $2.07)
            decimal cost5 = _engine.GetCurrentOddsUpgradeCost();
            Assert.That(cost5, Is.EqualTo(2.48832m).Within(0.01m));
            
            // Level 10: $1 * 1.2^10 = $6.191736 ? $5.16 (spec says $5.16)
            for (int i = 0; i < 5; i++)
            {
                _engine.PurchaseImproveOdds();
            }
            decimal cost10 = _engine.GetCurrentOddsUpgradeCost();
            Assert.That(cost10, Is.EqualTo(6.191736m).Within(0.01m));
        }
        
        [Test]
        public void FullGameFlow_AutomationUpgrades_ShouldScaleCorrectly()
        {
            _engine.State.Cash = 10000000m;
            _engine.State.HasAutoFlipper = true;
            
            // Purchase Faster Flipper
            _engine.PurchaseUpgrade(UpgradeType.FasterFlipper);
            Assert.That(_engine.State.AutomationSpeed, Is.EqualTo(2));
            Assert.That(_engine.State.AutomationStakeMultiplier, Is.EqualTo(1));
            
            // Purchase Automate x10
            _engine.PurchaseUpgrade(UpgradeType.AutomateX10);
            Assert.That(_engine.State.AutomationSpeed, Is.EqualTo(1));
            Assert.That(_engine.State.AutomationStakeMultiplier, Is.EqualTo(10));
            
            // Purchase Faster x10 Flipper
            _engine.PurchaseUpgrade(UpgradeType.FasterX10Flipper);
            Assert.That(_engine.State.AutomationSpeed, Is.EqualTo(2));
            Assert.That(_engine.State.AutomationStakeMultiplier, Is.EqualTo(10));
        }
        
        [Test]
        public void FullGameFlow_HeatPenalty_ShouldTriggerCorrectly()
        {
            bool caughtTriggered = false;
            _engine.OnCaught += () => caughtTriggered = true;
            
            _engine.State.Cash = 100000m;
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = true;
            _engine.State.WinPercentage = 80;
            _engine.State.AutomationSpeed = 2;
            _engine.State.AutomationStakeMultiplier = 1;
            _engine.State.Heat = 995m; // Close to max
            
            // Process enough time to trigger penalty
            // Heat generation: 8.0 Hps, cooldown: -1.5 Hps, net: 6.5 Hps
            // Need 5 more heat, so about 5/6.5 ? 0.77 seconds
            _engine.Update(TimeSpan.FromSeconds(1));
            
            Assert.That(caughtTriggered, Is.True);
            Assert.That(_engine.State.Heat, Is.EqualTo(0));
            Assert.That(_engine.State.Cash, Is.EqualTo(50000m));
            Assert.That(_engine.State.AutoFlipEnabled, Is.False);
        }
        
        [Test]
        public void FullGameFlow_WinCondition_ShouldCompleteMVP()
        {
            _engine.State.Cash = 129600000m;
            
            bool result = _engine.PurchaseUpgrade(UpgradeType.BuyDiceTableLicense);
            
            Assert.That(result, Is.True);
            Assert.That(_engine.State.HasDiceTableLicense, Is.True);
            Assert.That(_engine.State.Cash, Is.EqualTo(0));
        }
        
        [Test]
        public void ManualBet_X100_WithoutUpgrade_ShouldFail()
        {
            _engine.State.Cash = 10000m;
            _engine.State.HasHighRoller = false;
            
            bool result = _engine.ManualBet(1000);
            
            Assert.That(result, Is.False);
        }
        
        [Test]
        public void HeatCalculation_AllStakeMultipliers_ShouldMatchSpec()
        {
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = true;
            _engine.State.WinPercentage = 80;
            _engine.State.AutomationSpeed = 1;
            
            // x1 stake: (1.0 + 3.0) * 1.0 * 1.0 = 4.0
            _engine.State.AutomationStakeMultiplier = 1;
            decimal hps1 = _engine.State.CalculateHeatGeneration();
            Assert.That(hps1, Is.EqualTo(4.0m));
            
            // x10 stake: (1.0 + 3.0) * 1.0 * 1.5 = 6.0
            _engine.State.AutomationStakeMultiplier = 10;
            decimal hps10 = _engine.State.CalculateHeatGeneration();
            Assert.That(hps10, Is.EqualTo(6.0m));
            
            // x100 stake: (1.0 + 3.0) * 1.0 * 2.0 = 8.0
            _engine.State.AutomationStakeMultiplier = 100;
            decimal hps100 = _engine.State.CalculateHeatGeneration();
            Assert.That(hps100, Is.EqualTo(8.0m));
            
            // x1000 stake: (1.0 + 3.0) * 1.0 * 2.5 = 10.0
            _engine.State.AutomationStakeMultiplier = 1000;
            decimal hps1000 = _engine.State.CalculateHeatGeneration();
            Assert.That(hps1000, Is.EqualTo(10.0m));
        }
        
        [Test]
        public void HeatCalculation_WithSpeedMultiplier_ShouldMatchSpec()
        {
            _engine.State.HasAutoFlipper = true;
            _engine.State.AutoFlipEnabled = true;
            _engine.State.WinPercentage = 80;
            _engine.State.AutomationStakeMultiplier = 1;
            
            // 1x speed: (1.0 + 3.0) * 1.0 * 1.0 = 4.0
            _engine.State.AutomationSpeed = 1;
            decimal hps1 = _engine.State.CalculateHeatGeneration();
            Assert.That(hps1, Is.EqualTo(4.0m));
            
            // 2x speed: (1.0 + 3.0) * 2.0 * 1.0 = 8.0
            _engine.State.AutomationSpeed = 2;
            decimal hps2 = _engine.State.CalculateHeatGeneration();
            Assert.That(hps2, Is.EqualTo(8.0m));
        }
    }
}
