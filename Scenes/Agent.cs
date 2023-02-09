using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Agent : KinematicBody2D
{
    [field: Export]
    public bool CanBeHit { get; protected set; } = true;

    [field: Export]
    protected int Health { get; set; } = 3;

    public override void _Ready()
    {

    }

    public virtual void GetHit(int damage)
    {
        GD.Print("Agent GetHit Called. Override This Method");
        throw new NotImplementedException();
    }

    public virtual void OnHitFinished()
    {
        throw new NotImplementedException();
    }

}
