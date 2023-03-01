using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class FallingPlatform : Node2D
{
    private Tween _tween;
    private AnimatedSprite _sprite;
    private Timer _timer;

    [Export] private float _xMax = 2.0f;
    [Export] private float _rMax = 5f;

    private const float _stopThreshold = 0.1f;
    private const float _tweenDuration = 0.05f;
    private const float _recoveryFactor = 0.66f; 
    private const Tween.TransitionType _transitionType = Tween.TransitionType.Sine;
    
    private float _x, _r;
    private int _currentDir = 0;
    private bool _left = true;
    private bool _tweenMe;

    //[Signal] public delegate void TweenFinished(int value);
	
	public override void _Ready()
    {
        _tween = GetNode<Tween>("ShakeTween");
        _sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _timer = GetNode<Timer>("Timer");
        _timer.WaitTime = _tweenDuration;
        _currentDir = -1;
    }

    public override void _Process(float delta)
    {
        if (!_tweenMe) return;
        if (_x > _stopThreshold)
        {
            switch (_currentDir)
            {
                case -1:
                    TiltLeft(_x, _r);
                    break;
                case 1:
                    TiltRight(_x, _r);
                    break;
            }
        } 
        else
        {
            _tweenMe = false;
        }
    }

    private void StartTween()
    {
        _tweenMe = true;
        _x = _xMax;
        _r = _rMax;
    }
    
    private void TweenFinished(bool finished = false)
    {
        _x *= _recoveryFactor;
        _r *= _recoveryFactor;
        Recenter(finished);
    }

    private void Recenter(bool finished)
    {
        _tween.InterpolateProperty(
            _sprite,
            "position:x",
            _sprite.Position.x,
            0,
            _tweenDuration,
            _transitionType,
            Tween.EaseType.Out
        );
        
        _tween.InterpolateProperty(
            _sprite,
            "rotation_degrees",
            _sprite.RotationDegrees,
            0,
            _tweenDuration,
            _transitionType,
            Tween.EaseType.Out
        );
        
        _tween.Start();
        if (!_tweenMe)
        {
            _timer.Stop();
        }
        else
        {
            _timer.Start();
            _left = !_left;
            _currentDir = _left ? -1 : 1;
        }
    }
    
    private void TiltLeft(float x, float r)
    {
        _tween.InterpolateProperty(
            _sprite,
            "position:x",
            0,
            -x,
            _tweenDuration,
            _transitionType,
            Tween.EaseType.Out
            );
        
        _tween.InterpolateProperty(
            _sprite,
            "rotation_degrees",
            0,
            r,
            _tweenDuration,
            _transitionType,
            Tween.EaseType.Out
        );
        _tween.Start();
        _timer.Start();
        _currentDir = 0;
    }
    
    private void TiltRight(float x, float r)
    {
        _tween.InterpolateProperty(
            _sprite,
            "position:x",
            0,
            x,
            _tweenDuration,
            _transitionType,
            Tween.EaseType.Out
        );
        
        _tween.InterpolateProperty(
            _sprite,
            "rotation_degrees",
            0,
            r,
            _tweenDuration,
            _transitionType,
            Tween.EaseType.Out
        );
        _timer.Start();
        _tween.Start();
        _currentDir = 0;
    }

    public void OnArea2DBodyShapeEntered(RID bodyRid, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        if (!(body is PlayerTwo))
            return;
        StartTween();

    }
    
    private void OnTimerTimeout()
    {
        TweenFinished();
    }
}
