# Quick Start Guide

## Getting Started in 5 Minutes

### Option 1: Manual Unity Setup (Recommended for Production)

1. **Open in Unity**
   ```
   - Open Unity Hub
   - Click "Add" → Select this project folder
   - Open with Unity 2020.3 or later
   ```

2. **Create New Scene**
   ```
   - File → New Scene
   - Save as "MainGame.unity" in Assets/Scenes/
   ```

3. **Add Managers** (Create empty GameObjects)
   ```
   - GameManager (attach GameManager.cs)
   - ToyManager (attach ToyManager.cs)
   - UIManager (attach UIManager.cs)
   - CharacterSwapper (attach CharacterSwapper.cs)
   ```

4. **Add Characters**
   ```
   - Create Cube → Name "Player" → Add PlayerController.cs
   - Create Cube → Name "AI" → Add AIController.cs
   - Position them at (0, 0.5, 0) and (3, 0.5, 0)
   ```

5. **Add Workstations** (11 total)
   ```
   Create Cubes for each station type:
   - Wood, Metal, Fabric, Paint, Wheels, Stuffing, 
     Eyes, Bow, Circuits, Hair, Dress, Assembly
   - Add WorkStation.cs to each
   - Set stationType and componentProduced fields
   ```

6. **Add UI**
   ```
   - Create Canvas
   - Add Text elements for: Timer, ToyList, CompletedCount, 
     GameOver, Instructions
   - Link to UIManager
   ```

7. **Press Play!**

**Detailed instructions:** See `SETUP.md`

---

### Option 2: Quick Test Setup (For Testing Only)

1. **Open in Unity**
   - Open project in Unity

2. **Create Test Scene**
   - Create new empty scene
   - Create empty GameObject named "SceneSetup"
   - Attach `QuickSceneSetup.cs`
   - Right-click component → "Setup Test Scene"
   - **Note:** Still need to manually create UI Canvas

3. **Add UI** (Required)
   - Create UI → Canvas
   - Add Text elements (see Option 1 step 6)

4. **Press Play!**

---

## Game Controls

| Key | Action |
|-----|--------|
| **W** | Move Forward |
| **A** | Move Left |
| **S** | Move Backward |
| **D** | Move Right |
| **E** | Interact with Workstation/Assembly |
| **SPACE** | Swap between Player and AI |

---

## How to Play

### Objective
Complete all 5 toys as fast as possible!

### Gameplay Loop
1. **Check Toy List** (Top Left) - See what toy you're making and what component is needed next
2. **Craft Component** - Go to the appropriate workstation and press E
3. **Collect Component** - Wait for crafting to finish (progress shown on station)
4. **Deliver to Assembly** - Go to Assembly Station and press E to add component to toy
5. **Repeat** - Continue until all components added and toy is complete
6. **Next Toy** - Automatically moves to next toy in the list
7. **Win!** - Complete all 5 toys and see your final time

### Pro Tips
- **Use Character Swap!** - Press SPACE to take control of the AI. The AI will keep working when you're not controlling it!
- **Plan Ahead** - Look at what components the next toy needs while finishing the current one
- **Stay Organized** - Let the AI handle simple tasks while you work on complex ones
- **Watch the UI** - It tells you exactly what component is needed next

---

## Example Toy: Toy Car

1. Go to **Wood Station** → Press E → Wait → Press E to collect **Wood**
2. Go to **Assembly Station** → Press E to add Wood to car
3. Go to **Wheels Station** → Press E → Wait → Press E to collect **Wheels**
4. Go to **Assembly Station** → Press E to add Wheels to car
5. Go to **Paint Station** → Press E → Wait → Press E to collect **Paint**
6. Go to **Assembly Station** → Press E to add Paint to car
7. ✅ **Toy Car Complete!**

---

## Toy Types & Components

| Toy | Components Needed (in order) |
|-----|------------------------------|
| **Toy Car** | Wood → Wheels → Paint |
| **Toy Train** | Wood → Wheels → Wheels → Paint |
| **Teddy Bear** | Fabric → Stuffing → Eyes → Bow |
| **Robot** | Metal → Metal → Circuits → Paint |
| **Doll** | Fabric → Stuffing → Hair → Dress |

---

## Troubleshooting

### Character won't move
- Make sure scene is in Play mode
- Check that PlayerController.moveSpeed > 0
- Verify isPlayerControlled = true

### Workstation doesn't respond
- Get closer (within 2 units)
- Make sure station isn't occupied
- Check stationType is set correctly

### UI not updating
- Ensure UIManager has all Text references set
- Verify managers are in the scene

### AI not working
- Check ToyManager is generating toys
- Verify workstations have componentProduced set
- Make sure AI has isAIControlled = true

### Character swap not working
- Verify CharacterSwapper has both controllers assigned
- Check both controllers are in the scene

---

## File Guide

### Documentation
- `README.md` - Project overview
- `SETUP.md` - Detailed setup instructions
- `DESIGN.md` - Game design document
- `IMPLEMENTATION_SUMMARY.md` - Requirements verification
- `QUICKSTART.md` - This file

### Core Scripts
- `GameManager.cs` - Main game loop and timer
- `ToyManager.cs` - Toy generation and tracking
- `UIManager.cs` - UI updates
- `CharacterSwapper.cs` - Swap mechanic

### Character Scripts
- `PlayerController.cs` - Player input
- `AIController.cs` - AI behavior

### Gameplay Scripts
- `Toy.cs` - Toy data structure
- `WorkStation.cs` - Crafting logic

### Visual Scripts
- `WorkStationVisuals.cs` - Progress bars
- `CharacterVisuals.cs` - Character feedback

### Utility Scripts
- `QuickSceneSetup.cs` - Testing tool
- `GameTests.cs` - Unit tests
- `WorkStationSetup.cs` - Reference guide

---

## Next Steps

1. ✅ **Set up scene** (follow Option 1 or 2 above)
2. ✅ **Test gameplay** (Press Play and try making a toy)
3. ✅ **Try character swap** (Press SPACE while playing)
4. ✅ **Complete all 5 toys** (See your final time!)
5. 🎨 **Add polish** (Custom models, sounds, effects)
6. 🎄 **Christmas theme** (Decorations, music, particles)

---

## Support

For detailed information, see:
- **Scene Setup:** `SETUP.md`
- **Game Design:** `DESIGN.md`
- **Implementation Details:** `IMPLEMENTATION_SUMMARY.md`

---

Enjoy making toys for Santa! 🎅🎁
