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

    public virtual void GetHit(int damage, int direction = 1)
    {
        GD.Print("Agent GetHit Called. Override This Method");
    }

    public virtual void OnHitFinished()
    {

    }

}
