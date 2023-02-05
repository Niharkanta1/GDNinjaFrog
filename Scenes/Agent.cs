using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Agent : KinematicBody2D
{
    [Export] private int health = 3;
    [Export] private bool canBeHit = true;

    public bool CanBeHit
    {
        get => canBeHit;
        set => canBeHit = value;

    }

    public int Health
    {
        get => health;
        set => health = value;
    }

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
