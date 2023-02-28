using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class RockHead : Node2D
{
    [Export] private int _spikeHeadDamage = 1;
    [Export] private float _moveSpeed = 2000f;
    [Export] private float _idleDuration = 2.0f;
    [Export] private NodePath[] _movePositions;
    [Export] private float _distanceThreshold = 2.0f;
    [Export] private float _animationDuration = 0.4f;

    private int _currentPosIndex;

    private KinematicBody2D _spikeHead;
    private Tween _moveTween;
    private AnimatedSprite _animatedSprite;
    private Timer _timer, _smashTimer;
    private RayCast2D _left, _right, _up, _down;

    private State _nextState;
    private Direction _collisionDirection;
    private Vector2 _moveDirection;
    private enum State { Idle, Tween, Move };
    private enum Direction { Left, Right, Up, Down };

    private Position2D[] _moveToPositions;
    private Vector2 _currentPos;
    private bool _isColliding;

    
    public override void _Ready()
    {
        _spikeHead = GetNode<KinematicBody2D>("KinematicBody2D");
        _animatedSprite = GetNode<AnimatedSprite>("KinematicBody2D/AnimatedSprite");
        _moveTween = GetNode<Tween>("MoveTween");
        _timer = GetNode<Timer>("Timers/SimpleTimer");
        _smashTimer = GetNode<Timer>("Timers/SmashTimer");

        _left = GetNode<RayCast2D>("KinematicBody2D/RayCasts/Left");
        _right = GetNode<RayCast2D>("KinematicBody2D/RayCasts/Right");
        _up = GetNode<RayCast2D>("KinematicBody2D/RayCasts/Up");
        _down = GetNode<RayCast2D>("KinematicBody2D/RayCasts/Down");

        if (_movePositions.Length <= 0)
        {
            GD.PrintErr("No Move Positions are set for ::" + this.Name);
        }
        _moveToPositions = new Position2D[_movePositions.Length];
        var i = 0;
        foreach (var item in _movePositions)
        {
            _moveToPositions[i++] = GetNode<Position2D>(item);
        }
        _animatedSprite.Play("Idle");
        _nextState = State.Idle;
        _timer.Start();
        _currentPos = Vector2.Zero;
    }
    
    public override void _PhysicsProcess(float delta)
    {
        UpdateMoveDirection();
        UpdateRayCastCollisions();
        if (_isColliding)
        {
            _isColliding = false;
            switch (_collisionDirection)
            {
                case Direction.Left:
                    _animatedSprite.Play("Left");
                    _smashTimer.Start();
                    break;
                case Direction.Right:
                    _animatedSprite.Play("Right");
                    _smashTimer.Start();
                    break;
                case Direction.Up:
                    _animatedSprite.Play("Up");
                    _smashTimer.Start();
                    break;
                case Direction.Down:
                    _animatedSprite.Play("Down");
                    _smashTimer.Start();
                    break;
            }
        }
        if (_nextState != State.Tween)
            return;
        _nextState = State.Move;
        StartMoveTween();
    }

    private void StartMoveTween()
    {
        var duration = _currentPos.DistanceTo(_moveToPositions[_currentPosIndex].Position) / _moveSpeed * 16;
        _moveTween.InterpolateProperty(
            _spikeHead,
            "position",
            _currentPos,
            _moveToPositions[_currentPosIndex].Position,
            duration,
            Tween.TransitionType.Linear,
            Tween.EaseType.InOut,
            _idleDuration
        );
        _moveTween.Start(); // Starting Movement
        _timer.WaitTime = duration;
        _timer.Start();
        _currentPos = _moveToPositions[_currentPosIndex].Position;
        if (_currentPosIndex == _moveToPositions.Length - 1)
            _currentPosIndex = 0;
        else
            _currentPosIndex++;
    }

    private void UpdateMoveDirection()
    {
        _moveDirection = _currentPos.DirectionTo(_moveToPositions[_currentPosIndex].Position);
    }

    private void UpdateRayCastCollisions()
    {
        if (_left.IsColliding() && _moveDirection == Vector2.Up)
        {
            GD.Print("Left");
            _collisionDirection = Direction.Left;
            _isColliding = true;
        }
        else if (_right.IsColliding() && _moveDirection == Vector2.Down)
        {
            GD.Print("Right");
            _collisionDirection = Direction.Right;
            _isColliding = true;
        }
        else if (_up.IsColliding() && _moveDirection == Vector2.Right)
        {
            GD.Print("Up");
            _collisionDirection = Direction.Up;
            _isColliding = true;
        }
        else if (_down.IsColliding() && _moveDirection == Vector2.Left)
        {
            GD.Print("Down");
            _collisionDirection = Direction.Down;
            _isColliding = true;
        }
        else
        {
            _isColliding = false;
        }
    }

    // Signals

    public void OnSimpleTimerTimeout()
    {
        switch (_nextState) // checking the current state and transition to next state
        {
            case State.Idle:
                _animatedSprite.Play("Idle");
                _nextState = State.Tween;
                _timer.Start();
                break;

            case State.Move:
                _nextState = State.Idle;
                _timer.WaitTime = _idleDuration;
                _timer.Start();
                break;
        }

    }

    public void OnSmashTimerTimeout()
    {
        _animatedSprite.Play("Idle");
    }
    
}
