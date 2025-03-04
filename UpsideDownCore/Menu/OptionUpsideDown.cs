using JumpKing.PauseMenu.BT.Actions;

namespace UpsideDownCore.Menu;
public class OptionUpsideDown : IOptions
{
    public readonly static OptionUpsideDown Instance;

    static OptionUpsideDown() {
        Instance = new OptionUpsideDown();
    }
    
    public static void SetOption(UpsideDownType value)
    {
        Instance.CurrentOption = (int)value;
    } 

    private OptionUpsideDown() : base(3, (int)Controller.upsideDownType, EdgeMode.Wrap) {}

    protected override bool CanChange()
    {
        return true;
    }

    protected override string CurrentOptionName()
    {
        switch ((UpsideDownType)CurrentOption) {
            case UpsideDownType.Normal: return "Normal";
            case UpsideDownType.Flip: return "Flip";
            case UpsideDownType.Auto: return "Auto";
        }
        return "";
    }

    protected override void OnOptionChange(int option)
    {
        Controller.upsideDownType = (UpsideDownType)option;
        CurrentOption = option;
    }

}
