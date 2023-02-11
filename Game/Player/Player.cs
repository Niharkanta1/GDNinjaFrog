using Godot;
using System;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Player : Agent
{
    [Export] private float _moveSpeed = 200;
    [Export] private float _jumpImpulse = 600;
    [Export] private float _enemyBounceImpulse = 400;
    [Export] private float _knockBackSpeed = 50;
    [Export] private float _iFrameTime = 1.0f;
    [Export] private int _jumpDamage = 1;

    [Signal] public delegate void OnStateChange(string newState);

    private AnimationTree _animationTree;
    private AnimatedSprite _animatedSprite;
    private Area2D _stompHitBox;
    private Timer _timer;

    private GameSettings _gameSettings;
    private Vector2 _input, _velocity;
    private State _currentState;

    private bool _canJump = true;
    private bool _canDoubleJump;
    private Color _originalColor;

    private enum State
    {
        Idle,
        Run,
        Jump,
        Fall,
        Hit,
        DoubleJump,
        WallSlide
    }

    #region GODOT_CALLBACKS

    public override void _Ready()
    {
        _gameSettings = (GameSettings)GetNode("/root/GameSettings");
        _currentState = State.Idle;

        _animationTree = GetNode<AnimationTree>("AnimationTree");
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _stompHitBox = GetNode<Area2D>("StompHitBox");
        _timer = GetNode<Timer>("Timer");
        _originalColor = _animatedSprite.Modulate;
    }

    public override void _PhysicsProcess(float delta)
    {
        var input = GetPlayerInput();

        if (_currentState != State.Hit)
            UpdatePlayerMovement(input);
        else
            PlayerKnockBack(input);

        _velocity = MoveAndSlide(_velocity, Vector2.Up);
        SetAnimParameters();
        CheckNextState();
    }

    private void UpdatePlayerMovement(Vector2 input)
    {
        AdjustSpriteFlipDirection(input);

        _velocity.x = input.x * _moveSpeed;
        _velocity.y = Math.Min(_velocity.y + _gameSettings.Gravity, _gameSettings.TerminalVelocity);
    }

    private void PlayerKnockBack(Vector2 input)
    {
        CanBeHit = false;
        var knockBackDir = _animatedSprite.FlipH ? 1 : -1;
        _velocity.x = knockBackDir * _knockBackSpeed;
        _velocity.y = 0;
    }

    #endregion

    private void AdjustSpriteFlipDirection(Vector2 input)
    {
        switch (Math.Sign(input.x))
        {
            case 1:
                _animatedSprite.FlipH = false;
                break;
            case -1:
                _animatedSprite.FlipH = true;
                break;
        }
    }

    private void SetAnimParameters()
    {
        _animationTree.Set("parameters/MoveX/blend_position", Math.Sign(_velocity.x));
        _animationTree.Set("parameters/MoveY/blend_amount", Math.Sign(_velocity.y));
    }

    private void CheckNextState()
    {
        if (IsOnFloor()) // On the floor.
        {
            _canJump = true;
            _canDoubleJump = false;
            if (Input.IsActionJustPressed("jump") && _canJump)
            {
                EnterState(State.Jump);
                _canJump = false;
                _canDoubleJump = true;
            }
            else if (Math.Abs(_velocity.x) > 0)
            {
                EnterState(State.Run);
            }
            else
            {
                EnterState(State.Idle);
            }
        }
        else // On Air
        {
            if (Input.IsActionJustPressed("jump") && (_canJump || _canDoubleJump))
            {
                EnterState(State.DoubleJump);
            }
        }
    }

    private Vector2 GetPlayerInput()
    {
        _input.x = Input.GetActionStrength("right") - Input.GetActionRawStrength("left");
        _input.y = Input.GetActionStrength("down") - Input.GetActionStrength("up");
        return _input;
    }

    private void EnterState(State newState)
    {
        switch (newState)
        {
            case State.Jump:
                _canJump = false;
                _velocity.y = -_jumpImpulse; // Perform Jump
                break;

            case State.DoubleJump:
                _canJump = false;
                _canDoubleJump = false;
                _velocity.y = -_jumpImpulse; // Perform Jump
                _animationTree.Set("parameters/DoubleJump/active", true);
                break;

            case State.Hit:
                _animationTree.Set("parameters/Hit/active", true);
                break;
            case State.Idle:
                break;
            case State.Run:
                break;
            case State.Fall:
                break;
            case State.WallSlide:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        _currentState = newState;
        EmitSignal(nameof(OnStateChange), newState.ToString());
    }

    // Hit and Collisions
    #region HIT_COLLISIONS

    public override void GetHit(int damage, int direction = 1)
    {
        if (!CanBeHit) return;
        GD.Print(" Health: " + Health);
        Health -= damage;
        if (Health <= 0)
        {
            GD.Print("Player Died");
        }
        else
        {
            _timer.WaitTime = _iFrameTime;
            _timer.Start();
            _animatedSprite.Modulate = new Color(1, 1, 1, 0.3f);
            EnterState(State.Hit);
        }
    }

    public override void OnHitFinished()
    {
        GD.Print("Player Hit Anim Finished");
        EnterState(State.Idle);
    }

    #endregion
    // Signals
    #region  SIGNALS 

    public void OnStompHitBoxAreaShapeEntered(RID areaRid, Area2D area, int areaShapeIndex, int localShapeIndex)
    {
        if (!(area.Owner is Enemy)) return;
        var enemy = area.GetOwner<Enemy>();
        if (!enemy.CanBeHit)
            return;
        if (!(_stompHitBox.GlobalPosition.y < area.GlobalPosition.y)) return;
        _velocity.y = -_enemyBounceImpulse;
        enemy.GetHit(_jumpDamage);
    }

    public void OnTimerTimeout()
    {
        _timer.Stop();
        _animatedSprite.Modulate = _originalColor;
        CanBeHit = true;
    }

    #endregion
}
