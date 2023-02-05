using Godot;
using System;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Player : Agent
{
    [Export] private float moveSpeed = 200;
    [Export] private float jumpImpulse = 600;
    [Export] private float enemyBounceImpulse = 400;
    [Export] private float knockbackSpeed = 50;
    [Export] private float iFrameTime = 1.0f;
    [Export] private int jumpDamage = 1;

    [Signal] public delegate void OnStateChange(string newState);

    private AnimationTree animationTree;
    private AnimatedSprite animatedSprite;
    private Area2D jumpHitbox;
    private Timer timer;

    private GameSettings gameSettings;
    private Vector2 input, velocity;
    private State currentState;

    private bool canJump = true;
    private bool canDoubleJump = false;
    private Color originalColor;

    enum State
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
        gameSettings = (GameSettings)GetNode("/root/GameSettings");
        currentState = State.Idle;

        animationTree = GetNode<AnimationTree>("AnimationTree");
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        jumpHitbox = GetNode<Area2D>("JumpHitbox");
        timer = GetNode<Timer>("Timer");
        originalColor = animatedSprite.Modulate;
    }

    public override void _PhysicsProcess(float delta)
    {
        var input = GetPlayerInput();

        if (currentState != State.Hit)
            UpdatePlayerMovement(input);
        else
            PlayerKnockback(input);

        velocity = MoveAndSlide(velocity, Vector2.Up);
        SetAnimParameters();
        CheckNextState();
    }

    private void UpdatePlayerMovement(Vector2 input)
    {
        AdjustSpriteFlipDirection(input);

        velocity.x = input.x * moveSpeed;
        velocity.y = Math.Min(velocity.y + gameSettings.gravity, gameSettings.terminalVelocity);
    }

    private void PlayerKnockback(Vector2 input)
    {
        CanBeHit = false;
        var knockbackDir = animatedSprite.FlipH ? 1 : -1;
        velocity.x = knockbackDir * knockbackSpeed;
        velocity.y = 0;
    }

    #endregion

    private void AdjustSpriteFlipDirection(Vector2 input)
    {
        if (Math.Sign(input.x) == 1)
            animatedSprite.FlipH = false;
        else if (Math.Sign(input.x) == -1)
            animatedSprite.FlipH = true;
    }

    private void SetAnimParameters()
    {
        animationTree.Set("parameters/MoveX/blend_position", Math.Sign(velocity.x));
        animationTree.Set("parameters/MoveY/blend_amount", Math.Sign(velocity.y));
    }

    private void CheckNextState()
    {
        if (IsOnFloor()) // On the floor.
        {
            canJump = true;
            canDoubleJump = false;
            if (Input.IsActionJustPressed("jump") && canJump)
            {
                EnterState(State.Jump);
                canJump = false;
                canDoubleJump = true;
            }
            else if (Math.Abs(velocity.x) > 0)
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
            if (Input.IsActionJustPressed("jump") && (canJump || canDoubleJump))
            {
                EnterState(State.DoubleJump);
            }
        }
    }

    public Vector2 GetPlayerInput()
    {
        input.x = Input.GetActionStrength("right") - Input.GetActionRawStrength("left");
        input.y = Input.GetActionStrength("down") - Input.GetActionStrength("up");
        return input;
    }

    private void EnterState(State newState)
    {
        switch (newState)
        {
            case State.Jump:
                canJump = false;
                velocity.y = -jumpImpulse; // Perform Jump
                break;

            case State.DoubleJump:
                canJump = false;
                canDoubleJump = false;
                velocity.y = -jumpImpulse; // Perform Jump
                animationTree.Set("parameters/DoubleJump/active", true);
                break;

            case State.Hit:
                animationTree.Set("parameters/Hit/active", true);
                break;
        }
        currentState = newState;
        EmitSignal(nameof(OnStateChange), newState.ToString());
    }

    // Hit and Collisions
    #region HIT_COLLISIONS

    public override void GetHit(int damage)
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
            timer.WaitTime = iFrameTime;
            timer.Start();
            animatedSprite.Modulate = new Color(1, 1, 1, 0.3f);
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

    public void OnJumpHitboxAreaShapeEntered(RID areaRID, Area2D area, int areaShapeIndex, int localShapeIndex)
    {
        if (area.Owner is Enemy)
        {
            Enemy enemy = area.GetOwner<Enemy>();
            if (!enemy.CanBeHit)
                return;
            if (jumpHitbox.GlobalPosition.y < area.GlobalPosition.y)
            {
                velocity.y = -enemyBounceImpulse;
                enemy.GetHit(jumpDamage);
            }
        }
    }

    public void OnTimerTimeout()
    {
        timer.Stop();
        animatedSprite.Modulate = originalColor;
        CanBeHit = true;
    }

    #endregion
}
