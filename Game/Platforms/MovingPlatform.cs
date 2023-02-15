using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class MovingPlatform : Node2D // Moving Platform Using Tween
{
    [Export] private float _idleDuration;
    [Export] private float _moveSpeed;
    [Export] private NodePath _moveToPosition;

    private Position2D _moveTo;
    private KinematicBody2D _platform;
    private Tween _moveTween;

    public override void _Ready()
    {
        _platform = GetNode<KinematicBody2D>("Platform");
        _moveTween = GetNode<Tween>("MoveTween");
        _moveTo = GetNode<Position2D>(_moveToPosition);
        var duration = _moveTo.Position.Length() / _moveSpeed;
        _moveTween.InterpolateProperty(
            _platform,
            "position",
            Vector2.Zero,
            _moveTo.Position,
            duration,
            Tween.TransitionType.Linear,
            Tween.EaseType.InOut,
            _idleDuration
        );
        _moveTween.InterpolateProperty(
            _platform,
            "position",
            _moveTo.Position,
            Vector2.Zero,
            duration + _idleDuration,
            Tween.TransitionType.Linear,
            Tween.EaseType.InOut,
            _idleDuration
        );
        _moveTween.Start();
    }


}
