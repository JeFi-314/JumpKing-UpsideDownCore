using JumpKing.PauseMenu.BT.Actions;

namespace UpsideDownCore.Menu
{
    public class ToggleReverseGravity : ITextToggle
    {
        public ToggleReverseGravity() : base(UpsideDownCore.isRevertGravity)
        {
        }

        protected override string GetName() => "Revert Gravity";

        protected override void OnToggle()
        {
            UpsideDownCore.isRevertGravity = toggle;
        }
    }
}
