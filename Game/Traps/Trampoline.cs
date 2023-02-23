using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Trampoline : StaticBody2D
{
    [Export] private float _bounceSpeed = -600;

    private AnimatedSprite _animatedSprite;
    private Timer _bounceTimer;

    private bool _isActive = true;
	
	public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _bounceTimer = GetNode<Timer>("BounceTimer");
        
        _animatedSprite.Play("Idle");
    }

    // Signal
    public void OnArea2DBodyShapeEntered(RID bodyRid, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        if (!(body is PlayerTwo player) || !_isActive)
            return;
        
        _isActive = false;
        player.BouncePlayer(_bounceSpeed);
        _animatedSprite.Play("Bounce");
        _bounceTimer.Start();
    }

    public void OnBounceTimerTimeout()
    {
        _isActive = true;
        _animatedSprite.Play("Idle");
    }
}
