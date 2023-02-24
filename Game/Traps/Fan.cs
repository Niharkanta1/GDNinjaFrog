using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Fan : StaticBody2D
{

    public override void _Ready()
    {

    }


    // Signals

    public void OnArea2DBodyShapeEntered(RID bodyRid, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        if (!(body is PlayerTwo player))
            return;
        player.SetPlayerIsFlying(true);
    }

    public void OnArea2DBodyShapeExited(RID bodyRid, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        if (!(body is PlayerTwo player))
            return;
        player.SetPlayerIsFlying(false);
    }
}
