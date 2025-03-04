using HarmonyLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;


using JumpKing.Mods;
using JumpKing.PauseMenu;

using UpsideDownCore.Menu;

namespace UpsideDownCore;
[JumpKingMod(IDENTIFIER)]
public static class UpsideDownCore
{
    const string IDENTIFIER = "JeFi.UpsideDownCore";
    const string HARMONY_IDENTIFIER = "JeFi.UpsideDownCore.Harmony";

    public static string AssemblyPath { get; set; }

    [BeforeLevelLoad]
    public static void BeforeLevelLoad()
    {
        AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#if DEBUG
        Debugger.Launch();
        Debug.WriteLine("------");
        Harmony.DEBUG = true;
        Environment.SetEnvironmentVariable("HARMONY_LOG_FILE", $@"{AssemblyPath}\harmony.log.txt");
#endif

        Harmony harmony = new Harmony(HARMONY_IDENTIFIER);

        try {
            new Patching.JumpGame(harmony);
            // fix wrong slope hitbox
            new Patching.SlopeBlock(harmony);

            new Patching.Camera(harmony);
            new Patching.LevelScreen(harmony);
            new Patching.WaterParticleSpawningBehaviour(harmony);
            new Patching.ResolveXCollisionBehaviour(harmony);
            new Patching.ResolveYCollisionBehaviour(harmony);
            new Patching.ApplyGravityBehaviour(harmony);
            new Patching.SandBlockBehaviour(harmony);
            new Patching.GuardtowerSoulBugFixBehaviour(harmony);
            new Patching.FailState(harmony);
            new Patching.JumpState(harmony);
            new Patching.AirAnim(harmony);
            new Patching.Walk(harmony);
            new Patching.FollyPlayer(harmony);
            new Patching.PlayerEntity(harmony);
        }
        catch (Exception e) {
            Debug.WriteLine(e.ToString());

            // Debug.WriteLine($"Message: {e.Message}");
            // Debug.WriteLine($"Stack Trace: {e.StackTrace}");

            // if (e.InnerException != null)
            // {
            //         Debug.WriteLine("Inner Exception:");
            //         Debug.WriteLine(e.InnerException.ToString());
            // }
        }

#if DEBUG
        Environment.SetEnvironmentVariable("HARMONY_LOG_FILE", null);
#endif
    }

    [OnLevelStart]
    public static void OnLevelStart()
    {
        // reset Controller
        Controller.Reset();
        
#if DEBUG
        OptionUpsideDown.SetOption(UpsideDownType.Flip);
        ToggleReverseGravity.SetValue(true);
#endif
    }

#if DEBUG
    #region Menu Items
    [PauseMenuItemSetting]
    [MainMenuItemSetting]
    public static OptionUpsideDown GetOptionUpsideDown(object factory, GuiFormat format)
    {
        return OptionUpsideDown.Instance;
    }

    [PauseMenuItemSetting]
    [MainMenuItemSetting]
    public static ToggleReverseGravity GetToggleReverseGravity(object factory, GuiFormat format)
    {
        return ToggleReverseGravity.Instance;
    }
    #endregion
#endif
}
