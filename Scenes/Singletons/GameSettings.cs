using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class GameSettings : Node
{
    [Export] public float gravity = 50;
    [Export] public float terminalVelocity = 300;
    [Export] public bool shouldRandomize = true;

    public RandomNumberGenerator randomGen;

    public override void _Ready()
    {
        randomGen = new RandomNumberGenerator();
        if (shouldRandomize)
        {
            randomGen.Randomize();
        }
    }
}
