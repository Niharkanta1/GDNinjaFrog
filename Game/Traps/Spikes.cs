using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class Spikes : StaticBody2D
{
    [Export] private int _spikeDamage = 1;
	
	public override void _Ready()
    {
        
    }

    public void OnSpikesAreaShapeEntered(RID areaRid, Area2D area, int bodyShapeIndex, int localShapeIndex)
    {
        if (area.Owner is PlayerTwo playerTwo)
        {
            playerTwo.GetHit(_spikeDamage, Math.Sign(playerTwo.Position.x - Position.x));
        }
    }
	
}
