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
public class WaterParticleSpawningBehaviour
{
    public WaterParticleSpawningBehaviour (Harmony harmony)
    {
        Type type = AccessTools.TypeByName("JumpKing.BodyCompBehaviours.WaterParticleSpawningBehaviour");
        MethodInfo ExecuteBehaviour = type.GetMethod("ExecuteBehaviour");
        harmony.Patch(
            ExecuteBehaviour,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(WaterParticleSpawningBehaviour), nameof(transpileExecuteBehaviour)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileExecuteBehaviour(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            // if ((!waterBlockBehaviour.IsPlayerOnBlock && bodyComp.Velocity.Y < 0f) || (waterBlockBehaviour.IsPlayerOnBlock && bodyComp.Velocity.Y >= 0f))
            new CodeMatch(OpCodes.Ldloc_1),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.BlockBehaviours.WaterBlockBehaviour:get_IsPlayerOnBlock")),
            new CodeMatch(OpCodes.Brtrue_S),
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Ldc_R4, (float)0),
            new CodeMatch(OpCodes.Blt_S),
            new CodeMatch(OpCodes.Ldloc_1),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.BlockBehaviours.WaterBlockBehaviour:get_IsPlayerOnBlock")),
            new CodeMatch(OpCodes.Brfalse_S),
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Ldc_R4, (float)0),
            new CodeMatch(OpCodes.Blt_Un_S )
        ).ThrowIfInvalid($"Cant find code in {nameof(WaterParticleSpawningBehaviour)}")
        .Advance(7);
        var label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WaterParticleSpawningBehaviour), nameof(revLTff))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        )
        .Advance(7);
        label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WaterParticleSpawningBehaviour), nameof(revLTff))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        return matcher.Instructions();
    }

    private static bool revLTff(float left, float right) {
        return Utils.reverseComparison(left, right, "lt", UpsideDownCore.isUpsideDown);
    }
}