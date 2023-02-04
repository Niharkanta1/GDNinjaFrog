using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Label : Godot.Label
{
    [Export] private bool debugState = false;

    public override void _Ready()
    {
        if (debugState)
            GetParent().Connect("OnStateChange", this, nameof(OnPlayerStateChange));
    }

    private void OnPlayerStateChange(string value)
    {
        this.Text = value;
    }
}
