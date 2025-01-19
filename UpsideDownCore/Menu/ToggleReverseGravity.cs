using JumpKing.PauseMenu.BT.Actions;

namespace UpsideDownCore.Menu;
public class ToggleReverseGravity : ITextToggle
{
    public ToggleReverseGravity() : base(Controller.isReverseGravity)
    {
    }

    protected override string GetName() => "Reverse Gravity";

    protected override void OnToggle()
    {
        Controller.isReverseGravity = toggle;
    }
}
