using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Enemy : Agent
{
    [Export] private int collisionDamage = 1;
    public override void _Ready()
    {

    }

    public override void GetHit(int damage)
    {
        GD.Print("Enemy GetHit Called. Override This Method");
        throw new NotImplementedException();
    }

    public void OnEnemyCollisionHitboxBodyShapeEntered(RID bodyRID, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        if (body is Player)
        {
            ((Player)body).GetHit(collisionDamage);
        }
    }
}
