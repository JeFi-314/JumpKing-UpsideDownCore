using HarmonyLib;
using JK = JumpKing.BodyCompBehaviours;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using UpsideDownCore.Models;

namespace UpsideDownCore.Patching;
internal class ApplyGravityBehaviour
{
    public ApplyGravityBehaviour (Harmony harmony)
    {
        Type type = typeof(JK.ApplyGravityBehaviour);
        MethodInfo ExecuteBehaviour = type.GetMethod(nameof(JK.ApplyGravityBehaviour.ExecuteBehaviour));
        harmony.Patch(
            ExecuteBehaviour,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(ApplyGravityBehaviour), nameof(transpileExecuteBehaviour)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileExecuteBehaviour(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            // bodyComp.Velocity.Y += num
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Dup),
            new CodeMatch(OpCodes.Ldind_R4),
            new CodeMatch(OpCodes.Ldloc_1),
            new CodeMatch(OpCodes.Add),
            new CodeMatch(OpCodes.Stind_R4)
        ).ThrowIfInvalid($"Cant find code in {nameof(ApplyGravityBehaviour)}")
        .Advance(6)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ApplyGravityBehaviour), nameof(captureGravity)))
        )
        .MatchStartForward(
            // bodyComp.Velocity.Y = Math.Min(bodyComp.Velocity.Y, PlayerValues.MAX_FALL);
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Call, AccessTools.Method("JumpKing.PlayerValues:get_MAX_FALL")),
            new CodeMatch(OpCodes.Call, AccessTools.Method("System.Math:Min", new Type[]{typeof(float), typeof(float)})),
            new CodeMatch(OpCodes.Stfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y"))
        ).ThrowIfInvalid($"Cant find code in {nameof(ApplyGravityBehaviour)}")
        .Advance(6)
        .RemoveInstruction().InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ApplyGravityBehaviour), nameof(capYVelocity)))
        );

        return matcher.Instructions();
    }

    private static float captureGravity(float originGravity) {
        float finalGravity = Models.Utils.negative(originGravity, Manager.isReverseGravity);
        Manager.gravity = finalGravity;
        return finalGravity;
    }

    private static float capYVelocity(float y, float cap) {
        return Manager.isReverseGravity ? Math.Max(y, -cap) : Math.Min(y, cap);
    }
}









