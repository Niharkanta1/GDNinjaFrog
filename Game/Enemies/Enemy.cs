using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Enemy : Agent
{
    [Export] private int _collisionDamage = 1;
    public override void _Ready()
    {

    }

    public override void GetHit(int damage, int direction = 1)
    {
        GD.Print("Enemy GetHit Called. Override This Method");
    }

    public void OnEnemyCollisionHitBoxBodyShapeEntered(RID bodyRid, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        switch (body)
        {
            case Player player:
                player.GetHit(_collisionDamage);
                break;
            case PlayerTwo playerTwo:
                playerTwo.GetHit(_collisionDamage, Math.Sign(playerTwo.Position.x - Position.x));
                break;
        }

    }

    public void OnEnemyCollisionHitBoxAreaShapeEntered(RID areaRid, Area2D area, int bodyShapeIndex,
        int localShapeIndex)
    {
        if (area.Owner is PlayerTwo playerTwo)
        {
            playerTwo.GetHit(_collisionDamage, Math.Sign(playerTwo.Position.x - Position.x));
        }
    }
}
