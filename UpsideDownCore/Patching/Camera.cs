using HarmonyLib;
using JK = JumpKing;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using Microsoft.Xna.Framework;
using UpsideDownCore.Models;

namespace UpsideDownCore.Patching;
internal class Camera
{
    public Camera (Harmony harmony)
    {
        Type type = typeof(JK.Camera);
        Type[] parameterTypes = new Type[]
        {
            typeof(Point),
            typeof(Vector2),
            typeof(bool)
        };
        MethodInfo UpdateCameraWithVelocity = AccessTools.Method(type, "UpdateCameraWithVelocity", parameterTypes);
        harmony.Patch(
            UpdateCameraWithVelocity,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(Camera), nameof(transpileUpdateCameraWithVelocity)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileUpdateCameraWithVelocity(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            //`if (Math.Sign(value) != Math.Sign(0f - p_velocity.Y) || (p_velocity.Y < 0f && Math.Abs(p_velocity.Y) < 3f))`
            // p_velocity.Y < 0f
            new CodeMatch(OpCodes.Ldarg_1),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Vector2:Y")),
            new CodeMatch(OpCodes.Ldc_R4, (float)0),
            new CodeMatch(OpCodes.Bge_Un_S)
        )
        .ThrowIfInvalid($"Cant find code in {nameof(Camera)}")
        .Advance(3);
        var label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .Insert(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Camera), nameof(revGEff))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        return matcher.Instructions();
    }

    private static bool revGEff(float left, float right) {
        return Models.Utils.reverseComparison(left, right, "ge", Manager.isUpsideDown);
    }
}