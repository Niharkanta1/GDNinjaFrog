using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class FireTrap : StaticBody2D
{
    [Export] private int _fireDamage = 1;
	
    private enum States { On, Off, Trigger }

    private States _currentState;
    
	public override void _Ready()
    {
        _currentState = States.Off;
    }

    public void OnArea2DAreaShapeEntered(RID areaRid, Area2D area, int bodyShapeIndex, int localShapeIndex)
    {
        if (area.Owner is PlayerTwo playerTwo)
        {
            playerTwo.GetHit(_fireDamage, Math.Sign(playerTwo.Position.x - Position.x));
        }
    }
}
