namespace Crygotchi;

// TODO: Better Cry Input handling
//! Cry controls shouldn't be hardcoded like it is now

public partial class CryController : CharacterBody3D
{
    [ExportCategory("Character Controller")]
    [ExportGroup("References")]
    [Export] public Node3D Camera;

    [ExportGroup("Settings")]
    [Export] public float MoveSpeed = 100.0f;
    [Export] public float MaxSpeed = 50.0f;
    [Export] public float Gravity = -80.0f;

    private AnimationPlayer _animator;
    private CryState _state;

    private Vector3 _inputDirection = Vector3.Zero;
    private Vector3 _moveDirection = Vector3.Zero;
    private bool _isRunning = false;

    public override void _Ready()
    {
        this._animator = this.GetNode<AnimationPlayer>("./AnimationPlayer");
        this._state = this.GetNode<CryState>("/root/CryState");

        this._animator.Play("Idle");
    }

    public override void _Process(double delta)
    {
        if (this._state.IsBusy()) return;

        this._inputDirection = GetInputDirection();
        this._isRunning = Input.IsKeyPressed(Key.Shift);
    }
    public override void _PhysicsProcess(double delta)
    {
        // only forward direction if player trying to move forward or right
        Vector3 forwards = this.Camera.GlobalTransform.Basis.Z * this._inputDirection.Z;
        Vector3 right = this.Camera.GlobalTransform.Basis.X * this._inputDirection.X;

        // Get Move Direction relative to the camera
        this._moveDirection = forwards + right;
        if (this._moveDirection.Length() > 1.0f) this._moveDirection = this._moveDirection.Normalized();
        this._moveDirection.Y = 0.0f;

        //* Move character
        this.Velocity = GetVelocity(this._moveDirection, (float)delta);
        this.MoveAndSlide();

        if (this._moveDirection.Length() > 0.001)
        {
            //* If moving, look towards it
            this.LookAt(this.GlobalTransform.Origin + (-this._moveDirection), Vector3.Up);
            this._animator.Play(this._isRunning ? "Running" : "Walking");
            return;
        }

        //* Not moving, just stop and go idle
        this._animator.Play("Idle");
    }

    private Vector3 GetInputDirection()
    {
        return new Vector3(
            Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"),
            0.0f,
            Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up")
        );
    }

    private Vector3 GetVelocity(Vector3 direction, float delta)
    {
        Vector3 velocity = direction * delta * this.MoveSpeed;

        //* Clamp the velocity down
        if (velocity.Length() > this.MaxSpeed) velocity = velocity.Normalized() * this.MaxSpeed;

        if (this._isRunning) velocity *= 5f;

        //* Add gravity force and return it
        velocity.Y = velocity.Y + this.Gravity * delta;
        return velocity;
    }
}
