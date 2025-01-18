using HarmonyLib;
using JumpKing;
using JK = JumpKing.BlockBehaviours;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UpsideDownCore.Patching;
internal class SandBlockBehaviour
{
    public SandBlockBehaviour (Harmony harmony)
    {
        Type type = typeof(JK.SandBlockBehaviour);

        MethodInfo ModifyYVelocity = type.GetMethod(nameof(JK.SandBlockBehaviour.ModifyYVelocity));
        harmony.Patch(
            ModifyYVelocity,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(SandBlockBehaviour), nameof(transpileModifyYVelocity)))
        );

        MethodInfo AdditionalYCollisionCheck = type.GetMethod(nameof(JK.SandBlockBehaviour.AdditionalYCollisionCheck));
        harmony.Patch(
            AdditionalYCollisionCheck,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(SandBlockBehaviour), nameof(transpileAdditionalYCollisionCheck)))
        );

        MethodInfo ExecuteBlockBehaviour = type.GetMethod(nameof(JK.SandBlockBehaviour.ExecuteBlockBehaviour));
        harmony.Patch(
            ExecuteBlockBehaviour,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(SandBlockBehaviour), nameof(transpileExecuteBlockBehaviour)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileModifyYVelocity(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            //`float num = ((IsPlayerOnBlock && bodyComp.Velocity.Y <= 0f) ? 0.5f : 1f);`
            // bodyComp.Velocity.Y <= 0f
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Ldc_R4, (float)0),
            new CodeMatch(OpCodes.Ble_S)
        )
        .ThrowIfInvalid($"Cant find code in {nameof(SandBlockBehaviour)}")
        .Advance(4);
        var label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SandBlockBehaviour), nameof(revLEff))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        matcher.MatchStartForward(
            //`if (!IsPlayerOnBlock && bodyComp.IsOnGround && bodyComp.Velocity.Y > 0f)`
            // bodyComp.Velocity.Y > 0f
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Ldc_R4, (float)0),
            new CodeMatch(OpCodes.Ble_Un_S)
        )
        .ThrowIfInvalid($"Cant find code in {nameof(SandBlockBehaviour)}")
        .Advance(4);
        label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SandBlockBehaviour), nameof(revLEff))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        matcher.MatchStartForward(
            // bodyComp.Position.Y += 1f
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Position")),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Dup),
            new CodeMatch(OpCodes.Ldind_R4),
            new CodeMatch(OpCodes.Ldc_R4, (float)1),
            new CodeMatch(OpCodes.Add),
            new CodeMatch(OpCodes.Stind_R4)
        )
        .ThrowIfInvalid($"Cant find code in {nameof(SandBlockBehaviour)}")
        .Advance(6)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SandBlockBehaviour), nameof(negative)))
        );

        return matcher.Instructions();
    }
    
    private static IEnumerable<CodeInstruction> transpileAdditionalYCollisionCheck(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            // return behaviourContext.BodyComp.Velocity.Y < 0f;
            new CodeMatch(OpCodes.Ldarg_2),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("JumpKing.BodyCompBehaviours.BehaviourContext:BodyComp")),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Ldc_R4, (float) 0),
            new CodeMatch(OpCodes.Clt),
            new CodeMatch(OpCodes.Ret)
        )
        .ThrowIfInvalid($"Cant find code in {nameof(SandBlockBehaviour)}")
        .Advance(5)
        .RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SandBlockBehaviour), nameof(revLTff)))
        );

        return matcher.Instructions();
    }

    private static IEnumerable<CodeInstruction> transpileExecuteBlockBehaviour(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            // bodyComp.Velocity.Y = Math.Min(0.75f, bodyComp.Velocity.Y);
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldc_R4, (float)0.75),
            new CodeMatch(OpCodes.Ldloc_0),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field("JumpKing.Player.BodyComp:Velocity")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Call, AccessTools.Method("System.Math:Min", new Type[] {typeof(float), typeof(float)})),
            new CodeMatch(OpCodes.Stfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y"))
        )
        .ThrowIfInvalid($"Cant find code in {nameof(SandBlockBehaviour)}")
        .Advance(6)
        .RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SandBlockBehaviour), nameof(capYVelocity)))
        );

        return matcher.Instructions();
    }

    private static bool revLEff(float left, float right) {
        return Models.Utils.reverseComparison(left, right, "le", UpsideDownCore.isUpsideDown);
    }
    private static float negative(float value) {
        return Models.Utils.negative(value, UpsideDownCore.isUpsideDown);
    }
    private static bool revLTff(float left, float right) {
        return Models.Utils.reverseComparison(left, right, "lt", UpsideDownCore.isUpsideDown);
    }
    private static float capYVelocity(float cap, float y) {
        return UpsideDownCore.isUpsideDown ? Math.Max(y, -cap) : Math.Min(y, cap);
    }
}