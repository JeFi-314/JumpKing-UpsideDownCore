namespace UpsideDownCore;
public static class Controller
{
    private static bool _isReverseGravity;
    public static bool isReverseGravity {
        get; 
        set;
    }
    private static bool _upsideDownType;
    public static UpsideDownType upsideDownType {
        get; 
        set;
    }
    
    static Controller() {
        Reset();
    }

    public static void Reset() {
        isReverseGravity = false;
        upsideDownType = UpsideDownType.Normal;
    }
}
