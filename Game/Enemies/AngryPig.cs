using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class AngryPig : Enemy
{
    [Export] private NodePath[] _waypoints = new NodePath[2];
    [Export] private float _walkSpeed = 100;
    [Export] private float _runSpeed = 160;

    private AnimatedSprite _animatedSprite;
    private AnimationTree _animationTree;
    private Timer _timer;

    private GameSettings _gameSettings;
    private int _waypointIndex;
    private Vector2 _wayPointPos;
    private Vector2 _velocity;

    private const float WaypointMinDistance = 5.0f;
    private State _currentState;

    private enum State { Idle, Run, Walk, Hit }

    public override void _Ready()
    {
        _gameSettings = (GameSettings)GetNode("/root/GameSettings");
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _animationTree = GetNode<AnimationTree>("AnimationTree");
        _timer = GetNode<Timer>("Timer");

        _wayPointPos = GetWaypointPosition(_waypointIndex);
        _velocity = Vector2.Zero;
        _currentState = State.Walk;
    }

    public override void _PhysicsProcess(float delta)
    {
        var direction = Position.DirectionTo(_wayPointPos);
        var distance = new Vector2(Position.x, 0).DistanceTo(new Vector2(_wayPointPos.x, 0));
        float moveSpeed = 0;
        switch (_currentState)
        {
            case State.Walk:
                moveSpeed = _walkSpeed;
                break;
            case State.Run:
                moveSpeed = _runSpeed;
                break;
            case State.Idle:
                break;
            case State.Hit:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (distance >= WaypointMinDistance)
        {
            _velocity.x = Math.Sign(direction.x) * moveSpeed;
            _velocity.y = Math.Min(_velocity.y + _gameSettings.Gravity, _gameSettings.TerminalVelocity);

            switch (Math.Sign(direction.x))
            {
                case 1:
                    _animatedSprite.FlipH = true;
                    break;
                case -1:
                    _animatedSprite.FlipH = false;
                    break;
            }

            _velocity = MoveAndSlide(_velocity, Vector2.Up);
        }
        else
        {
            if (_waypointIndex < _waypoints.Length - 1)
                _waypointIndex++;
            else
                _waypointIndex = 0;
        }
        _wayPointPos = GetWaypointPosition(_waypointIndex);
    }

    private Vector2 GetWaypointPosition(int index)
    {
        return GetNode<Position2D>(_waypoints[index]).Position;
    }

    public override void GetHit(int damage, int direction = 1)
    {
        Health -= damage;
        if (Health <= 0)
        {
            // Play Enemy Death Animation
            QueueFree();
        }
        CanBeHit = false;
        _currentState = State.Hit;
        _animationTree.Set("parameters/Hit/active", true);
        _animationTree.Set("parameters/HitVariation/blend_amount", _gameSettings.RandomGen.RandiRange(0, 1));
        _timer.WaitTime = 0.5f;
        _timer.Start();
    }

    // Signals

    public void OnDetectionZoneBodyShapeEntered(RID bodyRid, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        _animationTree.Active = false;
        _animationTree.Active = true;
        _animationTree.Set("parameters/PlayerDetected/blend_position", 1);
        if (_currentState == State.Walk)
            _currentState = State.Run;
    }

    public void OnDetectionZoneBodyShapeExited(RID bodyRid, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        _animationTree.Active = false;
        _animationTree.Active = true;
        _animationTree.Set("parameters/PlayerDetected/blend_position", 0);
        if (_currentState == State.Run)
            _currentState = State.Walk;
    }

    public void OnTimerTimeout()
    {
        CanBeHit = true;
        _currentState = State.Run;
        _timer.Stop();
    }

}
