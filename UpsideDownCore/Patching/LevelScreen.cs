using HarmonyLib;
using JumpKing;
using JK = JumpKing.Level;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using JumpKing.GameManager;
using System.Diagnostics;
using JumpKing.Level;
using UpsideDownCore.Models;

namespace UpsideDownCore.Patching;
internal class LevelScreen
{
    public LevelScreen (Harmony harmony)
    {
        Type type = typeof(JK.LevelScreen);
        MethodInfo TryCollision = type.GetMethod(nameof(JK.LevelScreen.TryCollision));
        harmony.Patch(
            TryCollision,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(LevelScreen), nameof(transpileTryCollision)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileTryCollision(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            //`if (slopeBlock == null || block2.GetRect().Y < slopeBlock.GetRect().Y)`
            // block2.GetRect().Y < slopeBlock.GetRect().Y
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Level.IBlock:GetRect")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Rectangle:Y")),
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Level.SlopeBlock:GetRect")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Rectangle:Y")),
            new CodeMatch(OpCodes.Bge_S)
        ).ThrowIfInvalid($"Cant find code in {nameof(LevelScreen)}")
        .Advance(6);
        var label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LevelScreen), nameof(revGEii))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        matcher.MatchStartForward(
            //`else if (block == null || block2.GetRect().Y < block.GetRect().Y)`
            // bloblock2.GetRect().Y < block.GetRect().Y
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Level.IBlock:GetRect")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Rectangle:Y")),
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Level.IBlock:GetRect")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Rectangle:Y")),
            new CodeMatch(OpCodes.Bge_S)
        ).ThrowIfInvalid($"Cant find code in {nameof(LevelScreen)}")
        .Advance(6);
        label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LevelScreen), nameof(revGEii))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        );

        matcher.MatchStartForward(
            //`if (block != null && slopeBlock != null && block.GetRect().Y <= slopeBlock.GetRect().Y 
            //&& (slopeBlock.GetSlopeType() == SlopeType.TopLeft || slopeBlock.GetSlopeType() == SlopeType.TopRight))`
            // block != null && slopeBlock != null 
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Brfalse_S),
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Brfalse_S),
            // block.GetRect().Y <= slopeBlock.GetRect().Y 
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Level.IBlock:GetRect")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Rectangle:Y")),
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Level.SlopeBlock:GetRect")),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("Microsoft.Xna.Framework.Rectangle:Y")),
            new CodeMatch(OpCodes.Bgt_S),
            // slopeBlock.GetSlopeType() == SlopeType.TopLeft || slopeBlock.GetSlopeType() == SlopeType.TopRight)
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Level.SlopeBlock:GetSlopeType")),
            new CodeMatch(OpCodes.Brfalse_S),
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method("JumpKing.Level.SlopeBlock:GetSlopeType")),
            new CodeMatch(OpCodes.Ldc_I4_1),
            new CodeMatch(OpCodes.Bne_Un_S)
        ).ThrowIfInvalid($"Cant find code in {nameof(LevelScreen)}")
        .Advance(10);
        label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LevelScreen), nameof(revGTii))),
            new CodeInstruction(OpCodes.Brtrue_S, label)
        )
        .Advance(2)
        .RemoveInstructions(4);
        label = matcher.Instruction.operand;
        matcher.RemoveInstruction()
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LevelScreen), nameof(flipSlopeType))),
            new CodeInstruction(OpCodes.Brfalse_S, label)
        );

        return matcher.Instructions();
    }

    private static bool revGEii(int left, int right) {
        return Models.Utils.reverseComparison(left, right, "ge", Manager.isUpsideDown);
    }
    private static bool revGTii(int left, int right) {
        return Models.Utils.reverseComparison(left, right, "gt", Manager.isUpsideDown);
    }
    private static bool flipSlopeType(SlopeType type) {
        if (Manager.isUpsideDown) {
            return type==SlopeType.BottomLeft || type==SlopeType.BottomRight;
        } else {
            return type==SlopeType.TopLeft || type==SlopeType.TopRight;
        }
    }
}
