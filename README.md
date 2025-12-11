# Christmas Overworked

A Christmas-themed cooking game inspired by Overcooked, built with Unity and C#.

## Game Overview

In Christmas Overworked, you and an AI companion work together to help Santa make toys! Santa gives you a list of 5 toys to create, and the game times how fast you can complete them all.

## Key Features

- **Character Swapping**: Press SPACE to swap control between the player and AI character. When you swap, you take over the AI's position and the AI takes over yours!
- **AI Companion**: The AI will autonomously work on creating toy components and assembling toys when you're not controlling it
- **Toy Crafting System**: Different toys require different components (wood, wheels, paint, fabric, etc.)
- **Timed Gameplay**: Complete all 5 toys as fast as possible!

## Controls

- **WASD** - Move character
- **E** - Interact with workstations/assembly table
- **SPACE** - Swap control between player and AI

## Game Mechanics

1. **Workstations**: Interact with different workstations to craft components:
   - Wood Station
   - Metal Station
   - Fabric Station
   - Paint Station
   - Wheels Station
   - etc.

2. **Assembly**: Once you have a component, take it to the Assembly station to add it to the current toy

3. **Toy Requirements**: Each toy requires specific components in a specific order. Check the UI to see what's needed next!

## Project Structure

```
Assets/
├── Scripts/
│   ├── GameManager.cs - Main game controller
│   ├── ToyManager.cs - Manages toy list and completion
│   ├── PlayerController.cs - Player movement and interaction
│   ├── AIController.cs - AI behavior and decision making
│   ├── CharacterSwapper.cs - Handles character swapping
│   ├── WorkStation.cs - Workstation interaction logic
│   ├── Toy.cs - Toy data structure
│   └── UIManager.cs - UI updates and display
├── Scenes/
└── Prefabs/
```

## Setup Instructions

### For Unity Editor:

1. Open Unity Hub
2. Click "Add" and select this project folder
3. Open the project with Unity 2020.3 or later
4. Create a new scene or open an existing one
5. Add the required GameObjects:
   - GameManager (attach GameManager.cs)
   - ToyManager (attach ToyManager.cs)
   - UIManager (attach UIManager.cs with UI Text components)
   - CharacterSwapper (attach CharacterSwapper.cs)
   - Player character (attach PlayerController.cs)
   - AI character (attach AIController.cs)
   - Workstations (attach WorkStation.cs, set stationType and componentProduced)
6. Configure the UI elements in the UIManager
7. Press Play!

### For Development:

The project uses standard Unity C# scripts. All core game logic is contained in the `Assets/Scripts/` folder.

## Toy Types

The game randomly selects 5 toys from these options:

1. **Toy Car**: Wood → Wheels → Paint
2. **Toy Train**: Wood → Wheels → Wheels → Paint
3. **Teddy Bear**: Fabric → Stuffing → Eyes → Bow
4. **Robot**: Metal → Metal → Circuits → Paint
5. **Doll**: Fabric → Stuffing → Hair → Dress

## Technical Details

- Built with Unity (C#)
- Singleton pattern for managers
- Component-based architecture
- Simple AI decision-making system
- Real-time timer tracking

## Future Enhancements

- More toy types
- Multiple difficulty levels
- Power-ups and obstacles
- Local multiplayer
- Leaderboards
- Christmas-themed visuals and sound effects

## License

This is a demonstration project. 
