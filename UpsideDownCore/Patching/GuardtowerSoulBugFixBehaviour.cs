using HarmonyLib;
using JK = JumpKing.BodyCompBehaviours;

using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using System;
using UpsideDownCore.Models;

namespace UpsideDownCore.Patching;
internal class GuardtowerSoulBugFixBehaviour
{
    public GuardtowerSoulBugFixBehaviour (Harmony harmony)
    {
        Type type = typeof(JK.GuardtowerSoulBugFixBehaviour);
        MethodInfo ExecuteBehaviour = type.GetMethod(nameof(JK.GuardtowerSoulBugFixBehaviour.ExecuteBehaviour));
        harmony.Patch(
            ExecuteBehaviour,
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(GuardtowerSoulBugFixBehaviour), nameof(transpileExecuteBehaviour)))
        );
    }

    private static IEnumerable<CodeInstruction> transpileExecuteBehaviour(IEnumerable<CodeInstruction> instructions , ILGenerator generator) {
        CodeMatcher matcher = new CodeMatcher(instructions , generator);

        matcher.MatchStartForward(
            //`int num2 = (flag2 ? ((int)behaviourContext["YStep"]) : (-1));`
            // ((int)behaviourContext["YStep"])
            new CodeMatch(OpCodes.Ldarg_1),
            new CodeMatch(OpCodes.Ldstr, "YStep"),
            new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Dictionary<string, object>), "Item")),
            new CodeMatch(OpCodes.Unbox_Any, typeof(int))
        ).ThrowIfInvalid($"Cant find code in {nameof(GuardtowerSoulBugFixBehaviour)}")
        .Advance(4)
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GuardtowerSoulBugFixBehaviour), nameof(negative)))
        );

        return matcher.Instructions();
    }

    private static int negative(int value) {
        return (int)Models.Utils.negative(value, Manager.isUpsideDown);
    }
}









