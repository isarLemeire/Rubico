using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    public class PlayerInputHandler
    {
        private PlayerStateContext _ctx;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;
        private InputAction _attackAction;

        public bool FaceRight { get; private set; }

        public float MoveX { get; private set; }
        public Vector2 JoyStickAim { get; private set; }
        public BufferedBool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public BufferedBool DashPressed { get; private set; }
        public BufferedBool AttackPressed { get; private set; }

        public PlayerInputHandler(PlayerStateContext ctx)
        {
            _ctx = ctx;
            _moveAction = InputSystem.actions.FindAction("Move");
            _jumpAction = InputSystem.actions.FindAction("Jump");
            _dashAction = InputSystem.actions.FindAction("Dash");
            _attackAction = InputSystem.actions.FindAction("attack");

            JumpPressed = new BufferedBool(ctx.Stats.ForgivenessTime);
            DashPressed = new BufferedBool(ctx.Stats.ForgivenessTime);
            AttackPressed = new BufferedBool(ctx.Stats.ForgivenessTime);

            FaceRight = true;
        }

        public void Read()
        {
            if (!_ctx.inputBlock)
            {
                MoveX = getMoveX();
                JoyStickAim = correctAim(SnapAim(_moveAction.ReadValue<Vector2>()));
                UpdateFaceRight();
            }


            JumpPressed.Set(_jumpAction.WasPressedThisFrame());
            JumpHeld = _jumpAction.IsPressed();
            DashPressed.Set(_dashAction.WasPressedThisFrame());
            AttackPressed.Set(_attackAction.WasPressedThisFrame());
        }

        public void Update(float deltaTime)
        {
            JumpPressed.Update(deltaTime);
            DashPressed.Update(deltaTime);
            AttackPressed.Update(deltaTime);
        }

        private void UpdateFaceRight()
        {
            if (Mathf.Abs(MoveX) != 0)
                FaceRight = MoveX >= 0;
        }

        private float getMoveX()
        {
            Vector2 joystick = _moveAction.ReadValue<Vector2>();
            return Mathf.Abs(joystick.x) < _ctx.Stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(joystick.x);
        }


        private Vector2 SnapAim(Vector2 input)
        {
            float inputX = Mathf.Abs(input.x) < _ctx.Stats.HorizontalDeadZoneThreshold ? 0 : input.x;
            float inputY = Mathf.Abs(input.y) < _ctx.Stats.HorizontalDeadZoneThreshold ? 0 : input.y;

            if (inputX == 0 && inputY == 0)
            {
                return FaceRight ? new Vector2(1, 0) : new Vector2(-1, 0);
            }

            // Compute angle in radians from -pi to pi
            float angle = Mathf.Atan2(inputY, inputX);

            // Divide the circle into 8 equal slices (45 each).
            // Add 22.5 so we round to nearest instead of floor.
            float slice = Mathf.PI / 4f; // 45 degrees
            int sector = Mathf.RoundToInt(angle / slice);

            // Convert back to a snapped unit vector
            float snappedAngle = sector * slice;
            return new Vector2(Mathf.Cos(snappedAngle), Mathf.Sin(snappedAngle)).normalized;
        }

        private Vector2 correctAim(Vector2 aim)
        {
            Vector2 dashDir = aim;

            if (_ctx.grounded.IsTrue && aim.y < 0) // Downwards dash input while grounded
            {
                if (Mathf.Abs(dashDir.x) < 0.1)
                    dashDir.x = FaceRight ? 1f : -1f; // follow facing direction
                else
                    dashDir.x = Mathf.Sign(dashDir.x); // follow input direction

                dashDir.y = 0; // force pure horizontal dash
            }
            return dashDir;
        }

    }
}
