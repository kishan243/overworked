# Game Design Document - Christmas Overworked

## Overview
Christmas Overworked is a time-based cooperative toy-making game inspired by Overcooked, where players work with an AI companion to fulfill Santa's toy orders.

## Core Gameplay Loop

1. **Start Game** → Santa provides a list of 5 random toys to create
2. **Gather Components** → Players visit workstations to craft components
3. **Assemble Toys** → Bring components to the Assembly station in the correct order
4. **Character Swap** → Press SPACE to swap between player and AI control
5. **Complete All Toys** → Finish all 5 toys as fast as possible
6. **View Final Time** → Game displays completion time

## Game Mechanics

### Character Control
- **Player Character**: Direct control via WASD + E for interaction
- **AI Character**: Autonomous behavior when not controlled
- **Swap Mechanic**: SPACE key swaps positions and control

### Workstation System
Each workstation produces a specific component:
- **Interaction Time**: 1-3 seconds depending on complexity
- **Single User**: Only one character can use a station at a time
- **Component Collection**: Press E to start crafting, E again (or wait) to collect

### Toy Assembly
Toys require components in a specific order:
- **Sequential Assembly**: Components must be added in order
- **Wrong Component**: Rejected if not the next required component
- **Progress Tracking**: UI shows current toy and next required component

### AI Behavior
The AI follows a simple decision tree:
1. Check current toy's next required component
2. If holding a component → Move to Assembly and deliver
3. If not holding → Find workstation for needed component
4. Craft component and repeat

## Toy Types

### 1. Toy Car
- Wood → Wheels → Paint
- Difficulty: Easy (3 components)

### 2. Toy Train
- Wood → Wheels → Wheels → Paint
- Difficulty: Medium (4 components)

### 3. Teddy Bear
- Fabric → Stuffing → Eyes → Bow
- Difficulty: Medium (4 components)

### 4. Robot
- Metal → Metal → Circuits → Paint
- Difficulty: Medium (4 components)

### 5. Doll
- Fabric → Stuffing → Hair → Dress
- Difficulty: Medium (4 components)

## Component Types

| Component | Workstation | Craft Time | Used In |
|-----------|-------------|------------|---------|
| Wood | Wood Station | 2.0s | Car, Train |
| Metal | Metal Station | 2.5s | Robot |
| Fabric | Fabric Station | 1.5s | Teddy Bear, Doll |
| Paint | Paint Station | 3.0s | Car, Train, Robot |
| Wheels | Wheels Station | 2.0s | Car, Train |
| Stuffing | Stuffing Station | 1.5s | Teddy Bear, Doll |
| Eyes | Eyes Station | 1.0s | Teddy Bear |
| Bow | Bow Station | 1.0s | Teddy Bear |
| Circuits | Circuits Station | 2.5s | Robot |
| Hair | Hair Station | 1.5s | Doll |
| Dress | Dress Station | 2.0s | Doll |

## User Interface

### HUD Elements
1. **Timer** (Top Center): Running time in MM:SS:MS format
2. **Toy List** (Top Left): Shows all 5 toys with completion status
3. **Completed Count** (Top Right): X/5 toys completed
4. **Instructions** (Bottom Center): Control scheme reminder
5. **Game Over** (Center): Final time when all toys complete

### Visual Feedback
- **Character Colors**: Blue (Player), Red (AI)
- **Station Colors**: 
  - Green: Available
  - Yellow: In use
  - Cyan: Ready to collect
- **Held Items**: Display above character (optional visual)
- **Active Character**: Indicator showing who you control

## Strategy Tips

### Optimal Play Patterns
1. **Division of Labor**: Let AI handle simple components while you do complex ones
2. **Batch Crafting**: Craft multiple of the same component if multiple toys need it
3. **Swap Timing**: Swap when waiting for crafting to complete
4. **Planning Ahead**: Look at next toy while finishing current one

### Common Pitfalls
- Forgetting to swap → AI could be working while you wait
- Wrong component order → Wastes time going back
- Station blocking → Both characters trying to use same station

## Technical Architecture

### Manager Pattern
- **GameManager**: Controls game state and timer
- **ToyManager**: Handles toy list and completion tracking
- **UIManager**: Updates all UI elements
- **CharacterSwapper**: Manages control switching

### Component Pattern
- **PlayerController**: Player input and interaction
- **AIController**: AI decision making and movement
- **WorkStation**: Crafting logic and state
- **Toy**: Data structure for toy requirements

### Singleton Usage
All managers use singleton pattern for easy access:
```csharp
GameManager.Instance
ToyManager.Instance
UIManager.Instance
CharacterSwapper.Instance
```

## Scalability & Extensions

### Easy Additions
- More toy types (just add to ToyManager array)
- More components (add workstation + update toys)
- Difficulty levels (adjust toy count or time limits)
- Power-ups (speed boost, instant craft, etc.)

### Moderate Additions
- Multiplayer (replace AI with second player)
- Obstacles (conveyor belts, hazards)
- Special orders (bonus toys, rush orders)
- Workshop upgrades (faster stations, etc.)

### Complex Additions
- Procedural workshop layouts
- Story mode with progression
- Online leaderboards
- Customization system

## Performance Considerations

### Optimization Tips
- Use object pooling for UI updates
- Limit FindObjectsOfType calls
- Cache component references
- Use events instead of Update polling where possible

### Expected Performance
- Target: 60 FPS on moderate hardware
- Max simultaneous objects: ~20-30
- UI updates: Every frame for timer, on-change for others

## Testing Checklist

### Core Functionality
- [ ] Player movement works in all directions
- [ ] AI autonomous behavior functions
- [ ] Character swap exchanges positions and control
- [ ] All workstations craft components
- [ ] Assembly station accepts correct components
- [ ] Assembly station rejects wrong components
- [ ] Timer starts and counts correctly
- [ ] Game ends when all toys complete
- [ ] UI updates in real-time

### Edge Cases
- [ ] Rapid character swapping
- [ ] Multiple same components in a row
- [ ] Spamming interact key
- [ ] All 5 toys same type
- [ ] Starting/stopping interactions
- [ ] Game restart functionality

### Polish
- [ ] Controls feel responsive
- [ ] Visual feedback is clear
- [ ] UI is readable
- [ ] Win condition is satisfying
- [ ] No major bugs or glitches

## Future Enhancements

### Phase 2 (Short Term)
- Add Christmas theme (models, music, particles)
- Implement sound effects
- Add tutorial/instructions screen
- Save best times

### Phase 3 (Medium Term)
- Multiple difficulty modes
- Achievement system
- More toy variety (20+ types)
- Workshop customization

### Phase 4 (Long Term)
- Online multiplayer
- Procedural level generation
- Mobile port
- Seasonal events

## Credits & Attribution

Inspired by Overcooked by Ghost Town Games.
Built with Unity and C#.
