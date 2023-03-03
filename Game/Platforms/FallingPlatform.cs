using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class FallingPlatform : Node2D
{
    private Tween _tween;
    private AnimatedSprite _sprite;
    private Timer _timer, _fallTimer, _reactivateTimer;
    private Particles2D _fallParticle, _dustParticle;
    private CollisionShape2D _collisionShape2D, _activationCollisionArea;
        
    [Export] private float _xMax = 2.0f;
    [Export] private float _rMax = 5f;

    private const float _stopThreshold = 0.1f;
    private const float _tweenDuration = 0.05f;
    private const float _recoveryFactor = 0.66f; 
    private const Tween.TransitionType _transitionType = Tween.TransitionType.Sine;
    
    private float _x, _r;
    private int _currentDir = 0;
    private bool _left = true;
    private bool _isShaking;
    private bool _isFalling, _isActivated;

    //[Signal] public delegate void TweenFinished(int value);
	
	public override void _Ready()
    {
        _tween = GetNode<Tween>("ShakeTween");
        _sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _timer = GetNode<Timer>("Timers/Timer");
        _fallTimer = GetNode<Timer>("Timers/FallTimer");
        _reactivateTimer = GetNode<Timer>("Timers/ReactivateTimer");
        _fallParticle = GetNode<Particles2D>("Particles/FallParticle");
        _dustParticle = GetNode<Particles2D>("Particles/DustParticle");
        _collisionShape2D = GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D");
        _activationCollisionArea = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        _timer.WaitTime = _tweenDuration;
        _currentDir = -1;
    }

    public override void _Process(float delta)
    {
        if (!_isShaking) return;
        if (_x > _stopThreshold)
        {
            TweenObject(_x, _r, _currentDir);
        }
        else
        {
            _isShaking = false;
        }
        if (_isFalling && !_isShaking)
        {
            Fall();
        }
    }

    private void StartTween()
    {
        _isShaking = true;
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
        if (!_isShaking)
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


    private void TweenObject(float x, float r, int dir)
    {
        if (dir == 0) return;
        _tween.InterpolateProperty(
            _sprite,
            "position:x",
            0,
            x * dir,
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
    
    private void Fall()
    {
        _fallParticle.Emitting = true;
        _isFalling = false; // Only Once we need to call Fall Method.
        _collisionShape2D.Disabled = true;
        _activationCollisionArea.Disabled = true;
        _sprite.Visible = false;
        _reactivateTimer.Start();
    }

    private void Respawn()
    {
        _fallParticle.Emitting = false;
        _collisionShape2D.Disabled = false;
        _activationCollisionArea.Disabled = false;
        _sprite.Visible = true;
        _isActivated = false;
    }
   
    public void OnArea2DBodyShapeEntered(RID bodyRid, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        if (!(body is PlayerTwo) && _isActivated)
            return;
        StartTween();
        _fallTimer.Start();
        _isActivated = true;
    }
    
    public void OnTimerTimeout()
    {
        TweenFinished();
    }

    public void OnFallTimerTimeout()
    {
        StartTween();
        _sprite.Play("Off");
        _dustParticle.Emitting = false;
        _isFalling = true;
    }

    public void OnReactivateTimerTimeout()
    {
        Respawn();
        StartTween();
        _sprite.Play("Active");
        _dustParticle.Emitting = true;
    }

}
