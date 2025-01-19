using HarmonyLib;
using JumpKing;
using JK = JumpKing.Player;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using JumpKing.GameManager;
using System.Diagnostics;
using UpsideDownCore.Models;

namespace UpsideDownCore.Patching;
internal class JumpState
{
    public JumpState (Harmony harmony)
    {
        Type type = typeof(JK.JumpState);
        MethodInfo DoJump = AccessTools.Method(type, "DoJump");
        harmony.Patch(
            DoJump,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(JumpState), nameof(transpileDoJump)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileDoJump(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            // base.body.Velocity.Y = JUMP_STRENGTH * p_intensity;
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Call, AccessTools.Method("JumpKing.Player.PlayerNode:get_body")),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Call, AccessTools.Method("JumpKing.Player.JumpState:get_JUMP_STRENGTH")),
            new CodeMatch(OpCodes.Ldarg_1),
            new CodeMatch(OpCodes.Mul),
            new CodeMatch(OpCodes.Stfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y"))
        ).ThrowIfInvalid($"Cant find code in {nameof(JumpState)}")
        .Advance(5)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JumpState), nameof(negative)))
        );

        return matcher.Instructions();
    }

    private static float negative(float value) {
        return Models.Utils.negative(value, Manager.isUpsideDown);
    }}