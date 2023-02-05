using System;
using System.Collections.Generic;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class AngryPig : Enemy
{
    [Export] private NodePath[] waypoints = new NodePath[2];
    [Export] private float walkSpeed = 100;
    [Export] private float runSpeed = 160;

    private AnimatedSprite animatedSprite;
    private AnimationTree animationTree;
    private Timer timer;

    private GameSettings gameSettings;
    private int waypointIndex = 0;
    private Vector2 wayPointPos;
    private Vector2 velocity;

    private float waypointMinDistance = 5.0f;
    private State currentState;

    enum State { Idle, Run, Walk, Hit }

    public override void _Ready()
    {
        gameSettings = (GameSettings)GetNode("/root/GameSettings");
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        animationTree = GetNode<AnimationTree>("AnimationTree");
        timer = GetNode<Timer>("Timer");

        wayPointPos = GetWaypointPosition(waypointIndex);
        velocity = Vector2.Zero;
        currentState = State.Walk;
    }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 direction = Position.DirectionTo(wayPointPos);
        float distance = new Vector2(Position.x, 0).DistanceTo(new Vector2(wayPointPos.x, 0));
        float moveSpeed = 0;
        switch (currentState)
        {
            case State.Walk:
                moveSpeed = walkSpeed;
                break;
            case State.Run:
                moveSpeed = runSpeed;
                break;
        }

        if (distance >= waypointMinDistance)
        {
            velocity.x = Math.Sign(direction.x) * moveSpeed;
            velocity.y = Math.Min(velocity.y + gameSettings.gravity, gameSettings.terminalVelocity);

            if (Math.Sign(direction.x) == 1)
                animatedSprite.FlipH = true;
            else if ((Math.Sign(direction.x) == -1))
                animatedSprite.FlipH = false;

            velocity = MoveAndSlide(velocity, Vector2.Up);
        }
        else
        {
            if (waypointIndex < waypoints.Length - 1)
                waypointIndex++;
            else
                waypointIndex = 0;
        }
        wayPointPos = GetWaypointPosition(waypointIndex);
    }

    private Vector2 GetWaypointPosition(int index)
    {
        return GetNode<Position2D>(waypoints[index]).Position;
    }

    public override void GetHit(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            // Play Enemy Death Animation
            QueueFree();
        }
        CanBeHit = false;
        currentState = State.Hit;
        animationTree.Set("parameters/Hit/active", true);
        animationTree.Set("parameters/HitVariation/blend_amount", gameSettings.randomGen.RandiRange(0, 1));
        timer.WaitTime = 0.5f;
        timer.Start();
    }

    // Signals

    public void OnDetectionZoneBodyShapeEntered(RID bodyRID, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        animationTree.Active = false;
        animationTree.Active = true;
        animationTree.Set("parameters/PlayerDetected/blend_position", 1);
        if (currentState == State.Walk)
            currentState = State.Run;
    }

    public void OnDetectionZoneBodyShapeExited(RID bodyRID, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        animationTree.Active = false;
        animationTree.Active = true;
        animationTree.Set("parameters/PlayerDetected/blend_position", 0);
        if (currentState == State.Run)
            currentState = State.Walk;
    }

    public void OnTimerTimeout()
    {
        CanBeHit = true;
        currentState = State.Run;
        timer.Stop();
    }

}
