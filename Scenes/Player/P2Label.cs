using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class P2Label : Label
{
    [Export] private bool debugState = false;

    public override void _Ready()
    {
        if (debugState)
        {
            GetParent().Connect("OnStateChange", this, nameof(OnPlayerStateChange));
        }

    }

    private void OnPlayerStateChange(string value)
    {
        //GD.Print("State: " + value);
        this.Text = value;
    }
}
