using JumpKing.PauseMenu.BT.Actions;

namespace UpsideDownCore.Menu;
public class ToggleUpsideDown : ITextToggle
{
    public ToggleUpsideDown() : base(UpsideDownCore.isUpsideDown)
    {
    }

    protected override string GetName() => "Upside-Down";

    protected override void OnToggle()
    {
        UpsideDownCore.isUpsideDown = toggle;
    }
}
