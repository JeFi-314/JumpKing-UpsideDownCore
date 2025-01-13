using HarmonyLib;
using JumpKing;
using JK = JumpKing.Player;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using JumpKing.GameManager;
using System.Diagnostics;
using System.Linq.Expressions;

namespace UpsideDownCore.Patching;
public class Walk
{
    public Walk (Harmony harmony)
    {
        Type type = typeof(JK.Walk);
        MethodInfo MyRun = AccessTools.Method(type, "MyRun");
        harmony.Patch(
            MyRun,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(Walk), nameof(transpileMyRun)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileMyRun(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
		    //`if (base.body.Velocity.Y < 0f && base.body.IsOnBlock(typeof(SandBlock)))`
            // base.body.Velocity.Y < 0f
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Call, AccessTools.Method("JumpKing.Player.PlayerNode:get_body")),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Method("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Method("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Ldc_R4, (float)0),
            new CodeMatch(OpCodes.Bge_Un_S)
        )
        .ThrowIfInvalid($"Cant find code in {nameof(Walk)}")
        .Advance(5);
        var label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Walk), nameof(revGEff))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        return matcher.Instructions();
    }

    private static bool revGEff(float left, float right) {
        return Utils.reverseComparison(left, right, "ge", UpsideDownCore.isUpsideDown);
    }
}

