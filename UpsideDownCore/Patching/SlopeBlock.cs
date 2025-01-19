using HarmonyLib;
using JumpKing;
using JK = JumpKing.Level;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using JumpKing.GameManager;
using System.Diagnostics;
using ErikMaths;
using JumpKing.Level;
using Microsoft.Xna.Framework;

namespace UpsideDownCore.Patching;
internal class SlopeBlock
{
    static Line[] replaceLines;
    public SlopeBlock (Harmony harmony)
    {
        Type targetType = typeof(JK.SlopeBlock);
        Type interfaceType = typeof(JK.IBlock);
        InterfaceMapping map = targetType.GetInterfaceMap(interfaceType);
        MethodInfo interfaceMethod = typeof(JK.IBlock).GetMethod(nameof(JK.IBlock.Intersects));
        MethodInfo Intersects = null;
        for (int i = 0; i < map.InterfaceMethods.Length; i++)
        {
            if (map.InterfaceMethods[i] == interfaceMethod)
            {
                Intersects = map.TargetMethods[i];
                break;
            }
        }

        harmony.Patch(
            Intersects,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(SlopeBlock), nameof(transpileIntersects)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileIntersects(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            // bool flag = LineUtil.LineShapeIntersect(m_lines, LineUtil.CreateLinesFromRectangle(p_hitbox), out intersections);
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("JumpKing.Level.SlopeBlock:m_lines")),
            new CodeMatch(OpCodes.Ldarg_1),
            new CodeMatch(OpCodes.Call, AccessTools.Method("ErikMaths.LineUtil:CreateLinesFromRectangle")),
            new CodeMatch(OpCodes.Ldloca_S),
            new CodeMatch(OpCodes.Call, AccessTools.Method("ErikMaths.LineUtil:LineShapeIntersect")),
            new CodeMatch(OpCodes.Stloc_1)
        ).ThrowIfInvalid($"Cant find code in {nameof(SlopeBlock)}")
        .Advance(2)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SlopeBlock), nameof(slopeHitboxBugFix)))
        ).MatchStartForward(
            // Line[] lines = m_lines;
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field("JumpKing.Level.SlopeBlock:m_lines")),
            new CodeMatch(OpCodes.Stloc_S)
        ).ThrowIfInvalid($"Cant find code in {nameof(SlopeBlock)}")
        .Advance(2)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SlopeBlock), nameof(slopeHitboxBugFix)))
        );

        return matcher.Instructions();
    }

    private static Line[] slopeHitboxBugFix(Line[] lines, SlopeBlock slopeBlock) {
        if (UpsideDownCore.isUpsideDown && (SlopeType)AccessTools.Field(typeof(JK.SlopeBlock), "m_type").GetValue(slopeBlock) == SlopeType.BottomLeft) {
            replaceLines = new Line[3];
            var box = (Rectangle)AccessTools.Field(typeof(JK.SlopeBlock), "m_box").GetValue(slopeBlock);
            box.Deconstruct(out var x, out var y, out var width, out var height);
            Point p1 = new Point(x, y);
			Point p2 = new Point(x + width, y);
			Point p3 = new Point(x + width, y + height);
            replaceLines[0] = new Line
            {
                p0 = p1,
                p1 = p2
            };
            replaceLines[1] = new Line
            {
                p0 = p2,
                p1 = p3
            };
            replaceLines[2] = new Line
            {
                p0 = p3,
                p1 = p1
            };
            return replaceLines;
        }
        else {
            return lines;
        }
    }
}

