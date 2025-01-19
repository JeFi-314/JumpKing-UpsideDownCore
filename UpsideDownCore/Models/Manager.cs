namespace UpsideDownCore.Models;

internal static class Manager
{
    public static bool isReverseGravity;
    public static bool isUpsideDown;
    public static float gravity;
    static Manager() {
        isReverseGravity = false;
        isUpsideDown = false;
    }

    public static void Update() {
        isReverseGravity = Controller.isReverseGravity;

        switch (Controller.upsideDownType) {
            case UpsideDownType.Normal:
                isUpsideDown = false;
                break;
            case UpsideDownType.Flip:
                isUpsideDown = true;
                break;
            case UpsideDownType.Auto:
                if (gravity>0) isUpsideDown = false;
                else if (gravity<0) isUpsideDown = true;
                break;
        }
    }
}