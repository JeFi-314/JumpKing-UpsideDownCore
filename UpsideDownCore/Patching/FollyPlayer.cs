using HarmonyLib;
using JumpKing;
using JK = JumpKing.GameManager.TitleScreen;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using JumpKing.GameManager;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UpsideDownCore.Patching;
internal class FollyPlayer
{
    public FollyPlayer (Harmony harmony)
    {
        Type type = AccessTools.TypeByName("JumpKing.GameManager.TitleScreen.FollyPlayer");
        MethodInfo Draw = type.GetMethod("Draw");
        harmony.Patch(
            Draw,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(FollyPlayer), nameof(transpileDraw)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileDraw(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        //`m_sprite.Draw(m_center.ToVector2(), m_effect);`
        matcher.MatchStartForward(
            // m_center.ToVector2()
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Method("JumpKing.GameManager.TitleScreen.FollyPlayer:m_center")),
            new CodeMatch(OpCodes.Call, AccessTools.Method("Microsoft.Xna.Framework.Point:ToVector2"))
        ).ThrowIfInvalid($"Cant find code in {nameof(FollyPlayer)}")
        .Advance(3)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FollyPlayer), nameof(fixPosition)))
        )
        .MatchStartForward(
            // m_effect
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld,  AccessTools.Method("JumpKing.GameManager.TitleScreen.FollyPlayer:m_effect"))
        ).ThrowIfInvalid($"Cant find code in {nameof(FollyPlayer)}")
        .Advance(2)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FollyPlayer), nameof(flipSpiritV)))
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