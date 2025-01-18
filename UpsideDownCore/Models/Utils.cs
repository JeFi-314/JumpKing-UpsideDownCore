using System;

namespace UpsideDownCore.Models;
internal static class Utils
{
    public static bool reverseComparison(float left, float right, string opcode, bool reverse) {
        if (reverse) {
            switch (opcode) {
                case "gt": opcode = "lt"; break;
                case "lt": opcode = "gt"; break;
                case "ge": opcode = "le"; break;
                case "le": opcode = "ge"; break;
                default: throw new Exception("Invalid opcode");
            }
        }

        switch (opcode) {
            case "gt":
                return left > right;
            case "lt":
                return left < right;
            case "ge":
                return left >= right;
            case "le":
                return left <= right;
            default:
                throw new Exception("Invalid opcode");
        }
    }
    public static float negative(float value, bool neg) {
        return neg ? -value : value;
    }
}
