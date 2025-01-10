using HarmonyLib;
using JumpKing;
using JK = JumpKing.Player;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using JumpKing.GameManager;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UpsideDownCore.Patching;
public class PlayerEntity
{
    public PlayerEntity (Harmony harmony)
    {
        Type type = typeof(JK.PlayerEntity);
        MethodInfo Draw = type.GetMethod("Draw");
        harmony.Patch(
            Draw,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(PlayerEntity), nameof(transpileDraw)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileDraw(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            // Camera.TransformVector2(vector)
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Call, AccessTools.Method("JumpKing.Camera:TransformVector2"))
        ).ThrowIfInvalid($"Cant find code in {nameof(PlayerEntity)}")
        .Advance(2)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlayerEntity), nameof(fixPosition)))
        );

        matcher.MatchStartForward(
            // m_flip
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld,  AccessTools.Method("JumpKing.Player.PlayerEntity:m_flip"))
        ).ThrowIfInvalid($"Cant find code in {nameof(PlayerEntity)}")
        .Advance(2)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlayerEntity), nameof(flipSpiritV)))
        );

        return matcher.Instructions();
    }

    private static Vector2 fixPosition(Vector2 origin) {
        return origin + (UpsideDownCore.isUpsideDown ? new Vector2(0f, 23f) : Vector2.Zero);
    }
    private static int flipSpiritV(int effect) {
        return effect | (UpsideDownCore.isUpsideDown ? (int)SpriteEffects.FlipVertically : 0);
    }
}