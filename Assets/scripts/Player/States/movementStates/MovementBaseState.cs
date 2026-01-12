using UnityEngine;

namespace Player
{

    public abstract class MovementBaseState : PlayerStateBase
    {
        protected MovementBaseState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override abstract void Enter();
        public override abstract void Exit();
        public override abstract void Update();
        public override abstract void FixedUpdate();
        public override abstract void LateFixedUpdate();

        protected void UpdateMoveSpeed(
            ref float speed,
            float input,
            float maxSpeed,
            float overSpeedDecelerationNoInput,
            float overSpeedDecelerationTurnAround,
            float overSpeedDeceleration,
            float decelerationNoInput,
            float decelerationTurnAround,
            float acceleration,
            float deltaTime
            )
        {
            float targetSpeed = input * maxSpeed;
            float moveAcceleration = GetMoveAcceleration(
                speed,
                targetSpeed,
                maxSpeed,
                overSpeedDecelerationNoInput,
                overSpeedDecelerationTurnAround,
                overSpeedDeceleration,
                decelerationNoInput,
                decelerationTurnAround,
                acceleration);
            speed = Mathf.MoveTowards(speed, targetSpeed, moveAcceleration * deltaTime);
        }

        private float GetMoveAcceleration(
            float speed,
            float targetSpeed,
            float maxSpeed,
            float overSpeedDecelerationNoInput,
            float overSpeedDecelerationTurnAround,
            float overSpeedDeceleration,
            float decelerationNoInput,
            float decelerationTurnAround,
            float acceleration)
        {
            bool TurnAround = (Mathf.Sign(speed) != 0 && Mathf.Sign(speed) != Mathf.Sign(targetSpeed));

            if (Mathf.Abs(speed) > maxSpeed)
            {
                if (targetSpeed == 0) return overSpeedDecelerationNoInput;
                return TurnAround ? overSpeedDecelerationTurnAround : overSpeedDeceleration;
            }

            if (targetSpeed == 0) return decelerationNoInput;
            if (TurnAround) return decelerationTurnAround;
            return acceleration;
        }

        protected void UpdateSimpleMoveSpeed(
            ref float speed,
            float input,
            float maxSpeed,
            float deceleration,
            float decelerationNoInput,
            float decelerationTurnAround,
            float acceleration,
            float deltaTime
            )
        {
            float targetSpeed = input * maxSpeed;
            float moveAcceleration = GetSimpleMoveAcceleration(
                speed,
                targetSpeed,
                maxSpeed,
                deceleration,
                decelerationNoInput,
                decelerationTurnAround,
                acceleration);
            speed = Mathf.MoveTowards(speed, targetSpeed, moveAcceleration * deltaTime);
        }

        private float GetSimpleMoveAcceleration(
            float speed,
            float targetSpeed,
            float maxSpeed,
            float deceleration,
            float decelerationNoInput,
            float decelerationTurnAround,
            float acceleration)
                {

                    if (targetSpeed != 0 && Mathf.Sign(speed) != Mathf.Sign(targetSpeed)) return decelerationTurnAround;
                    if (targetSpeed == 0) return decelerationNoInput;
                    if (Mathf.Abs(targetSpeed) < Mathf.Abs(speed)) return deceleration;

                    return acceleration;
                }

        public float ApplyGravityWithThreshold(
            float currentSpeed,
            float deltaTime,
            float gravityBeforeThreshold,
            float gravityAfterThreshold,
            float thresholdVelocity)
        {
            float nextSpeed = currentSpeed - gravityBeforeThreshold * deltaTime;

            // If we haven't crossed the threshold yet, return normal gravity result
            bool thresholdNotCrossed =
                (gravityBeforeThreshold > 0 && nextSpeed >= thresholdVelocity) ||
                (gravityBeforeThreshold < 0 && nextSpeed <= thresholdVelocity);

            if (thresholdNotCrossed)
                return nextSpeed;

            // Otherwise, split time: first falling to threshold, then apply new gravity
            float timeToThreshold = (currentSpeed - thresholdVelocity) / gravityBeforeThreshold;
            float remainingTime = deltaTime - timeToThreshold;

            return thresholdVelocity - gravityAfterThreshold * remainingTime;
        }

        public float ApplyGravity(float currentSpeed, float deltaTime, bool down = false)
        {
            // Falling normally after upward momentum is gone
            
            if (currentSpeed <= 0f)
            {
                float maxSpeed = down ? ctx.Stats.MaxFallSpeedDown : ctx.Stats.MaxFallSpeed;
                return Mathf.MoveTowards(currentSpeed, -maxSpeed, ctx.Stats.Gravity * deltaTime);
            }
                

            // above "float threshold"
            if (currentSpeed > ctx.Stats.JumpPhaseEndVelocity)
                return ApplyGravityWithThreshold(currentSpeed, deltaTime,
                                                ctx.Stats.JumpGravity,
                                                ctx.Stats.FloatGravity,
                                                ctx.Stats.JumpPhaseEndVelocity);

            // Past float threshold but still above 0 -> gradually apply back to default gravity
            return ApplyGravityWithThreshold(currentSpeed, deltaTime,
                                            ctx.Stats.FloatGravity,
                                            ctx.Stats.Gravity,
                                            0f);
        }


        protected void HandleCollision()
        {
            if (ctx.CollisionHandler.hitWall) ctx.speed.x = 0;
            if (ctx.CollisionHandler.hitCeiling) ctx.speed.y = Mathf.Min(0, ctx.speed.y);
            if (ctx.CollisionHandler.grounded)
            {
                ctx.speed.y = Mathf.Max(0, ctx.speed.y);
                ctx.grounded.Set(true);
                ctx.canDash = true;
                ctx.canEngage = true;
            }
            else
            {
                ctx.grounded.Set(false);
            }
        }

        protected void checkRun()
        {
            if (Mathf.Abs(inputHandler.MoveX) > 0.1)
            {
                controller.QueueMovementState(PlayerMovementStateType.Running);
            }
        }

        protected void checkIdle()
        {
            if (Mathf.Abs(inputHandler.MoveX) < 0.1 && Mathf.Abs(ctx.speed.x) < 0.1)
            {
                controller.QueueMovementState(PlayerMovementStateType.Idle);
            }
        }

        protected void checkJump()
        {
            if (inputHandler.JumpPressed.IsActive && ctx.grounded.IsActive && ctx.dashBuffer.IsActive
                && ctx.dashAim.y <= 0 && Mathf.Abs(ctx.dashAim.x) > 0.1 && Mathf.Abs(ctx.speed.x) > 1e-3f)
            {
                controller.QueueMovementState(PlayerMovementStateType.Wavedashing);
                return;
            }
            if (inputHandler.JumpPressed.IsActive && ctx.grounded.IsActive)
            {
                controller.QueueMovementState(PlayerMovementStateType.Jumping);
            }
        }

        protected void checkWavedash()
        {
            if (inputHandler.JumpPressed.IsActive && ctx.grounded.IsActive
                && ctx.dashAim.y <= 0 && Mathf.Abs(ctx.dashAim.x) > 0.1 && Mathf.Abs(ctx.speed.x) > 1e-3f )
            {
                controller.QueueMovementState(PlayerMovementStateType.Wavedashing);
            }
        }

        protected void checkDash()
        {
            if (inputHandler.DashPressed.IsActive && ctx.canDash && ctx.dashCoolDownTimer <= 0)
            {
                controller.QueueMovementState(PlayerMovementStateType.Dashing);
            }
        }

        protected void returnToDefaultState()
        {
            if (ctx.grounded.IsTrue)
                returnToDefaultGroundState();
            else
                controller.QueueMovementState(PlayerMovementStateType.Falling);
        }

        protected void returnToDefaultGroundState()
        {
            if (Mathf.Abs(inputHandler.MoveX) < 0.1 && Mathf.Abs(ctx.speed.x) < 0.1)
            {
                
                controller.QueueMovementState(PlayerMovementStateType.Idle);
            }
            else
            {
                controller.QueueMovementState(PlayerMovementStateType.Running);
            }
        }

        protected void checkKnockBack()
        {
            
            if (controller.HitTarget && ctx.canKnockBack)
            {
                controller.QueueMovementState(PlayerMovementStateType.KnockBack);
            }
        }

        protected void checkPogo()
        {
            if (controller.HitTarget)
            {
                if (ctx.canKnockBack && ctx.attackAim.y <= 0 && !ctx.grounded.IsActive)
                {
                    controller.QueueMovementState(PlayerMovementStateType.Pogoing);
                }
                else
                    checkKnockBack();
            }
        }

        protected void checkFall()
        {
            if (!ctx.CollisionHandler.grounded)
            {
                controller.QueueMovementState(PlayerMovementStateType.Falling);
            }
        }

    }
}