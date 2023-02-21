using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class PlatformV3 : Node2D
{
    [Export] private float _moveToDistance = 100;
    [Export] private float _moveSpeed = 800.0f;
    [Export] private int _spriteSize = 8;

    [Export] private Vector2 _chainSize = new Vector2(8, 128);
    [Export] private Vector2 _chainOffset = new Vector2(0, -60);

    [Export] private float _idleDuration = 1.0f;
    [Export] private MoveDirection _moveDirection = MoveDirection.Up;

    private enum MoveDirection { Up, Down, Right, Left };

    private KinematicBody2D _platformBody;
    private Tween _moveTween;
    private AnimatedSprite _animatedSprite;
    private Sprite _chainSprite;

    private readonly Vector2 _follow = Vector2.Zero;
    private Vector2 _moveTo = Vector2.Zero;

    public override void _Ready()
    {
        _platformBody = GetNode<KinematicBody2D>("KinematicBody2D");
        _moveTween = GetNode<Tween>("MoveTween");
        _animatedSprite = GetNode<AnimatedSprite>("KinematicBody2D/AnimatedSprite");
        _chainSprite = GetNode<Sprite>("ChainPosition/Sprite");
        
        _animatedSprite.Play("Active");
        
        InitializeChainPath();
        InitializeTween();
    }
    
    public override void _PhysicsProcess(float delta)
    {
        _platformBody.Position = _platformBody.Position.LinearInterpolate(_follow, 0.075f);
    }
    
    private void InitializeTween()
    {
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
    
    private void InitializeChainPath()
    {
        var extraChainSprite = _moveToDistance % _spriteSize;
        GD.Print(extraChainSprite);
        _moveToDistance += extraChainSprite;
        switch (_moveDirection)
        {
            case MoveDirection.Up:
                _moveTo = new Vector2(0, -_moveToDistance);
                _chainSize = new Vector2(_spriteSize, -(_moveTo.y) + _spriteSize);
                _chainOffset = new Vector2(0, _moveTo.y / 2);
                break;
            case MoveDirection.Down:
                _moveTo = new Vector2(0, _moveToDistance);
                _chainSize = new Vector2(_spriteSize, _moveTo.y + _spriteSize);
                _chainOffset = new Vector2(0, _moveTo.y / 2);
                break;
            case MoveDirection.Right:
                _moveTo = new Vector2(_moveToDistance, 0);
                _chainSize = new Vector2(_moveTo.x + _spriteSize, _spriteSize);
                _chainOffset = new Vector2(_moveTo.x / 2, 0);
                break;
            case MoveDirection.Left:
                _moveTo = new Vector2(-_moveToDistance, 0);
                _chainSize = new Vector2(-_moveTo.x + _spriteSize, _spriteSize);
                _chainOffset = new Vector2(_moveTo.x / 2, 0);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        /* Default:
         * Chain Size Y =  (-Move To Y) + 8
         * Offset Y = Chain Size Y / 2
         * Region Rect: ((0, 0), (8, 128)) and Offset: (0, -60)
         */
        _chainSprite.RegionRect = new Rect2(Vector2.Zero, _chainSize);
        _chainSprite.Offset = _chainOffset;
    }



}
