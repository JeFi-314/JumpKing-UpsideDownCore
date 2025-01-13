using HarmonyLib;
using JumpKing;
using JK = JumpKing.BodyCompBehaviours;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using JumpKing.GameManager;
using System.Diagnostics;

namespace UpsideDownCore.Patching;
public class ResolveYCollisionBehaviour
{
    public ResolveYCollisionBehaviour (Harmony harmony)
    {
        Type type = typeof(JK.ResolveYCollisionBehaviour);
        MethodInfo ExecuteBehaviour = type.GetMethod(nameof(JK.ResolveYCollisionBehaviour.ExecuteBehaviour));
        harmony.Patch(
            ExecuteBehaviour,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(ResolveYCollisionBehaviour), nameof(transpileExecuteBehaviour)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileExecuteBehaviour(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            // if (num > 0)
            new CodeMatch(OpCodes.Ldloc_2),
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(OpCodes.Ble)
        )
        .ThrowIfInvalid($"Cant find code in {nameof(ResolveYCollisionBehaviour)}")
        .Advance(2);
        var label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResolveYCollisionBehaviour), nameof(revLEif))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        matcher.MatchStartForward(
            //`if (info.SlopeType == SlopeType.None || bodyComp.Velocity.Y <= 0f || bodyComp.IsOnGround)`
            // bodyComp.Velocity.Y <= 0f
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Ldc_R4, (float)0),
            new CodeMatch(OpCodes.Ble_S)
        )
        .ThrowIfInvalid($"Cant find code in {nameof(ResolveYCollisionBehaviour)}")
        .Advance(4);
        label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResolveYCollisionBehaviour), nameof(revLEff))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        matcher.MatchStartForward(
            //`if (info.Quark && bodyComp.Velocity.Y == PlayerValues.MAX_FALL)`
            // bodyComp.Velocity.Y == PlayerValues.MAX_FALL
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Call, AccessTools.Method("JumpKing.PlayerValues:get_MAX_FALL")),
            new CodeMatch(OpCodes.Bne_Un_S)
        )
        .ThrowIfInvalid($"Cant find code in {nameof(ResolveYCollisionBehaviour)}")
        .Advance(4)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResolveYCollisionBehaviour), nameof(negative)))
        );

        return matcher.Instructions();
    }

    // CAUSION! local variable num is int not float
    private static bool revLEif(int left, float right) {
        return Utils.reverseComparison((float)left, right, "le", UpsideDownCore.isUpsideDown);
    }
    private static bool revLEff(float left, float right) {
        return Utils.reverseComparison(left, right, "le", UpsideDownCore.isUpsideDown);
    }
    private static float negative(float value) {
        return Utils.negative(value, UpsideDownCore.isUpsideDown);
    }
}