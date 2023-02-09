using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class GameSettings : Node
{
    [Export] public float Gravity = 50;
    [Export] public float TerminalVelocity = 300;
    [Export] public bool ShouldRandomize = true;

    public RandomNumberGenerator RandomGen;

    public override void _Ready()
    {
        RandomGen = new RandomNumberGenerator();
        if (ShouldRandomize)
        {
            RandomGen.Randomize();
        }
    }
}
