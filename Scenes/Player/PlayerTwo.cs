using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class PlayerTwo : KinematicBody2D
{
    private AnimationPlayer _animationPlayer;
    private AnimatedSprite _animatedSprite;
    private Node2D _body;
    private Timer _coyoteTimer, _jumpBufferTimer;
    private RayCast2D _leftWallChecker1, _rightWallChecker1, _leftWallChecker2, _rightWallChecker2,
                    _floorChecker1, _floorChecker2;

    [Export] private int _gravity = 1000;
    [Export] private int _jumpSpeed = -350;
    [Export] private int _doubleJumpSpeed = -300;
    [Export] private int _walkSpeed = 150;
    [Export] private int _dashSpeed = 300;
    [Export] private int _numDash = 1;

    public bool IsAttacking = false;
    public bool IsDashing = false;
    
    private Vector2 _velocity;
    private int _lookDirection = 1;
    private bool _canJump = true;
    private bool _canDoubleJump;

    private States _state;

    private States CurrentState
    {
        get => _state;
        set
        {
            InitState(value);
            _state = value;
            EmitSignal(nameof(OnStateChange), value.ToString());
        }
    }

    private enum States { Idle, Run, Fall, Jump, DoubleJump, Dash, WallSlide, WallJump, Die };

    [Signal] public delegate void OnStateChange(string newState);

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite>("Body/AnimatedSprite");
        _animationPlayer = GetNode<AnimationPlayer>("Body/AnimationPlayer");
        _body = GetNode<Node2D>("Body");
        _coyoteTimer = GetNode<Timer>("Timers/CoyoteeTimer");
        _jumpBufferTimer = GetNode<Timer>("Timers/JumpBufferTimer");

        _leftWallChecker1 = GetNode<RayCast2D>("WallChecker/LeftWallChecker1");
        _rightWallChecker1 = GetNode<RayCast2D>("WallChecker/RightWallChecker1");
        _leftWallChecker2 = GetNode<RayCast2D>("WallChecker/LeftWallChecker2");
        _rightWallChecker2 = GetNode<RayCast2D>("WallChecker/RightWallChecker2");
        _floorChecker1 = GetNode<RayCast2D>("WallChecker/FloorChecker1");
        _floorChecker2 = GetNode<RayCast2D>("WallChecker/FloorChecker2");

        CurrentState = States.Idle;
        _velocity = Vector2.Zero;
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (CurrentState)
        {
            // Idle State
            case States.Idle:
                if (!IsPlayerOnFloor())
                {
                    if (_velocity.y > 0)
                    {
                        CurrentState = States.Fall;
                        return;
                    }
                }
                ApplyGravity(delta);
                Move();

                // Handle Transitions:
                if (Input.IsActionPressed("left") || Input.IsActionPressed("right"))
                {
                    CurrentState = States.Run;
                }
                if (Input.IsActionJustPressed("jump") || !_jumpBufferTimer.IsStopped()) // Adding a jump buffer
                {
                    _jumpBufferTimer.Stop();
                    CurrentState = States.Jump;
                }
                break;

            // Walk State
            case States.Run:
                if (!IsPlayerOnFloor())
                {
                    if (_velocity.y > 0)
                    {
                        CurrentState = States.Fall;
                        return;
                    }
                }
                float inputDirectionX = HorizontalMovement(delta);

                // Handle Transitions:
                if (IsEqualApprox(inputDirectionX, 0.0f))
                {
                    CurrentState = States.Idle;
                }
                else if (Input.IsActionJustPressed("jump"))
                {
                    CurrentState = States.Jump;
                }

                break;

            // Fall State
            case States.Fall:

                if (IsPlayerOnFloor())
                {
                    CurrentState = States.Idle;
                    return;
                }
                inputDirectionX = HorizontalMovement(delta);

                // Handle Transitions:
                if (Input.IsActionJustPressed("jump") && (_canJump || _canDoubleJump))
                {
                    if (!_coyoteTimer.IsStopped())
                    {
                        CurrentState = States.Jump;
                    }
                    else
                    {
                        CurrentState = States.DoubleJump;
                    }
                    _coyoteTimer.Stop();
                }
                else if (Input.IsActionJustPressed("jump") && !_canJump) // Jump Pressed early. need to use a buffer. 
                {
                    if (!_jumpBufferTimer.IsStopped())
                        _jumpBufferTimer.Stop();
                    _jumpBufferTimer.Start();
                }
                if (IsOnWall() && !IsPlayerOnFloor() &&
                    ((inputDirectionX > 0 && IsNextToRightWall()) || (inputDirectionX < 0 && IsNextToLeftWall())))
                {
                    CurrentState = States.WallSlide;
                }
                break;


            // Jump State
            case States.Jump:
                if (_velocity.y > 0)
                {
                    CurrentState = States.Fall;
                    return;
                }

                inputDirectionX = HorizontalMovement(delta);

                // Handle Transitions:
                if (Input.IsActionJustPressed("jump") && _canDoubleJump)
                {
                    CurrentState = States.DoubleJump;
                }
                if (IsOnWall() && !IsPlayerOnFloor())
                {
                    CurrentState = States.WallSlide;
                }
                break;

            case States.Dash:
                break;

            case States.DoubleJump:
                if (_velocity.y > 0)
                {
                    CurrentState = States.Fall;
                    return;
                }
                HorizontalMovement(delta);

                // Handle Transitions:
                if (IsNextToWall() && !IsPlayerOnFloor())
                {
                    CurrentState = States.WallSlide;
                }
                break;

            case States.WallSlide:
                if (IsPlayerOnFloor())
                {
                    CurrentState = States.Idle;
                    return;
                }
                inputDirectionX = Input.GetActionStrength("right") - Input.GetActionStrength("left");
                if ((_lookDirection == 1 && inputDirectionX < 0) ||
                        (_lookDirection == -1 && inputDirectionX > 0) || !IsNextToWall())
                {
                    CurrentState = States.Fall;
                }
                _velocity.y += _gravity * 0.2f * delta;
                Move();
                break;

            case States.Die:
                break;

            case States.WallJump:
                break;
        }
    }

    private float HorizontalMovement(float delta)
    {
        var inputDirectionX = Input.GetActionStrength("right") - Input.GetActionStrength("left");

        if (Input.IsActionPressed("right") && Input.IsActionPressed("left"))
        {
            inputDirectionX = _lookDirection;
        }

        UpdateDirection(inputDirectionX);
        _velocity.x = _walkSpeed * inputDirectionX;
        ApplyGravity(delta);
        Move();
        return inputDirectionX;
    }

    private void Move() => _velocity = MoveAndSlide(_velocity, Vector2.Up);

    private bool IsNextToWall() => IsNextToLeftWall() || IsNextToRightWall();

    private bool IsNextToRightWall() => _rightWallChecker1.IsColliding() && _rightWallChecker2.IsColliding();

    private bool IsNextToLeftWall() => _leftWallChecker1.IsColliding() && _leftWallChecker2.IsColliding();

    private bool IsPlayerOnFloor() => _floorChecker1.IsColliding() || _floorChecker2.IsColliding();

    // Initialization for the State occurs when entering a state.
    // This is meant for 
    // setting up the flags, animation and Sounds
    private void InitState(States nextState)
    {
        switch (nextState)
        {
            case States.Idle:
                _canJump = true;
                _velocity.x = 0;
                _animationPlayer.Play("Idle");
                break;

            case States.Run:
                _animationPlayer.Play("Run");
                break;

            case States.Jump:
                _canJump = false;
                _canDoubleJump = true;
                _velocity.y = _jumpSpeed;
                _animationPlayer.Play("Jump");
                break;

            case States.Fall:
                if (_canJump)
                    _coyoteTimer.Start();
                _animationPlayer.Play("Fall");
                break;

            case States.DoubleJump:
                _canDoubleJump = false;
                _canJump = false;
                _velocity.y = _doubleJumpSpeed;
                _animationPlayer.Play("DoubleJump");
                break;

            case States.WallSlide:
                _canDoubleJump = true;
                _velocity = Vector2.Zero;
                _animationPlayer.Play("WallSlide");
                break;

            case States.Dash:
                break;

            case States.Die:
                break;

            case States.WallJump:
                break;
        }
    }

    private void UpdateDirection(float inputDirectionX)
    {
        if (inputDirectionX > 0)
            SetSpriteDirection(1, false);
        else if (inputDirectionX < 0)
            SetSpriteDirection(-1, true);
    }

    private void SetSpriteDirection(int lookDirection, bool flipH)
    {
        this._lookDirection = lookDirection;
        _animatedSprite.FlipH = flipH;
    }

    private void ApplyGravity(float delta)
    {
        _velocity.y += _gravity * delta;
    }

    private void OnDashFinished() => IsDashing = false;
    private void ResetDashCounter(int value) => _numDash = value;
    private bool HasDashes() => _numDash > 0;

    // Utils
    private float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    private static bool IsEqualApprox(float a, float b)
    {
        return IsEqualApprox(a, b, 0.00001);
    }

    private static bool IsEqualApprox(double left, double right, double delta)
    {
        return Math.Abs(left - right) < delta;
    }
}
