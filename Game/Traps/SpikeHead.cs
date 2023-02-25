using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class SpikeHead : Node2D
{
    [Export] private int _spikeHeadDamage = 1;
    [Export] private float _moveSpeed = 6000f;
    [Export] private float _idleDuration = 2.0f;
    [Export] private NodePath[] _movePositions;
    [Export] private float _distanceThreshold = 2.0f;
    [Export] private float _animationDuration = 0.4f;

    private int currentPosIndex = 0;

    private KinematicBody2D _spikeHead;
    private Tween _moveTween;
    private AnimatedSprite _animatedSprite;
    private Timer _timer;
    private RayCast2D _left, _right, _up, _down;

    private State _state;
    private Direction _collisionDirection, _moveDirection;
    private enum State { Idle, Blink, Tween, Move, Smash };
    private enum Direction { Left, Right, Up, Down };

    private Position2D[] _moveToPositions;
    private Vector2 _currentPos;

    public override void _Ready()
    {
        _spikeHead = GetNode<KinematicBody2D>("KinematicBody2D");
        _animatedSprite = GetNode<AnimatedSprite>("KinematicBody2D/AnimatedSprite");
        _moveTween = GetNode<Tween>("MoveTween");
        _timer = GetNode<Timer>("Timer");

        _left = GetNode<RayCast2D>("KinematicBody2D/RayCasts/Left");
        _right = GetNode<RayCast2D>("KinematicBody2D/RayCasts/Right");
        _up = GetNode<RayCast2D>("KinematicBody2D/RayCasts/Up");
        _down = GetNode<RayCast2D>("KinematicBody2D/RayCasts/Down");

        if (_movePositions.Length <= 0)
        {
            GD.PrintErr("No Move Positions are set for ::" + this.Name);
        }
        _moveToPositions = new Position2D[_movePositions.Length];
        int i = 0;
        foreach (var item in _movePositions)
        {
            _moveToPositions[i++] = GetNode<Position2D>(item);
        }
        GD.Print("Move To positions size::" + _moveToPositions.Length);
        _animatedSprite.Play("Idle");
        _state = State.Idle;
        _timer.Start();
        _currentPos = Vector2.Zero;
    }

    public override void _PhysicsProcess(float delta)
    {
        UpdateMoveDirection();
        UpdateRayCastCollisions();
        if (_state != State.Tween)
            return;
        _state = State.Move;
        StartMoveTween();
    }

    private void StartMoveTween()
    {
        var duration = this.Position.DistanceTo(_moveToPositions[currentPosIndex].Position) / _moveSpeed * 16;
        _moveTween.InterpolateProperty(
            _spikeHead,
            "position",
            _currentPos,
            _moveToPositions[currentPosIndex].Position,
            duration,
            Tween.TransitionType.Linear,
            Tween.EaseType.InOut,
            _idleDuration
        );
        _moveTween.Start();
        _timer.WaitTime = duration;
        _timer.Start();
        _currentPos = _moveToPositions[currentPosIndex].Position;
        if (currentPosIndex == _moveToPositions.Length - 1)
            currentPosIndex = 0;
        else
            currentPosIndex++;
    }

    private void UpdateMoveDirection()
    {
        var dir = this.Position.DirectionTo(_moveToPositions[currentPosIndex].Position);
    }

    private void UpdateRayCastCollisions()
    {
        if (_left.IsColliding())
            _collisionDirection = Direction.Left;
        if (_right.IsColliding())
            _collisionDirection = Direction.Right;
        if (_up.IsColliding())
            _collisionDirection = Direction.Up;
        if (_down.IsColliding())
            _collisionDirection = Direction.Down;
    }


    // Signals

    public void OnTimerTimeout()
    {
        switch (_state)
        {
            case State.Idle:
                _animatedSprite.Play("Idle");
                _state = State.Tween;
                _timer.Start();
                break;

            case State.Move:
                _state = State.Idle;
                _timer.WaitTime = _animationDuration;
                _timer.Start();
                break;

            case State.Smash:
                _animatedSprite.Play("Idle");
                break;
        }

    }

    public void OnArea2DAreaShapeEntered(RID areaRid, Area2D area, int bodyShapeIndex, int localShapeIndex)
    {
        if (area.Owner is PlayerTwo playerTwo)
        {
            playerTwo.GetHit(_spikeHeadDamage, Math.Sign(playerTwo.Position.x - Position.x));
        }
    }
}
