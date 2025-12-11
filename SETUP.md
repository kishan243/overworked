# Unity Scene Setup Guide

This guide explains how to set up a basic scene for the Christmas Overcooked game.

## Required GameObjects

### 1. Game Managers

Create empty GameObjects and attach the following scripts:

#### GameManager
- Create empty GameObject named "GameManager"
- Attach `GameManager.cs` script

#### ToyManager
- Create empty GameObject named "ToyManager"
- Attach `ToyManager.cs` script
- Set `totalToysToMake` to 5 (or your desired number)

#### CharacterSwapper
- Create empty GameObject named "CharacterSwapper"
- Attach `CharacterSwapper.cs` script
- Assign the Player and AI controllers in the inspector (after creating them)

### 2. Characters

#### Player Character
- Create a Cube or custom 3D model named "Player"
- Set position to (0, 0.5, 0)
- Attach `PlayerController.cs` script
- Set `moveSpeed` to 5
- Set `characterName` to "Player"
- Add a distinct material color (e.g., blue)

#### AI Character
- Create a Cube or custom 3D model named "AI"
- Set position to (3, 0.5, 0)
- Attach `AIController.cs` script
- Set `moveSpeed` to 5
- Set `characterName` to "AI Helper"
- Add a distinct material color (e.g., red)

### 3. Workstations

Create workstations for each component type. For each:
- Create a Cube or custom model
- Scale it appropriately (e.g., 1, 1, 1)
- Add WorkStation.cs component
- Configure the station type and component produced

#### Example Workstation Layout:
```
Wood Station:       Position (-5, 0.5, 0)
Metal Station:      Position (-3, 0.5, 0)
Fabric Station:     Position (-5, 0.5, -3)
Paint Station:      Position (5, 0.5, 0)
Wheels Station:     Position (5, 0.5, -3)
Stuffing Station:   Position (-3, 0.5, -3)
Assembly Station:   Position (0, 0.5, -5)
```

### 4. UI Canvas

Create a Canvas for the UI:

#### Canvas Setup
- Create UI > Canvas
- Set Canvas Scaler to "Scale With Screen Size"
- Reference resolution: 1920x1080

#### UI Text Elements
Create the following UI Text elements as children of the Canvas:

**Timer Text:**
- Position: Top center
- Anchor: Top center
- Text: "Time: 00:00:00"

**Toy List Text:**
- Position: Top left
- Anchor: Top left
- Text: "Toys to Make:"
- Font size: 18

**Completed Count Text:**
- Position: Top right
- Anchor: Top right
- Text: "Completed: 0/5"

**Game Over Text:**
- Position: Center
- Anchor: Middle center
- Text: ""
- Font size: 36
- Start with this disabled

**Instructions Text:**
- Position: Bottom center
- Anchor: Bottom center
- Text: "WASD - Move | E - Interact | SPACE - Swap"

#### UIManager Setup
- Create empty GameObject named "UIManager"
- Attach `UIManager.cs` script
- Drag and drop the UI Text elements to their respective fields

### 5. Camera

Position the main camera:
- Position: (0, 8, -10)
- Rotation: (30, 0, 0)
- The CharacterSwapper will automatically control the camera

### 6. Ground

- Create a Plane
- Position: (0, 0, 0)
- Scale: (2, 1, 2) or larger
- Add a material/texture for the floor

## Testing the Scene

1. Press Play
2. Use WASD to move the player
3. Move near a workstation and press E to interact
4. Watch the progress bar (if visual feedback is added)
5. Press E again to collect the component
6. Move to Assembly station and press E to add component to toy
7. Press SPACE to swap characters
8. The AI should start working autonomously

## Tips

- Add visual distinction to workstations using different colors
- Consider adding labels above workstations
- Add simple particle effects for interactions
- Use ProBuilder or other tools to create a more detailed environment
- Add Christmas decorations and theming

## Common Issues

**Characters not moving:**
- Check that PlayerController is attached
- Verify moveSpeed is greater than 0
- Make sure isPlayerControlled is true

**UI not updating:**
- Ensure all UI Text references are set in UIManager
- Check that UIManager is in the scene
- Verify GameManager is running

**Workstations not working:**
- Confirm stationType and componentProduced are set
- Check that character is close enough (< 2 units)
- Ensure station IsAvailable() returns true

**AI not working:**
- Verify AIController script is attached
- Check that ToyManager has toys generated
- Ensure workstations have correct componentProduced values

## Next Steps

- Add more visual polish (models, textures, particles)
- Implement sound effects
- Add Christmas music
- Create obstacles or challenges
- Add difficulty scaling
- Implement a main menu
- Add save/load functionality for high scores
