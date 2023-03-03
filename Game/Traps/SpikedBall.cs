using System;
using Godot;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public class SpikedBall : Position2D
{
    [Export] private float _chainLength = 48.0f;
    [Export] private float _swingSpeed = 120.0f;
    [Export] private float _startingRotation = -45;
    [Export] private float _endingRotation = 45;
    [Export] private int _spikeDamage = 1;
    
    private Sprite _chainSprite;
    private Area2D _spikeBall;

    private const float _spriteSize = 8;

    [Export] private Vector2 _chainSize = new Vector2(8, -72);
    [Export] private Vector2 _chainOffset = new Vector2(0, -40);

    private int _direction = 1;
    private float _posX;
    
	public override void _Ready()
    {
        _chainSprite = GetNode<Sprite>("Chain");
        _spikeBall = GetNode<Area2D>("Chain/SpikedBall");
        InitializeChainSprite();
        _posX = _spikeBall.GlobalPosition.x;
    }
    
    public override void _PhysicsProcess(float delta)
    {
        if (this.RotationDegrees < _startingRotation)
            _direction = 1;
        else if (this.RotationDegrees > _endingRotation)
            _direction = -1;
        var newSpeed = _swingSpeed * (1 - Mathf.Abs(_posX - _spikeBall.GlobalPosition.x)/100);
        this.RotationDegrees += newSpeed * _direction * delta;
    }
    
    private void InitializeChainSprite()
    {
        _chainLength += _chainLength % _spriteSize;
        _chainSize = new Vector2(_spriteSize, -_chainLength + _spriteSize);
        _chainOffset = new Vector2(0, -_chainLength / 2);
        _chainSprite.RegionRect = new Rect2(Vector2.Zero, _chainSize);
        _chainSprite.Offset = _chainOffset;
        _spikeBall.Position = new Vector2(0, _chainLength);
    }

    public void OnSpikedBallAreaShapeEntered(RID areaRid, Area2D area, int bodyShapeIndex, int localShapeIndex)
    {
        if (area.Owner is PlayerTwo playerTwo)
        {
            playerTwo.GetHit(_spikeDamage, Math.Sign(playerTwo.Position.x - Position.x));
        }
    }
    
}
