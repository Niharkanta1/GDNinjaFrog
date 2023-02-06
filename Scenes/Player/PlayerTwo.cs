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

    enum State { Move, Wall, Fly, DoubleJump, Dead }

    private State currentState;
    private Vector2 velocity;
    private int lookDirection = 1;
    private float direction = 1;

    private float gravity = 1000;
    private float moveSpeed = 100;
    private float acceleration = 0.5f;
    private float deacceleration = 0.2f;

    private float jumpHeight = 350;
    private Vector2 snapVector = new Vector2(0, 10);
    private bool hasDoubleJump = true;
    private bool landed = false;


    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite>("Body/AnimatedSprite");
        animationPlayer = GetNode<AnimationPlayer>("Body/AnimationPlayer");
        body = GetNode<Node2D>("Body");
        ct = GetNode<Timer>("Timer/CT");
        ib = GetNode<Timer>("Timer/IB");

        currentState = State.Move;
        velocity = Vector2.Zero;
    }

    public override void _PhysicsProcess(float delta)
    {
        body.Scale = new Vector2(lookDirection, 1);
        switch (currentState)
        {
            default:
                break;
        }
    }


    // Utils
    float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }
}
