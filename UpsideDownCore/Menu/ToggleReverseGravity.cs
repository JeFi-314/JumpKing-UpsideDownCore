using HarmonyLib;
using JumpKing.PauseMenu.BT.Actions;

namespace UpsideDownCore.Menu;
public class ToggleReverseGravity : ITextToggle
{
    public readonly static ToggleReverseGravity Instance;

    static ToggleReverseGravity() {
        Instance = new ToggleReverseGravity();
    }

    public static void SetValue(bool value)
    {
        Traverse.Create(Instance).Field<bool>("_toggle").Value = value;
        Instance.OnToggle();
    } 

    public ToggleReverseGravity() : base(Controller.isReverseGravity) {}

    protected override string GetName() => "Reverse Gravity";

    protected override void OnToggle()
    {
        Controller.isReverseGravity = toggle;
    }
}
