# UpsideDown Core - A Jump King Core Mod

This Jump King mod allows the player character to flip upside-down while preserving the original game physics. The physics features include:

1. **Basic Movement & Animations**: Walking, jumping, and splatting work seamlessly.
2. **Collision Handling**: Proper handling of ground detection, slope collisions, and block collision precedence.
3. **Sand Blocks**: Players can enter sand blocks from the **bottom** when upside-down.
4. **Player Sprite**: Fully supports player flipping (not compatible with hitbox resizer).
5. **Camera & Water Particles**: Adjusted for the upside-down state.
6. **Maximum Fall Speed**: Preserved from the original gameplay.
7. **Slope Blocks Fix**: Corrects an incorrect bottom-left slope hitbox when the player is upside-down.

The mod also provides a function to generally reverse gravity in the game.

You can find this mod on [Steam Workshop Page](https://steamcommunity.com/sharedfiles/filedetails/?id=3410235901).

---

## Interfaces

The `Controller` class in the root namespace serves as the public interface for this mod. It includes the following members:

### Properties

1. `UpsideDownType upsideDownType`: The current upside-down state. It can be one of three types:

   - `Normal`: Standard gameplay.
   - `Flip`: Upside-down state.
   - `Auto`: Automatically flips the player based on the current gravity value. If `gravity == 0`, the player state remains unchanged.

2. `bool isReverseGravity`: A flag to reverse gravity for the entire game.

(⚠ **Reminder**: Unless it's a special effect, `ExecuteBlockBehaviour` should avoid setting the controller's state when the player is no longer interacting with the block.)

### Methods

1. `void Reset()`: Resets all states to normal gameplay. This method is called at the start of every level (map).

---

## How to Use Core Mod

1. **Download**: Get the latest version from the [releases page](https://github.com/JeFi-314/JumpKing-UpsideDownCore/releases).
2. **Include the DLL in your project**:
   - Place the assembly in a suitable folder (e.g., `<your project folder>/lib`).
   - Reference it in your C# project file as follows:
     ```xml
     <Reference Include="UpsideDownCore">
       <Private>False</Private> <!-- Prevents auto-copy during `dotnet build` -->
       <HintPath>lib/UpsideDownCore.dll</HintPath>
     </Reference>
     ```
   - **Note: Do not include this DLL in your release build!**
3. **Steam Workshop Dependency**: Add [this](https://steamcommunity.com/sharedfiles/filedetails/?id=3410235901) as a dependency for your mod. It will always be updated to the latest version.

---

## Implementation Details

Most features of this mod are achieved by using **Harmony transpilers**. The transpiler locates the IL code in the target methods and modifies it dynamically.  
If the method body is skipped due to prefix patching, the mod's patch will not be applied.  
If other mods also modify the same IL code, this mod may not work correctly (it could crash and be ignored by JumpKing).  

---

## To Do

1. Event hook: Provide a public interface for other mods to register events that respond to upside-down state changes or gravity reversals.
2. Save file: Implement auto-saving for mod state to file.
3. Chagne the implementation of `Controller` from static class to Enitity.

