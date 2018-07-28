using Godot;
using MaquinaDeEstados;
using MaquinaDeEstados.FSM;
using MaquinaDeEstados.InputHandler;
using System;
using System.Collections.Generic;

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
    protected FiniteStateMachine<PlayerState, Player> _fsm;
    protected InputHandler<InputActions, string> _input;
    [Export]
    public NodePath CollisionWeaponNode;
    public override void _Ready()
    {
        _input = new InputHandler<InputActions, string>(
            map: new Dictionary<InputActions, string>
            {
                { InputActions.MoveLeft, "ui_left" },
                { InputActions.MoveRight, "ui_right" },
                { InputActions.Jump, "ui_up" },
                { InputActions.Attack, "ui_accept" }
            },
            actionJustPressed: Input.IsActionJustPressed,
            actionPressed: Input.IsActionPressed,
            actionReleased: Input.IsActionJustReleased
        );
        _fsm = new FiniteStateMachine<PlayerState, Player>(equalizer: (current, captured) => current == captured) { InitialState = PlayerState.OnAir };
        _fsm.Add(PlayerState.OnAir, (current, player) =>
        {
            if (!IsOnFloor() && !IsOnWall())
            {
                GlobalVelocity += GravityVector;
                return PlayerState.OnAir;
            }
            return IsOnFloor() && !IsOnWall() ? PlayerState.OnGround : current;
        });
        _fsm.Add(PlayerState.OnGround, (current, player) =>
        {
            if (_input.IsActionPressed(InputActions.Jump))
            {
                var jump = new Vector2(GlobalVelocity.x, -JumpForce);
                GlobalVelocity = jump;
                return PlayerState.OnAir;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnGround, (current, player) =>
        {
            var horizontal = Vector2.Zero;
            if (_input.IsActionPressed(InputActions.MoveLeft))
            {
                horizontal = new Vector2((-1 * HorizontalSpeed), GlobalVelocity.y);
            }
            if (_input.IsActionPressed(InputActions.MoveRight))
            {
                horizontal = new Vector2((HorizontalSpeed), GlobalVelocity.y);
            }
            GlobalVelocity = horizontal;
            return current;
        });
        _fsm.Add(PlayerState.OnAir, (current, player) =>
        {
            if (_wallTime > 0) return current;
            var horizontal = Vector2.Zero;
            if (_input.IsActionPressed(InputActions.MoveLeft))
            {
                horizontal = new Vector2((-1 * HorizontalSpeed), GlobalVelocity.y);
                GlobalVelocity = horizontal;
            }
            if (_input.IsActionPressed(InputActions.MoveRight))
            {
                horizontal = new Vector2((HorizontalSpeed), GlobalVelocity.y);
                GlobalVelocity = horizontal;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnAir, (current, player) =>
        {
            if (IsOnFloor())
            {
                GlobalVelocity.x = 0;
                return PlayerState.OnGround;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnAir, (current, player) =>
        {
            if (IsOnWall())
            {
                GlobalVelocity = GravityVector;
                var isLef = _getCollisionNormal() == Vector2.Left;
                return isLef ? PlayerState.OnRightWall : PlayerState.OnLeftWall;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnLeftWall, (current, player) => IsOnFloor() ? PlayerState.OnGround : current);
        _fsm.Add(PlayerState.OnRightWall, (current, player) => IsOnFloor() ? PlayerState.OnGround : current);
        _fsm.Add(PlayerState.OnGround, (current, player) => !IsOnFloor() ? PlayerState.OnAir : current);
        _fsm.Add(PlayerState.OnLeftWall, (current, player) =>
        {
            if (_input.IsActionPressed(InputActions.MoveRight))
            {
                var jump = new Vector2(HorizontalSpeed, -JumpForce);
                GlobalVelocity = jump;
                return PlayerState.OnAir;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnRightWall, (current, player) =>
        {
            if (!_input.IsActionPressed(InputActions.MoveRight))
            {
                return PlayerState.OnAir;
            }
            if (_input.IsActionPressed(InputActions.MoveLeft))
            {
                var jump = new Vector2(HorizontalSpeed * -1, -JumpForce);
                GlobalVelocity = jump;
                return PlayerState.OnAir;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnLeftWall, (current, player) =>
        {
            if (!_input.IsActionPressed(InputActions.MoveLeft))
            {
                return PlayerState.OnAir;
            }
            if (_input.IsActionJustPressed(InputActions.Jump))
            {
                var jump = new Vector2(WallJumpHorizontalForce, -JumpForce);
                GlobalVelocity = jump;
                _wallTime = WallTime;
                return PlayerState.OnAir;
            }
            return current;
        });
        _fsm.Add(PlayerState.OnRightWall, (current, player) =>
        {
            if (_input.IsActionJustPressed(InputActions.Jump))
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
        _fsm.Tick(this);
        GlobalVelocity = MoveAndSlide(GlobalVelocity, FloorNormal);
        _wallTime = Mathf.Clamp(_wallTime - delta, 0, 10);
    }
}
