using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class FireTrap : StaticBody2D
{
    [Export] private int _fireDamage = 1;

    private AnimatedSprite _animatedSprite;
    private Area2D _flameArea;
    
    // Trigger Timer is the trigger animation time - 0.4 sec
    // Toggle Timer is the time to toggle on and off - 2 sec 
    private Timer _triggerTimer, _toggleTimer;

    private bool _isOn;
    
	public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _flameArea = GetNode<Area2D>("Area2D");
        _triggerTimer = GetNode<Timer>("TriggerTimer");
        _toggleTimer = GetNode<Timer>("ToggleTimer");
        _animatedSprite.Play("Off");
        _isOn = false;
        _toggleTimer.Start();
    }

    public void OnArea2DAreaShapeEntered(RID areaRid, Area2D area, int bodyShapeIndex, int localShapeIndex)
    {
        if (area.Owner is PlayerTwo playerTwo)
        {
            playerTwo.GetHit(_fireDamage, Math.Sign(playerTwo.Position.x - Position.x));
        }
    }

    public void OnToggleTimerTimeout()
    {
        if (_isOn)
            _flameArea.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        _animatedSprite.Play("Trigger");
        _triggerTimer.Start();
    }

    public void OnTriggerTimerTimeout()
    {
        if (_isOn)
        {
            _animatedSprite.Play("Off");
            _isOn = false;
        }
        else
        {
            _animatedSprite.Play("On");
            _isOn = true;
            _flameArea.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        }
        _toggleTimer.Start();
    }
}
