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
internal class ResolveXCollisionBehaviour
{
    public ResolveXCollisionBehaviour (Harmony harmony)
    {
        Type type = typeof(JK.ResolveXCollisionBehaviour);
        MethodInfo ExecuteBehaviour = type.GetMethod(nameof(JK.ResolveXCollisionBehaviour.ExecuteBehaviour));
        harmony.Patch(
            ExecuteBehaviour,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(ResolveXCollisionBehaviour), nameof(transpileExecuteBehaviour)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileExecuteBehaviour(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            // if (!bodyComp.IsOnGround || bodyComp.Velocity.Y <= 0f)
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Player.BodyComp:get_IsOnGround")),
            new CodeMatch(OpCodes.Brfalse_S),
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Ldc_R4, (float)0),
            new CodeMatch(OpCodes.Bgt_Un)
        ).ThrowIfInvalid($"Cant find code in {nameof(ResolveXCollisionBehaviour)}")
        .Advance(7);
        var label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResolveXCollisionBehaviour), nameof(revGTff))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        matcher.MatchStartForward(
            // if (bodyComp.IsOnGround && Math.Abs(bodyComp.Velocity.X) <= PlayerValues.WALK_SPEED && bodyComp.Velocity.Y > 0f)
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Player.BodyComp:get_IsOnGround")),
            new CodeMatch(OpCodes.Brfalse_S),
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:X")),
            new CodeMatch(OpCodes.Call, AccessTools.Method("System.Math:Abs", new Type[] {typeof(float)})),
            new CodeMatch(OpCodes.Call, AccessTools.Method("JumpKing.PlayerValues:get_WALK_SPEED")),
            new CodeMatch(OpCodes.Bgt_Un_S ),
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Ldc_R4, (float)0),
            new CodeMatch(OpCodes.Ble_Un_S)
        ).ThrowIfInvalid($"Cant find code in {nameof(ResolveXCollisionBehaviour)}")
        .Advance(13);
        label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResolveXCollisionBehaviour), nameof(revLEff))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        return matcher.Instructions();
    }

    private static bool revGTff(float left, float right) {
        return Models.Utils.reverseComparison(left, right, "gt", UpsideDownCore.isUpsideDown);
    }
    private static bool revLEff(float left, float right) {
        return Models.Utils.reverseComparison(left, right, "le", UpsideDownCore.isUpsideDown);
    }
}