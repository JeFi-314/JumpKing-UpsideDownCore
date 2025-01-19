namespace UpsideDownCore;
public static class Controller
{
    public static bool isReverseGravity {get; set;}
    public static UpsideDownType upsideDownType {get; set;}
    static Controller() {
        Reset();
    }

    public static void Reset() {
        isReverseGravity = false;
        upsideDownType = UpsideDownType.Normal;
    }
}
