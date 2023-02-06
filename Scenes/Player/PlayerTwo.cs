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

    [Export] private int gravity = 1000;
    [Export] private int jumpSpeed = -200;
    [Export] private int walkSpeed = 150;
    [Export] private int dashSpeed = 300;
    [Export] private int numDash = 1;

    private Vector2 velocity;
    private int lookDirection = 1;
    private bool isAttacking = false;
    private bool isDashing = false;

    private States currentState;
    enum States { IDLE, WALK, FALL, JUMP, DOUBLEJUMP, DASH, WALLSLIDE, DEATH };


    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite>("Body/AnimatedSprite");
        animationPlayer = GetNode<AnimationPlayer>("Body/AnimationPlayer");
        body = GetNode<Node2D>("Body");

        currentState = States.IDLE;
        velocity = Vector2.Zero;
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (currentState)
        {
            // Idle State
            case States.IDLE:
                if (!IsOnFloor())
                {
                    if (velocity.y > 0)
                    {
                        currentState = States.FALL;
                        return;
                    }
                }
                animationPlayer.Play("Idle");
                if (Input.IsActionPressed("left") || Input.IsActionPressed("right"))
                {
                    currentState = States.WALK;
                }
                if (Input.IsActionJustPressed("jump"))
                {
                    currentState = States.JUMP;
                }
                break;

            // Walk State
            case States.WALK:
                animationPlayer.Play("Run");
                var inputDirectionX = Input.GetActionStrength("right") - Input.GetActionStrength("left");

                if (Input.IsActionPressed("right") && Input.IsActionPressed("left"))
                {
                    inputDirectionX = lookDirection;
                }

                UpdateDirection(inputDirectionX);
                velocity.x = walkSpeed * inputDirectionX;
                ApplyGravity(delta);
                velocity = MoveAndSlide(velocity, Vector2.Up);

                if (IsEqualApprox(inputDirectionX, 0.0f))
                    currentState = States.IDLE;

                break;

            // Fall State
            case States.FALL:
                animationPlayer.Play("Fall");

                if (IsOnFloor())
                {
                    currentState = States.IDLE;
                    return;
                }
                break;


            // Jump State
            case States.JUMP:
                velocity.y = jumpSpeed;
                animationPlayer.Play("Jump");
                if (velocity.y > 0)
                {
                    currentState = States.FALL;
                    return;
                }

                inputDirectionX = Input.GetActionStrength("right") - Input.GetActionStrength("left");
                UpdateDirection(inputDirectionX);
                velocity.x = walkSpeed * inputDirectionX;
                ApplyGravity(delta);
                velocity = MoveAndSlide(velocity, Vector2.Up);

                break;

            case States.DASH:
                break;

            case States.DOUBLEJUMP:
                break;

            case States.WALLSLIDE:
                break;

            case States.DEATH:
                break;

            default:
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
        this.lookDirection = lookDirection;
        animatedSprite.FlipH = flipH;
    }

    private void ApplyGravity(float delta)
    {
        velocity.y += gravity * delta;
    }

    private void OnAttackFinished() => isAttacking = false;
    private void OnDashFinished() => isDashing = false;
    private void ResetDashCounter(int value) => numDash = value;
    private bool HasDashes() => numDash > 0;

    // Utils
    float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    public static bool IsEqualApprox(float a, float b)
    {
        return IsEqualApprox(a, b, 0.00001);
    }

    private static bool IsEqualApprox(double left, double right, double delta)
    {
        return Math.Abs(left - right) < delta;
    }
}
