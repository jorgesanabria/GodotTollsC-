using Godot;
using MaquinaDeEstados.FSM;
using System;

public class Player : KinematicBody2D
{
    [Export]
    public Vector2 GravityVector = new Vector2(0f, 9.8f);
    [Export]
    public Vector2 GlobalVelocity = Vector2.Zero;
    [Export]
    public Vector2 FloorNormal = new Vector2(0f, -1f);
    [Export]
    public float HorizontalSpeed = 50f;
    [Export]
    public float JumpForce = 100f;
    [Export]
    public float WallJumpHorizontalForce = 50f;
    private float _wallTime = 0f;
    [Export]
    public float WallTime = 0.5f;
    protected Fsm<PlayerState> _fsm;
    public override void _Ready()
    {
        _fsm = new Fsm<PlayerState> { Initial = PlayerState.OnAir };
        _fsm.Add(PlayerState.OnAir, (current, delta) =>
        {
            if (!IsOnFloor() && !IsOnWall())
            {
                GlobalVelocity += GravityVector;
                return PlayerState.OnAir;
            }
            return IsOnFloor() && !IsOnWall() ? PlayerState.OnGround : current;
        });
        _fsm.Add(PlayerState.OnGround, (current, delta) =>
        {
            if (Input.IsActionPressed("ui_up"))
            {
                var jump = new Vector2(GlobalVelocity.x, -JumpForce);
                GlobalVelocity = jump;
                return PlayerState.OnAir;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnGround, (current, delta) =>
        {
            var horizontal = Vector2.Zero;
            if (Input.IsActionPressed("ui_left"))
            {
                horizontal = new Vector2((-1 * HorizontalSpeed), GlobalVelocity.y);
            }
            if (Input.IsActionPressed("ui_right"))
            {
                horizontal = new Vector2((HorizontalSpeed), GlobalVelocity.y);
            }
            GlobalVelocity = horizontal;
            return current;
        });
        _fsm.Add(PlayerState.OnAir, (current, delta) =>
        {
            if (_wallTime > 0) return current;
            var horizontal = Vector2.Zero;
            if (Input.IsActionPressed("ui_left"))
            {
                horizontal = new Vector2((-1 * HorizontalSpeed), GlobalVelocity.y);
                GlobalVelocity = horizontal;
            }
            if (Input.IsActionPressed("ui_right"))
            {
                horizontal = new Vector2((HorizontalSpeed), GlobalVelocity.y);
                GlobalVelocity = horizontal;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnAir, (current, delta) =>
        {
            if (IsOnFloor())
            {
                GlobalVelocity.x = 0;
                return PlayerState.OnGround;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnAir, (current, delta) =>
        {
            if (IsOnWall())
            {
                GlobalVelocity = GravityVector;
                var isLef = _getCollisionNormal() == Vector2.Left;
                return isLef ? PlayerState.OnRightWall : PlayerState.OnLeftWall;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnLeftWall, (current, delta) => IsOnFloor() ? PlayerState.OnGround : current);
        _fsm.Add(PlayerState.OnRightWall, (current, delta) => IsOnFloor() ? PlayerState.OnGround : current);
        _fsm.Add(PlayerState.OnLeftWall, (current, delta) =>
        {
            if (Input.IsActionPressed("ui_right"))
            {
                var jump = new Vector2(HorizontalSpeed, -JumpForce);
                GlobalVelocity = jump;
                return PlayerState.OnAir;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnRightWall, (current, delta) =>
        {
            if (Input.IsActionPressed("ui_left"))
            {
                var jump = new Vector2(HorizontalSpeed * -1, -JumpForce);
                GlobalVelocity = jump;
                return PlayerState.OnAir;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnLeftWall, (current, delta) =>
        {
            if (Input.IsActionPressed("ui_up"))
            {
                var jump = new Vector2(WallJumpHorizontalForce, -JumpForce);
                GlobalVelocity = jump;
                _wallTime = WallTime;
                return PlayerState.OnAir;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnRightWall, (current, delta) =>
        {
            if (Input.IsActionPressed("ui_up"))
            {
                var jump = new Vector2(WallJumpHorizontalForce * -1, -JumpForce);
                GlobalVelocity = jump;
                _wallTime = WallTime;
                return PlayerState.OnAir;
            }
            return current;
        });
    }

    private Vector2 _getCollisionNormal()
    {
        var lastIndex = GetSlideCount() - 1;
        var wallNormal = GetSlideCollision(lastIndex).Normal;
        return wallNormal;
    }

    public override void _PhysicsProcess(float delta)
    {
        _fsm.Tick(delta);
        GlobalVelocity = MoveAndSlide(GlobalVelocity, FloorNormal);
        _wallTime = Mathf.Clamp(_wallTime - delta, 0, 10);
    }
}
