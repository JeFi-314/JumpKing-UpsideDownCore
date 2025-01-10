using HarmonyLib;
using JumpKing;
using JK = JumpKing.Player;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using JumpKing.GameManager;
using System.Diagnostics;

namespace UpsideDownCore.Patching;
public class FailState
{
    public FailState (Harmony harmony)
    {
        Type type = typeof(JK.FailState);
        MethodInfo MyRun = AccessTools.Method(type, "MyRun");
        harmony.Patch(
            MyRun,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(FailState), nameof(transpileMyRun)))
        );
    }

private static IEnumerable<CodeInstruction> transpileMyRun(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
                // base.body.Velocity.Y > 0f
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Call, AccessTools.Method("JumpKing.Player.PlayerNode:get_body")),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Player.BodyComp:get_LastVelocity")),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Method("Microsoft.Xna.Framework.Vector2:Y")),
                new CodeMatch(OpCodes.Call, AccessTools.Method("JumpKing.PlayerValues:get_MAX_FALL")),
                new CodeMatch(OpCodes.Beq_S)
            )
            .ThrowIfInvalid($"Cant find code in {nameof(FailState)}");
        matcher.Advance(5);
        var label = matcher.Instruction.operand;
        matcher.RemoveInstruction();
        matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FailState), nameof(negative))),
                new CodeInstruction(OpCodes.Beq_S, label)
            );

        return matcher.Instructions();
    }

    private static float negative(float value) {
        return Utils.negative(value, UpsideDownCore.isUpsideDown);
    }
}