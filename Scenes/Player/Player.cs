using Godot;
using System;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Player : KinematicBody2D
{
    [Export] private float moveSpeed = 200;
    [Export] private float jumpImpulse = 600;

    [Signal] public delegate void OnStateChange(string newState);

    private AnimationTree animationTree;
    private AnimatedSprite animatedSprite;

    private GameSettings gameSettings;
    private Vector2 input, velocity;
    private State currentState;

    private int jumps = 0;

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

    public override void _Ready()
    {
        gameSettings = (GameSettings)GetNode("/root/GameSettings");
        currentState = State.Idle;

        animationTree = GetNode<AnimationTree>("AnimationTree");
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        var input = GetPlayerInput();
        AdjustSpriteFlipDirection(input);

        velocity = new Vector2(
            input.x * moveSpeed,
            Math.Min(velocity.y + gameSettings.gravity, gameSettings.terminalVelocity)
        );

        velocity = MoveAndSlide(velocity, Vector2.Up);
        SetAnimParameters();
        CheckNextState();
    }

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
            jumps = 0;
            if (Input.IsActionJustPressed("jump") && jumps <= 2)
            {
                EnterState(State.Jump);
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
            // TO-DO: Double Jump
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
                PerformJump();
                break;
        }
        currentState = newState;
        EmitSignal(nameof(OnStateChange), newState.ToString());
    }

    private void PerformJump()
    {
        velocity.y = -jumpImpulse;
        jumps++;
    }
}
