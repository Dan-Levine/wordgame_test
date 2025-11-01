# Bust to Baron MVP - Implementation

This is a complete implementation of the "Bust to Baron" MVP incremental game according to the provided specifications.

## Project Structure

```
/workspace/
??? BustToBaron.Core/          # Core game logic library
?   ??? GameState.cs           # Game state management
?   ??? GameEngine.cs          # Main game engine with all mechanics
?   ??? Upgrade.cs             # Upgrade system definitions
??? BustToBaron.Tests/         # Unit tests
?   ??? GameStateTests.cs      # Tests for GameState
?   ??? GameEngineTests.cs     # Tests for GameEngine
?   ??? IntegrationTests.cs    # End-to-end integration tests
??? BustToBaron.sln            # Solution file
```

## Core Features Implemented

### 1. The Grind System
- **Work Shift Button**: Grants $0.50 per click (upgraded to $1.00 with uniform)
- **Get a Uniform Upgrade**: Costs $50, upgrades Work Shift to $1.00 per click

### 2. Coin Flip Manual System
- **Bet Buttons**: 
  - Bet $10 (x1) - always available
  - Bet $100 (x10) - requires "Raise the Stakes" upgrade ($5,000)
  - Bet $1,000 (x100) - requires "High Roller" upgrade ($50,000)
- **Payout**: 2x the bet amount
- **Base Win %**: 45% (EV = -$1.00 per $10 bet)
- **Improve Odds Upgrade**: Costs $1 * (1.2 ^ Level), increases Win % by 1% per level

### 3. Coin Flip Automation System
- **Auto-Flipper**: Unlocks at $2,000, enables Auto-Flip toggle
- **Automation Upgrades**:
  - Faster Flipper ($25,000): Doubles speed to 2x/sec
  - Automate x10 Bets ($100,000): Sets stake multiplier to x10
  - Automate x100 Bets ($216,000): Sets stake multiplier to x100
  - Automate x1000 Bets ($8,640,000): Sets stake multiplier to x1000
  - Faster x10 Flipper ($54,000): Doubles speed and sets stake to x10
  - Faster x100 Flipper ($1,000,000): Doubles speed and sets stake to x100
  - Faster x1000 Flipper ($86,400,000): Doubles speed and sets stake to x1000

### 4. Heat System
- **Heat Generation (Hps)**: Only when Auto-Flip is ON
  - Formula: `Hps = (1.0 + OddsHps) * SpeedMultiplier * StakeMultiplier`
  - OddsHps: `0.1 * (Win % - 50)`
  - SpeedMultiplier: 1.0 (1x/sec) or 2.0 (2x/sec)
  - StakeMultiplier: 1.0 (x1), 1.5 (x10), 2.0 (x100), 2.5 (x1000)
- **Base Cooldown**: -1.5 Hps (always active)
- **Heat Management Upgrades**:
  - Inconspicuous ($50,000): Cooldown ? -2.0 Hps
  - Grease the Palms ($750,000): Cooldown ? -3.0 Hps
  - Bribe Floor Manager ($5,000,000): Max Heat ? 1,500
  - Sleight of Hand ($20,000,000): All Heat generation -10%
- **Penalty**: When Heat >= Max Heat:
  - Cash reduced by 50%
  - Heat reset to 0
  - Auto-Flip disabled

### 5. Win Condition
- **Buy Dice Table License**: Costs $129,600,000
- Purchasing this upgrade completes the MVP

## Usage Example

```csharp
using BustToBaron.Core;
using System;

var engine = new GameEngine();

// Grind to get starting cash
for (int i = 0; i < 100; i++)
{
    engine.WorkShift();
}

// Purchase uniform
engine.PurchaseGetUniform();

// Purchase auto-flipper
engine.State.Cash = 2000m;
engine.PurchaseUpgrade(UpgradeType.AutoFlipper);

// Enable automation
engine.ToggleAutoFlip();

// Main game loop
var lastUpdate = DateTime.Now;
while (!engine.State.HasDiceTableLicense)
{
    var now = DateTime.Now;
    var deltaTime = now - lastUpdate;
    lastUpdate = now;
    
    engine.Update(deltaTime);
    
    // Purchase upgrades, manage heat, etc.
    if (engine.CanAffordUpgrade(UpgradeType.FasterFlipper))
    {
        engine.PurchaseUpgrade(UpgradeType.FasterFlipper);
    }
}
```

## Testing

The project includes comprehensive unit tests covering:
- Game state initialization and updates
- Grind system mechanics
- Coin flip betting and odds improvements
- Automation system
- Heat generation and cooling
- All upgrade purchases
- Heat penalty triggers
- Win condition

Run tests using:
```bash
dotnet test BustToBaron.sln
```

## Implementation Notes

1. **High Roller Upgrade**: The cost for "High Roller" (x100 betting) was not explicitly specified in the requirements. It has been set to $50,000 (10x the "Raise the Stakes" cost) as a reasonable progression.

2. **Upgrade Logic**: The upgrade system allows purchasing upgrades in any order, but some combinations may be redundant (e.g., buying "Faster Flipper" then "Automate x10" achieves the same result as "Faster x10 Flipper").

3. **Random Number Generation**: The game uses a `Random` instance for coin flip outcomes. For deterministic testing, consider using a seeded random generator or dependency injection.

4. **Event System**: The `GameEngine` exposes events (`OnCashChanged`, `OnHeatChanged`, `OnMessage`, `OnCaught`) for UI integration. These are optional and can be ignored if not needed.

5. **Precision**: All monetary values use `decimal` type for precise financial calculations without floating-point errors.

## Definition of Done Checklist

? Game State tracks Cash and Heat correctly  
? The Grind tab functionality (Work Shift, Get a Uniform)  
? Coin Flip manual betting with all bet levels  
? Improve Odds upgrade with correct cost scaling  
? Raise the Stakes and High Roller upgrades unlock betting  
? Automation loop with Auto-Flip toggle  
? All automation upgrades functional  
? Heat generation based on Hps formula  
? Passive Heat cooldown  
? Heat penalty triggers correctly  
? Heat management upgrades functional  
? Buy Dice Table License win condition  
? All costs and formulas match specifications  
? Comprehensive unit tests  

## Future Enhancements (Post-MVP)

- Save/load game state
- Web UI integration
- Dice Table game implementation
- Additional games and progression systems
- Achievements system
- Statistics tracking
