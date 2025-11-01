using System;
using System.Threading;
using NUnit.Framework;
using BustToBaron.Core;

namespace BustToBaron.Tests
{
    [TestFixture]
    public class GameStateTests
    {
        [Test]
        public void InitialState_ShouldHaveZeroCashAndHeat()
        {
            var state = new GameState();
            
            Assert.That(state.Cash, Is.EqualTo(0));
            Assert.That(state.Heat, Is.EqualTo(0));
            Assert.That(state.MaxHeat, Is.EqualTo(1000));
        }
        
        [Test]
        public void WorkShiftValue_WithoutUniform_ShouldBeFiftyCents()
        {
            var state = new GameState();
            
            Assert.That(state.WorkShiftValue, Is.EqualTo(0.50m));
        }
        
        [Test]
        public void WorkShiftValue_WithUniform_ShouldBeOneDollar()
        {
            var state = new GameState();
            state.HasUniform = true;
            
            Assert.That(state.WorkShiftValue, Is.EqualTo(1.00m));
        }
        
        [Test]
        public void WinPercentage_Initial_ShouldBe45()
        {
            var state = new GameState();
            
            Assert.That(state.WinPercentage, Is.EqualTo(45));
        }
        
        [Test]
        public void CalculateHeatCooldown_NoUpgrades_ShouldBeNegativeOnePointFive()
        {
            var state = new GameState();
            state.UpdateHeatCooldown();
            
            Assert.That(state.HeatCooldownRate, Is.EqualTo(-1.5m));
        }
        
        [Test]
        public void CalculateHeatCooldown_WithInconspicuous_ShouldBeNegativeTwo()
        {
            var state = new GameState();
            state.HasInconspicuous = true;
            state.UpdateHeatCooldown();
            
            Assert.That(state.HeatCooldownRate, Is.EqualTo(-2.0m));
        }
        
        [Test]
        public void CalculateHeatCooldown_WithGreasePalms_ShouldBeNegativeThree()
        {
            var state = new GameState();
            state.HasGreasePalms = true;
            state.UpdateHeatCooldown();
            
            Assert.That(state.HeatCooldownRate, Is.EqualTo(-3.0m));
        }
        
        [Test]
        public void UpdateMaxHeat_NoUpgrades_ShouldBe1000()
        {
            var state = new GameState();
            state.UpdateMaxHeat();
            
            Assert.That(state.MaxHeat, Is.EqualTo(1000));
        }
        
        [Test]
        public void UpdateMaxHeat_WithBribeFloorManager_ShouldBe1500()
        {
            var state = new GameState();
            state.HasBribeFloorManager = true;
            state.UpdateMaxHeat();
            
            Assert.That(state.MaxHeat, Is.EqualTo(1500));
        }
        
        [Test]
        public void CalculateHeatGeneration_AutoFlipOff_ShouldBeZero()
        {
            var state = new GameState();
            
            Assert.That(state.CalculateHeatGeneration(), Is.EqualTo(0));
        }
        
        [Test]
        public void CalculateHeatGeneration_AutoFlipOn_BaseSettings_ShouldMatchFormula()
        {
            var state = new GameState();
            state.AutoFlipEnabled = true;
            state.HasAutoFlipper = true;
            state.WinPercentage = 45;
            state.AutomationSpeed = 1;
            state.AutomationStakeMultiplier = 1;
            
            // OddsHps = 0.1 * (45 - 50) = -0.5
            // Hps = (1.0 + (-0.5)) * 1.0 * 1.0 = 0.5
            // Net = 0.5 - 1.5 = -1.0 (but we're just checking generation)
            decimal hps = state.CalculateHeatGeneration();
            
            // Expected: (1.0 + (-0.5)) * 1.0 * 1.0 = 0.5
            Assert.That(hps, Is.EqualTo(0.5m));
        }
        
        [Test]
        public void CalculateHeatGeneration_At80PercentWin_ShouldMatchFormula()
        {
            var state = new GameState();
            state.AutoFlipEnabled = true;
            state.HasAutoFlipper = true;
            state.WinPercentage = 80;
            state.AutomationSpeed = 2;
            state.AutomationStakeMultiplier = 1;
            
            // OddsHps = 0.1 * (80 - 50) = 3.0
            // Hps = (1.0 + 3.0) * 2.0 * 1.0 = 8.0
            decimal hps = state.CalculateHeatGeneration();
            
            Assert.That(hps, Is.EqualTo(8.0m));
        }
        
        [Test]
        public void CalculateHeatGeneration_WithSleightOfHand_ShouldReduceBy10Percent()
        {
            var state = new GameState();
            state.AutoFlipEnabled = true;
            state.HasAutoFlipper = true;
            state.WinPercentage = 80;
            state.AutomationSpeed = 2;
            state.AutomationStakeMultiplier = 1;
            state.HasSleightOfHand = true;
            
            // Without Sleight: 8.0
            // With Sleight: 8.0 * 0.9 = 7.2
            decimal hps = state.CalculateHeatGeneration();
            
            Assert.That(hps, Is.EqualTo(7.2m));
        }
        
        [Test]
        public void CalculateCoinFlipEV_At45Percent_ShouldBeNegativeOne()
        {
            var state = new GameState();
            state.WinPercentage = 45;
            
            // EV = (0.45 * 10) + (0.55 * -10) = 4.5 - 5.5 = -1.0
            decimal ev = state.CalculateCoinFlipEV(10);
            
            Assert.That(ev, Is.EqualTo(-1.0m));
        }
        
        [Test]
        public void CalculateCoinFlipEV_At50Percent_ShouldBeZero()
        {
            var state = new GameState();
            state.WinPercentage = 50;
            
            // EV = (0.50 * 10) + (0.50 * -10) = 5 - 5 = 0
            decimal ev = state.CalculateCoinFlipEV(10);
            
            Assert.That(ev, Is.EqualTo(0));
        }
        
        [Test]
        public void CalculateCoinFlipEV_At55Percent_ShouldBePositiveOne()
        {
            var state = new GameState();
            state.WinPercentage = 55;
            
            // EV = (0.55 * 10) + (0.45 * -10) = 5.5 - 4.5 = 1.0
            decimal ev = state.CalculateCoinFlipEV(10);
            
            Assert.That(ev, Is.EqualTo(1.0m));
        }
    }
}
