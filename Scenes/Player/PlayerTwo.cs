using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class PlayerTwo : KinematicBody2D
{
    private AnimationPlayer animationPlayer;
    private AnimatedSprite animatedSprite;
    private Node2D body;
    private Timer coyoteTimer, jumpBufferTimer;
    private RayCast2D leftWallChecker1, rightWallChecker1, leftWallChecker2, rightWallChecker2,
                    floorChecker1, floorChecker2;

    [Export] private int gravity = 1000;
    [Export] private int jumpSpeed = -350;
    [Export] private int doubleJumpSpeed = -300;
    [Export] private int walkSpeed = 150;
    [Export] private int dashSpeed = 300;
    [Export] private int numDash = 1;

    private Vector2 velocity;
    private int lookDirection = 1;
    private bool isAttacking = false;
    private bool isDashing = false;
    private bool canJump = true;
    private bool canDoubleJump = false;

    private States state;
    public States CurrentState
    {
        get { return state; }
        set
        {
            InitState(value);
            state = value;
            EmitSignal(nameof(OnStateChange), value.ToString());
        }
    }
    public enum States { IDLE, RUN, FALL, JUMP, DOUBLEJUMP, DASH, WALLSLIDE, DEATH };

    [Signal] public delegate void OnStateChange(string newState);

    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite>("Body/AnimatedSprite");
        animationPlayer = GetNode<AnimationPlayer>("Body/AnimationPlayer");
        body = GetNode<Node2D>("Body");
        coyoteTimer = GetNode<Timer>("Timers/CoyoteeTimer");
        jumpBufferTimer = GetNode<Timer>("Timers/JumpBufferTimer");

        leftWallChecker1 = GetNode<RayCast2D>("WallChecker/LeftWallChecker1");
        rightWallChecker1 = GetNode<RayCast2D>("WallChecker/RightWallChecker1");
        leftWallChecker2 = GetNode<RayCast2D>("WallChecker/LeftWallChecker2");
        rightWallChecker2 = GetNode<RayCast2D>("WallChecker/RightWallChecker2");
        floorChecker1 = GetNode<RayCast2D>("WallChecker/FloorChecker1");
        floorChecker2 = GetNode<RayCast2D>("WallChecker/FloorChecker2");

        CurrentState = States.IDLE;
        velocity = Vector2.Zero;
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (CurrentState)
        {
            // Idle State
            case States.IDLE:
                if (!IsPlayerOnFloor())
                {
                    if (velocity.y > 0)
                    {
                        CurrentState = States.FALL;
                        return;
                    }
                }
                ApplyGravity(delta);
                Move();

                // Handle Transitions:
                if (Input.IsActionPressed("left") || Input.IsActionPressed("right"))
                {
                    CurrentState = States.RUN;
                }
                if (Input.IsActionJustPressed("jump") || !jumpBufferTimer.IsStopped()) // Adding a jump buffer
                {
                    jumpBufferTimer.Stop();
                    CurrentState = States.JUMP;
                }
                break;

            // Walk State
            case States.RUN:
                if (!IsPlayerOnFloor())
                {
                    if (velocity.y > 0)
                    {
                        CurrentState = States.FALL;
                        return;
                    }
                }
                float inputDirectionX = HorrizontalMovement(delta);

                // Handle Transitions:
                if (IsEqualApprox(inputDirectionX, 0.0f))
                {
                    CurrentState = States.IDLE;
                }
                else if (Input.IsActionJustPressed("jump"))
                {
                    CurrentState = States.JUMP;
                }

                break;

            // Fall State
            case States.FALL:

                if (IsPlayerOnFloor())
                {
                    CurrentState = States.IDLE;
                    return;
                }
                inputDirectionX = HorrizontalMovement(delta);

                // Handle Transitions:
                if (Input.IsActionJustPressed("jump") && (canJump || canDoubleJump))
                {
                    if (!coyoteTimer.IsStopped())
                    {
                        CurrentState = States.JUMP;
                    }
                    else
                    {
                        CurrentState = States.DOUBLEJUMP;
                    }
                    coyoteTimer.Stop();
                }
                else if (Input.IsActionJustPressed("jump") && !canJump) // Jump Pressed early. need to use a buffer. 
                {
                    if (!jumpBufferTimer.IsStopped())
                        jumpBufferTimer.Stop();
                    jumpBufferTimer.Start();
                }
                if (IsOnWall() && !IsPlayerOnFloor() &&
                    ((inputDirectionX > 0 && IsNextToRightWall()) || (inputDirectionX < 0 && IsNextToLeftWall())))
                {
                    CurrentState = States.WALLSLIDE;
                }
                break;


            // Jump State
            case States.JUMP:
                if (velocity.y > 0)
                {
                    CurrentState = States.FALL;
                    return;
                }

                inputDirectionX = HorrizontalMovement(delta);

                // Handle Transitions:
                if (Input.IsActionJustPressed("jump") && canDoubleJump)
                {
                    CurrentState = States.DOUBLEJUMP;
                }
                if (IsOnWall() && !IsPlayerOnFloor())
                {
                    CurrentState = States.WALLSLIDE;
                }
                break;

            case States.DASH:
                break;

            case States.DOUBLEJUMP:
                if (velocity.y > 0)
                {
                    CurrentState = States.FALL;
                    return;
                }
                inputDirectionX = HorrizontalMovement(delta);

                // Handle Transitions:
                if (IsNextToWall() && !IsPlayerOnFloor())
                {
                    CurrentState = States.WALLSLIDE;
                }
                break;

            case States.WALLSLIDE:
                if (IsPlayerOnFloor())
                {
                    CurrentState = States.IDLE;
                    return;
                }
                inputDirectionX = Input.GetActionStrength("right") - Input.GetActionStrength("left");
                if ((lookDirection == 1 && inputDirectionX < 0) ||
                        (lookDirection == -1 && inputDirectionX > 0) || !IsNextToWall())
                {
                    CurrentState = States.FALL;
                }
                velocity.y += gravity * 0.2f * delta;
                Move();
                break;

            case States.DEATH:
                break;

            default:
                break;
        }
    }

    private float HorrizontalMovement(float delta)
    {
        var inputDirectionX = Input.GetActionStrength("right") - Input.GetActionStrength("left");

        if (Input.IsActionPressed("right") && Input.IsActionPressed("left"))
        {
            inputDirectionX = lookDirection;
        }

        UpdateDirection(inputDirectionX);
        velocity.x = walkSpeed * inputDirectionX;
        ApplyGravity(delta);
        Move();
        return inputDirectionX;
    }

    private void Move() => velocity = MoveAndSlide(velocity, Vector2.Up);

    private bool IsNextToWall() => IsNextToLeftWall() || IsNextToRightWall();

    private bool IsNextToRightWall() => rightWallChecker1.IsColliding() && rightWallChecker2.IsColliding();

    private bool IsNextToLeftWall() => leftWallChecker1.IsColliding() && leftWallChecker2.IsColliding();

    private bool IsPlayerOnFloor() => floorChecker1.IsColliding() || floorChecker2.IsColliding();

    // Initialization for the State occurs when entering a state.
    // This is meant for 
    // setting up the flags, animation and Sounds
    private void InitState(States nextState)
    {
        switch (nextState)
        {
            case States.IDLE:
                canJump = true;
                velocity.x = 0;
                animationPlayer.Play("Idle");
                break;

            case States.RUN:
                animationPlayer.Play("Run");
                break;

            case States.JUMP:
                canJump = false;
                canDoubleJump = true;
                velocity.y = jumpSpeed;
                animationPlayer.Play("Jump");
                break;

            case States.FALL:
                if (canJump)
                    coyoteTimer.Start();
                animationPlayer.Play("Fall");
                break;

            case States.DOUBLEJUMP:
                canDoubleJump = false;
                canJump = false;
                velocity.y = doubleJumpSpeed;
                animationPlayer.Play("DoubleJump");
                break;

            case States.WALLSLIDE:
                canDoubleJump = true;
                velocity = Vector2.Zero;
                animationPlayer.Play("WallSlide");
                break;

            case States.DASH:
                break;

            case States.DEATH:
                break;

            default: break;
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
        this.lookDirection = lookDirection;
        animatedSprite.FlipH = flipH;
    }

    private void ApplyGravity(float delta)
    {
        velocity.y += gravity * delta;
    }

    private void OnDashFinished() => isDashing = false;
    private void ResetDashCounter(int value) => numDash = value;
    private bool HasDashes() => numDash > 0;

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
