using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class PlatformMoving : KinematicBody2D // Moving Platform Using MoveAndSlide (Need to update player position)
{
    [Export] private float _moveSpeed;
    [Export] private bool _isVertical;
    [Export] private bool _isHorizontal;

    private Timer _waitTimer, _moveTimer;
    private bool _isWaiting;
    private int _direction = 1;
    private Vector2 _velocity = Vector2.Zero;

    public override void _Ready()
    {
        _isWaiting = true;
        _waitTimer = GetNode<Timer>("WaitTimer");
        _moveTimer = GetNode<Timer>("MoveTimer");
        _waitTimer.Start();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_isWaiting) return;
        if (_isHorizontal)
        {
            _velocity.x = _moveSpeed * _direction;
        }
        else if (_isVertical)
        {
            _velocity.y = _moveSpeed * _direction;
        }
        MoveAndSlideWithSnap(_velocity, Vector2.Zero, Vector2.Up, infiniteInertia: true);
    }

    // Signals
    public void OnMovementTimerTimeout()
    {
        _direction = -1 * _direction;
        _isWaiting = true;
        _waitTimer.Start();
    }

    public void OnWaitTimerTimeout()
    {
        _isWaiting = false;
        _moveTimer.Start();
    }
}
