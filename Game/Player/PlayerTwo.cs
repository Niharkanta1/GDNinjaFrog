using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class PlayerTwo : Agent
{
    private AnimationPlayer _animationPlayer;
    private AnimatedSprite _animatedSprite;
    private Timer _coyoteTimer, _jumpBufferTimer, _wallJumpTimer, _hitTimer, _knockBackTimer, _fallThroughTimer;
    private RayCast2D _leftWallChecker1, _rightWallChecker1, _leftWallChecker2, _rightWallChecker2,
        _floorChecker1, _floorChecker2;
    private Particles2D _deathParticle, _dashParticleL, _dashParticleR;
    private Area2D _stompHitBox;

    [Export] private float _gravity = 1200;
    [Export] private float _fallGravityMultiplier = 1.5f;
    [Export] private float _jumpSpeed = -350;
    [Export] private float _wallJumpSpeed = -350;
    [Export] private float _wallJumpMoveSpeed = 150;
    [Export] private float _doubleJumpSpeed = -300;
    [Export] private float _walkSpeed = 200;
    [Export] private float _dashSpeed = 300;
    [Export] private int _numDash = 1;
    [Export] private float _stompBounceSpeed = -300;
    [Export] private int _stompDamage = 1;
    [Export] private float _iFrameTime = 1;
    [Export] private float _knockBackSpeed = 150f;

    private Color _originalColor;
    private Vector2 _velocity;
    private int _lookDirection = 1;
    private int _wallJumpDirection = 1;
    private int _knockBackDirection = 1;
    private bool _canJump = true;
    private bool _canDoubleJump;
    private bool _canDash = true;
    private bool _isDashing;
    private bool _isKnockedBack;
    private bool _isFallingThrough;
    private bool _hitAnimationFinished = true;

    private readonly Vector2 _snapVector = Vector2.Down * 16;
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

    private enum States { Idle, Run, Fall, Jump, DoubleJump, Dash, WallSlide, WallJump, Hit, Die };

    [Signal] public delegate void OnStateChange(string newState);

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite>("Body/AnimatedSprite");
        _animationPlayer = GetNode<AnimationPlayer>("Body/AnimationPlayer");
        _coyoteTimer = GetNode<Timer>("Timers/CoyoteTimer");
        _jumpBufferTimer = GetNode<Timer>("Timers/JumpBufferTimer");
        _wallJumpTimer = GetNode<Timer>("Timers/WallJumpTimer");
        _hitTimer = GetNode<Timer>("Timers/HitTimer");
        _knockBackTimer = GetNode<Timer>("Timers/KnockBackTimer");
        _fallThroughTimer = GetNode<Timer>("Timers/FallThroughTimer");

        _deathParticle = GetNode<Particles2D>("Particles/DeathParticle");
        _dashParticleR = GetNode<Particles2D>("Particles/DashParticleR");
        _dashParticleL = GetNode<Particles2D>("Particles/DashParticleL");

        _stompHitBox = GetNode<Area2D>("StompHitBox");

        _leftWallChecker1 = GetNode<RayCast2D>("WallChecker/LeftWallChecker1");
        _rightWallChecker1 = GetNode<RayCast2D>("WallChecker/RightWallChecker1");
        _leftWallChecker2 = GetNode<RayCast2D>("WallChecker/LeftWallChecker2");
        _rightWallChecker2 = GetNode<RayCast2D>("WallChecker/RightWallChecker2");
        _floorChecker1 = GetNode<RayCast2D>("WallChecker/FloorChecker1");
        _floorChecker2 = GetNode<RayCast2D>("WallChecker/FloorChecker2");

        CurrentState = States.Idle;
        _velocity = Vector2.Zero;
        _originalColor = _animatedSprite.Modulate;
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
                Move(_snapVector);

                // Handle Transitions:
                if (Input.IsActionPressed("left") || Input.IsActionPressed("right"))
                {
                    CurrentState = States.Run;
                }
                else if (Input.IsActionPressed("jump") && Input.IsActionPressed("down"))
                {
                    if (!_isFallingThrough)
                    {
                        _isFallingThrough = true;
                        SetCollisionMaskBit(2, false);
                        _fallThroughTimer.Start();
                    }
                }
                else if (Input.IsActionJustPressed("jump") || !_jumpBufferTimer.IsStopped()) // Adding a jump buffer
                {
                    _jumpBufferTimer.Stop();
                    CurrentState = States.Jump;
                }
                else if (Input.IsActionJustPressed("dash") && _canDash)
                {
                    CurrentState = States.Dash;
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
                var inputDirectionX = HorizontalMovementWithSnap(delta, _walkSpeed, _snapVector);

                // Handle Transitions:
                if (Utils.IsEqualApprox(inputDirectionX, 0.0f))
                {
                    CurrentState = States.Idle;
                }
                else if (Input.IsActionJustPressed("jump"))
                {
                    CurrentState = States.Jump;
                }
                else if (Input.IsActionJustPressed("dash") && _canDash)
                {
                    CurrentState = States.Dash;
                }

                break;

            // Fall State
            case States.Fall:

                if (IsPlayerOnFloor())
                {
                    CurrentState = States.Idle;
                    return;
                }
                inputDirectionX = HorizontalMovementWithSnap(delta, _walkSpeed, _snapVector, _fallGravityMultiplier);

                // Handle Transitions:
                if (Input.IsActionJustPressed("jump") && (_canJump || _canDoubleJump))
                {
                    CurrentState = !_coyoteTimer.IsStopped() ? States.Jump : States.DoubleJump;
                    _coyoteTimer.Stop();
                }
                else if (!_jumpBufferTimer.IsStopped() && (_canDoubleJump || _canJump))
                {
                    _jumpBufferTimer.Stop();
                    CurrentState = States.DoubleJump;
                }
                else if (Input.IsActionJustPressed("jump") && !_canJump) // Jump Pressed early. need to use a buffer. 
                {
                    if (!_jumpBufferTimer.IsStopped())
                        _jumpBufferTimer.Stop();
                    _jumpBufferTimer.Start();
                }
                else if (IsNextToWall() && !IsPlayerOnFloor() &&
                         ((inputDirectionX > 0 && IsNextToRightWall()) || (inputDirectionX < 0 && IsNextToLeftWall())))
                {
                    CurrentState = States.WallSlide;
                }
                else if (Input.IsActionJustPressed("dash") && _canDash)
                {
                    CurrentState = States.Dash;
                }
                break;


            // Jump State
            case States.Jump:
                if (_velocity.y > 0)
                {
                    CurrentState = States.Fall;
                    return;
                }

                inputDirectionX = HorizontalMovement(delta, _walkSpeed);

                // Handle Transitions:
                if (Input.IsActionJustPressed("jump") && _canDoubleJump)
                {
                    CurrentState = States.DoubleJump;
                }
                else if (IsNextToWall() && !IsPlayerOnFloor() &&
                         ((inputDirectionX > 0 && IsNextToRightWall()) || (inputDirectionX < 0 && IsNextToLeftWall())))
                {
                    CurrentState = States.WallSlide;
                }
                else if (Input.IsActionJustPressed("dash") && _canDash)
                {
                    CurrentState = States.Dash;
                }
                break;

            case States.Dash:
                _dashParticleL.Position = Position;
                _dashParticleR.Position = Position;
                _velocity.y = 0;
                _velocity.x = _dashSpeed * _lookDirection;
                if (!_isDashing)
                {
                    CurrentState = IsPlayerOnFloor() ? States.Idle : States.Fall;
                }
                Move(_snapVector);

                // Handle Transitions:
                if (IsNextToWall() && !IsPlayerOnFloor() &&
                    ((_lookDirection > 0 && IsNextToRightWall()) || (_lookDirection < 0 && IsNextToLeftWall())))
                {
                    ResetDashState();
                    CurrentState = States.WallSlide;
                }
                else if (Input.IsActionJustPressed("jump") && (_canJump || _canDoubleJump)) // Jump Pressed early. need to use a buffer. 
                {
                    if (!_jumpBufferTimer.IsStopped())
                        _jumpBufferTimer.Stop();
                    _jumpBufferTimer.Start();
                }
                break;

            case States.DoubleJump:
                if (_velocity.y > 0)
                {
                    CurrentState = States.Fall;
                    return;
                }
                inputDirectionX = HorizontalMovement(delta, _walkSpeed);

                // Handle Transitions:
                if (IsNextToWall() && !IsPlayerOnFloor() &&
                    ((inputDirectionX > 0 && IsNextToRightWall()) || (inputDirectionX < 0 && IsNextToLeftWall())))
                {
                    CurrentState = States.WallSlide;
                }
                else if (Input.IsActionJustPressed("dash") && _canDash)
                {
                    CurrentState = States.Dash;
                }
                break;

            case States.WallSlide:
                if (IsPlayerOnFloor())
                {
                    CurrentState = States.Idle;
                    return;
                }
                inputDirectionX = Input.GetActionStrength("right") - Input.GetActionStrength("left");

                _velocity.y += _gravity * 0.2f * delta;
                _velocity.x = 0;
                Move();

                // Handle Transitions
                if ((_lookDirection == 1 && inputDirectionX < 0) ||
                    (_lookDirection == -1 && inputDirectionX > 0) ||
                    !IsNextToWall() || Input.IsActionPressed("down"))
                {
                    CurrentState = States.Fall;
                }
                if (Input.IsActionJustPressed("jump"))
                {
                    switch (_animatedSprite.FlipH)
                    {
                        case true when IsNextToLeftWall():
                            _wallJumpDirection = 1;
                            break;
                        case false when IsNextToRightWall():
                            _wallJumpDirection = -1;
                            break;
                    }
                    CurrentState = States.WallJump;
                    _wallJumpTimer.Start();
                }
                break;

            case States.Die:
                break;

            case States.WallJump:
                if (_velocity.y > 0)
                {
                    CurrentState = States.Fall;
                    return;
                }
                _velocity.x = _wallJumpDirection * _wallJumpMoveSpeed;

                inputDirectionX = FindInputDirection();
                ApplyGravity(delta);
                Move();

                // Handle Transitions:
                if (Input.IsActionJustPressed("jump") && _canDoubleJump)
                {
                    CurrentState = States.DoubleJump;
                }
                else if (IsNextToWall() && !IsPlayerOnFloor() && _wallJumpTimer.IsStopped() &&
                         ((inputDirectionX > 0 && IsNextToRightWall()) || (inputDirectionX < 0 && IsNextToLeftWall())))
                {
                    CurrentState = States.WallSlide;
                }
                else if (Input.IsActionJustPressed("dash") && _canDash)
                {
                    CurrentState = States.Dash;
                }
                break;

            case States.Hit:
                if (!_isKnockedBack && _hitAnimationFinished)
                    CurrentState = States.Idle;
                if (_knockBackTimer.IsStopped())
                {
                    _velocity = Vector2.Zero;
                    _isKnockedBack = false;
                }

                else
                    _velocity.x = _knockBackDirection * _knockBackSpeed;
                Move();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private float HorizontalMovement(float delta, float moveSpeed, float gravityMultiplier = 1)
    {
        var inputDirectionX = FindInputDirection();
        UpdateDirection(inputDirectionX);
        _velocity.x = moveSpeed * inputDirectionX;
        ApplyGravity(delta, gravityMultiplier);
        Move();
        return inputDirectionX;
    }
    private float HorizontalMovementWithSnap(float delta, float moveSpeed, Vector2 snap, float gravityMultiplier = 1)
    {
        var inputDirectionX = FindInputDirection();
        UpdateDirection(inputDirectionX);
        _velocity.x = moveSpeed * inputDirectionX;
        ApplyGravity(delta, gravityMultiplier);
        Move(snap);
        return inputDirectionX;
    }

    private float FindInputDirection()
    {
        var inputDirectionX = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        if (Input.IsActionPressed("right") && Input.IsActionPressed("left"))
        {
            inputDirectionX = _lookDirection;
        }
        return inputDirectionX;
    }

    private void Move() => _velocity = MoveAndSlide(_velocity, Vector2.Up);
    private void Move(Vector2 snap) => _velocity = MoveAndSlideWithSnap(_velocity, snap, Vector2.Up, infiniteInertia: false);
    private void ApplyGravity(float delta, float gravityMultiplier = 1) =>
        _velocity.y += _gravity * gravityMultiplier * delta;

    private bool IsNextToWall() => IsNextToLeftWall() || IsNextToRightWall();
    private bool IsNextToRightWall() => _rightWallChecker1.IsColliding() && _rightWallChecker2.IsColliding();
    private bool IsNextToLeftWall() => _leftWallChecker1.IsColliding() && _leftWallChecker2.IsColliding();
    private bool IsPlayerOnFloor() => !_isFallingThrough && (_floorChecker1.IsColliding() || _floorChecker2.IsColliding());

    // Initialization for the State occurs when entering a state.
    // This is meant for 
    // setting up the flags, animation and Sounds
    private void InitState(States nextState)
    {
        switch (nextState)
        {
            case States.Idle:
                _canDash = true;
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

            case States.WallJump:
                _canJump = false;
                _canDoubleJump = true;
                _velocity.y = _wallJumpSpeed;
                _velocity.x = 0;
                _lookDirection = _wallJumpDirection;
                _animatedSprite.FlipH = (_wallJumpDirection != 1);
                _animationPlayer.Play("Jump");
                break;

            case States.WallSlide:
                _canDoubleJump = true;
                _velocity = Vector2.Zero;
                _animationPlayer.Play("WallSlide");
                break;

            case States.Dash:
                StartDashParticle();
                _animationPlayer.Play("Dash");
                _canDash = false;
                _isDashing = true;
                CanBeHit = false;
                break;

            case States.Die:
                _animationPlayer.Play("Death");
                _velocity.y = _jumpSpeed * 0.5f;
                Move();
                break;

            case States.Hit:
                _animationPlayer.Play("Hit");
                _hitAnimationFinished = false;
                _isKnockedBack = true;
                _knockBackTimer.Start();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(nextState), nextState, null);
        }
    }
    private void StartDashParticle()
    {
        switch (_lookDirection)
        {
            case 1:
                _dashParticleR.Position = Position;
                _dashParticleR.Emitting = true;
                break;
            case -1:
                _dashParticleL.Position = Position;
                _dashParticleL.Emitting = true;
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
        _lookDirection = lookDirection;
        _animatedSprite.FlipH = flipH;
    }

    // Inherited Method Overrides
    public override void GetHit(int damage, int direction = 1)
    {
        if (!CanBeHit) return;
        _knockBackDirection = direction;
        Health -= damage;
        if (Health <= 0)
        {
            CurrentState = States.Die;
        }
        else
        {
            _hitTimer.WaitTime = _iFrameTime;
            CanBeHit = false;
            _hitTimer.Start();
            _animatedSprite.Modulate = new Color(1, 1, 1, 0.3f);
            CurrentState = States.Hit;
        }
    }

    public void BouncePlayer(float bounceSpeed)
    {
        CurrentState = States.Jump;
        _velocity.y = bounceSpeed;
    }

    // Signals

    public void OnJumpHitBoxAreaShapeEntered(RID areaRid, Area2D area, int areaShapeIndex, int localShapeIndex)
    {
        if (!(area.Owner is Enemy)) return;
        var enemy = area.GetOwner<Enemy>();
        if (!enemy.CanBeHit)
            return;
        if (!(_stompHitBox.GlobalPosition.y < area.GlobalPosition.y)) return;
        BouncePlayer(_stompBounceSpeed);
        enemy.GetHit(_stompDamage);
    }

    public void OnTimerTimeout()
    {
        _animatedSprite.Modulate = _originalColor;
        CanBeHit = true;
    }

    public void OnFallThroughTimerTimeout()
    {
        _isFallingThrough = false;
        SetCollisionMaskBit(2, true);
    }

    public void OnHitAnimationFinished() => _hitAnimationFinished = true;
    public void OnDashAnimationFinished() => ResetDashState();
    private void ResetDashState()
    {
        _isDashing = false;
        CanBeHit = true;
        if (_dashParticleR.Emitting)
            _dashParticleR.Emitting = false;
        if (_dashParticleL.Emitting)
            _dashParticleL.Emitting = false;
    }

    public void OnPlayerDeath()
    {
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        GetNode<Node2D>("Body").Visible = false;
        GetNode<Area2D>("HitBoxArea").Monitorable = false;
        GetNode<Label>("P2Label").Visible = false;
        _stompHitBox.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        _deathParticle.Position = Position;
        _deathParticle.Scale = new Vector2(_lookDirection, 1);
        _deathParticle.Emitting = true;
    }
}