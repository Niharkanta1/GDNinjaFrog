using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class PlatformV3 : Node2D
{
    [Export] private Vector2 _moveTo = Vector2.Up * 120;
    [Export] private float _moveSpeed = 500.0f;
    [Export] private int _chainSizeX = 8;
    [Export] private int _chainSizeY = 240;
    [Export] private int _chainOffsetX = 0;
    [Export] private int _chainOffsetY = -120;
    
    private KinematicBody2D _platformBody;
    private Tween _moveTween;
    private AnimatedSprite _animatedSprite;
    private Sprite _chainSprite;

    private readonly float _idleDuration = 1.0f;
    private Vector2 _follow = Vector2.Zero;

    public override void _Ready()
    {
        _platformBody = GetNode<KinematicBody2D>("KinematicBody2D");
        _moveTween = GetNode<Tween>("MoveTween");
        _animatedSprite = GetNode<AnimatedSprite>("KinematicBody2D/AnimatedSprite");
        _chainSprite = GetNode<Sprite>("ChainPosition/Sprite");
        
        /*
         * Chain Position Y =  - Move To Y
         * Offset Y = Position Y - (direction) SpriteSize Y
         */
        _chainSprite.RegionRect = new Rect2(Vector2.Zero, new Vector2(_chainSizeX, _chainSizeY));
        _chainSprite.Offset = new Vector2(_chainOffsetX, _chainOffsetY);

        _animatedSprite.Play("Active");
        var duration = _moveTo.Length() / _moveSpeed * 16;
        _moveTween.InterpolateProperty(
            this, 
            "_follow", 
            Vector2.Zero, 
            _moveTo, 
            duration, 
            Tween.TransitionType.Linear,
            Tween.EaseType.InOut,
            _idleDuration
            );
        _moveTween.InterpolateProperty(
            this, 
            "_follow",
            _moveTo, 
            Vector2.Zero,
            duration, 
            Tween.TransitionType.Linear,
            Tween.EaseType.InOut,
            duration + _idleDuration * 2
        );

        _moveTween.Start();
        
    }

    public override void _PhysicsProcess(float delta)
    {
        _platformBody.Position = _platformBody.Position.LinearInterpolate(_follow, 0.075f);
    }

}
