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

    public override void GetHit(int damage)
    {
        GD.Print("Enemy GetHit Called. Override This Method");
    }

    public void OnEnemyCollisionHitBoxBodyShapeEntered(RID bodyRid, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        if (body is Player player)
        {
            player.GetHit(_collisionDamage);
        }
    }
}
