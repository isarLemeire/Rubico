using UnityEngine;

namespace PlayerController
{
    [CreateAssetMenu]
    public class PlayerScriptableStats : ScriptableObject
    {

        [Header("INPUT")]
        [Tooltip("Minimum input required before it is recognized. Avoids drifting with sticky controllers")]
        public float HorizontalDeadZoneThreshold = 0.3f;

        [Tooltip("Amount of time a the Player can be too early or too late with their input")]
        public float ForgivenessTime = 0.1f;

        [Header("PLAYER")]
        public float PlayerWidth = 0.8f;
        public float PlayerHeight = 1.8f;

        [Header("COLLISION")]
        [Tooltip("Maximum distance to edge of ceiling, that allows a ceiling scoot")]
        public float CeilingLedgeCorrection = 0.5f;


        [Tooltip("Maximum distance to edge of wall, that allows a wall scoot")]
        public float WallLedgeCorrection = 0.6f;

        [Tooltip("Maximum distance to edge of wall, that allows a wall scoot")]
        public float FloorLedgeCorrection = 0.6f;

        [Tooltip("Maximum player velocity, that allows a wall scoot")]
        public float WallAssistVerticalSpeedThreshold = 0.01f;

        [Tooltip("Speed at which the player scoots")]
        public float ScootSpeed = 10f;

        [Header("GROUND MOVEMENT")]
        [Tooltip("The horizontal movement speed")]
        public float MaxMovementSpeed = 8f;

        [Tooltip("The player's horizontal acceleration on the ground")]
        public float GroundAccelerationTime = 0.1f;

        [Tooltip("The player's horizontal deceleration on the ground")]
        public float GroundDecelerationTime = 0.05f;

        [Tooltip("The player's horizontal deceleration when moving the oposite to the current movement on the ground")]
        public float GroundTurnAroundTime = 0.05f;

        public float GroundAcceleration
        {
            get { return MaxMovementSpeed / GroundAccelerationTime; }
        }

        public float GroundDeceleration
        {
            get { return MaxMovementSpeed / GroundDecelerationTime; }
        }

        public float GroundTurnAroundAcceleration
        {
            get { return MaxMovementSpeed / GroundTurnAroundTime; }
        }

        [Header("AIR MOVEMENT")]
        [Tooltip("The player's horizontal acceleration in the air")]
        public float AirAccelerationTime = 0.2f;

        [Tooltip("The player's horizontal deceleration in the air")]
        public float AirDecelerationTime = 0.1f;

        [Tooltip("The player's horizontal deceleration when moving the oposite to the current movement in the air")]
        public float AirTurnAroundTime = 0.1f;

        public float AirAcceleration
        {
            get { return MaxMovementSpeed / AirAccelerationTime; }
        }

        public float AirDeceleration
        {
            get { return MaxMovementSpeed / AirDecelerationTime; }
        }

        public float AirTurnAroundAcceleration
        {
            get { return MaxMovementSpeed / AirTurnAroundTime; }
        }

        [Header("JUMPING")]
        [Tooltip("The Players maximum jumping height")]
        public float MaxJumpHeight = 3.5f;

        [Tooltip("The Players downwards acceleration")]
        public float TimeToMaxFallSpeed = 0.375f;

        [Tooltip("The Players maximum downwards velocity")]
        public float MaxFallSpeed = 28;

        [Tooltip("The Players maximum downwards velocity when holding down")]
        public float MaxFallSpeedDown = 28;

        [Tooltip("The time before the Player reaches maximum jumping")]
        public float JumpTime = 0.2f;

        [Tooltip("The time before the Player reaches maximum jumping")]
        public float FloatHeight = 0.3f;

        [Tooltip("The time before the Player reaches maximum jumping")]
        public float FloatTime = 0.2f;

        [Tooltip("The time before the Player reaches maximum jumping")]
        public float EarlyReleaseHeight = 0.3f;

        [Tooltip("The time before the Player reaches maximum jumping")]
        public float EarlyReleaseFloatTime = 0.075f;


        public float Gravity
        {
            get { return MaxFallSpeed / TimeToMaxFallSpeed; }
        }

        public float JumpPhaseEndVelocity
        {
            get
            {
                return 2f * FloatHeight / FloatTime;
            }
        }

        public float FloatGravity
        {
            get
            {
                return JumpPhaseEndVelocity / FloatTime;
            }
        }

        public float EarlyReleaseEndVelocity
        {
            get
            {
                return 2f * EarlyReleaseHeight / FloatTime;
            }
        }

        public float EarlyReleaseFloatGravity
        {
            get
            {
                return EarlyReleaseEndVelocity / EarlyReleaseFloatTime;
            }
        }

        public float JumpGravity
        {
            get
            {
                float h = MaxJumpHeight - FloatHeight;
                return 2f * (h - JumpPhaseEndVelocity * JumpTime) / (JumpTime * JumpTime);
            }
        }

        public float JumpSpeed
        {
            get
            {
                return JumpPhaseEndVelocity + JumpGravity * JumpTime;
            }
        }

        [Header("DASHING")]
        [Tooltip("The Players dash upwards speed")]
        public float DashDistanceX = 3f;

        [Tooltip("The Players dash sideways speed")]
        public float DashDistanceUp = 2.75f;

        [Tooltip("The Players dash downwards speed")]
        public float DashDistanceDown = 3.5f;

        [Tooltip("The Players dash time")]
        public float DashTime = 0.15f;

        [Tooltip("The Players cooldown between dashed")]
        public float DashCooldown = 0.3f;

        public float DashSpeedX
        {
            get
            {
                return DashDistanceX / DashTime;
            }
        }

        public float DashSpeedUp
        {
            get
            {
                return DashDistanceUp / DashTime;
            }
        }

        public float DashSpeedDown
        {
            get
            {
                return DashDistanceDown / DashTime;
            }
        }



        [Header("DASHING DECELERATIONS")]
        [Tooltip("The time between the end of a dash and being back at normal speed, while on the ground")]
        public float DashDecelerationTime = 0.1f;

        [Tooltip("The time between the end of a dash and being back at normal speed, while in the air")]
        public float DashAirDecelerationTime = 0.3f;

        [Tooltip("The time between the end of a dash and being back at normal speed when no input is given, while on the ground")]
        public float DashDecelerationTimeNoInput = 0.3f;

        [Tooltip("The time between the end of a dash and being back at normal speed when no input is given, while in the air")]
        public float DashAirDecelerationTimeNoInput = 0.3f;

        [Tooltip("The time between the end of a dash and being back at normal speed, when steering the oposite direction, while on the ground")]
        public float DashTurnAroundTime = 0.05f;

        [Tooltip("The time between the end of a dash and being back at normal speed, when steering the oposite direction, while in the air")]
        public float DashAirTurnAroundTime = 0.2f;


        public float DashDeceleration
        {
            get 
            { 
                return (DashSpeedX - MaxMovementSpeed) / DashDecelerationTime;
            }
        }

        public float DashAirDeceleration
        {
            get
            {
                return (DashSpeedX - MaxMovementSpeed) / DashAirDecelerationTime;
            }
        }

        public float DashDecelerationNoInput
        {
            get
            {
                return (DashSpeedX - MaxMovementSpeed) / DashDecelerationTimeNoInput;
            }
        }

        public float DashAirDecelerationNoInput
        {
            get
            {
                return (DashSpeedX - MaxMovementSpeed) / DashAirDecelerationTimeNoInput;
            }
        }

        public float DashTurnAround
        {
            get 
            { 
                return (DashSpeedX - MaxMovementSpeed) / DashTurnAroundTime; 
            }
        }

        public float DashAirTurnAround
        {
            get
            {
                return (DashSpeedX - MaxMovementSpeed) / DashAirTurnAroundTime;
            }
        }

        [Header("WAVEDASH")]
        public float WavedashSpeedFactor = 1f;
        public float WavedashDecelerationTime = 0.1f;
        public float WavedashNoInputDecelerationTime = 0.6f;
        public float WavedashTurnAroundTime = 0.4f;


        public float WavedashSpeed
        {
            get
            {
                return DashSpeedX * WavedashSpeedFactor;
            }
        }

        public float WavedashDeceleration
        {
            get
            {
                return (DashSpeedX) / WavedashDecelerationTime;
            }
        }

        public float WavedashNoInputDeceleration
        {
            get
            {
                return (DashSpeedX) / WavedashNoInputDecelerationTime;
            }
        }

        public float WavedashTurnAroundDeceleration
        {
            get
            {
                return (DashSpeedX) / WavedashTurnAroundTime;
            }
        }




        [Header("ATTACKING")]
        public float AttackWidth = 3f;
        public float AttackHeight = 1f;

        public float AttackStartupTime = 0.1f;
        public float AttackActiveTime = 0.2f;
        public float AttackRecoveryTime = 0.2f;
        public float AttackAnimationTime = 0.2f;

        public float AttackDamage = 1f;
        public float AttackCoolDown = 1f;

        [Header("ENGAGING")]

        public float engageDistanceX = 2f;
        public float engageDistanceDU = 2f;
        public float engageDistanceUp = 2f;

        public float engageStopDistance = 0.5f;

        public float engageAirDistanceX = 2f;
        public float engageAirDistanceDU = 2f;
        public float engageAirDistanceUp = 2f;
        public float engageAirDistanceDown = 2f;
        public float engageAirDistanceDD = 2f;

        public float engageSpeedX
        {
            get
            {
                return engageDistanceX / AttackStartupTime;
            }
        }

        public float engageSpeedDU
        {
            get
            {
                return engageDistanceDU / AttackStartupTime;
            }
        }

        public float engageSpeedUp
        {
            get
            {
                return engageDistanceUp / AttackStartupTime;
            }
        }

        public float engageAirSpeedX
        {
            get
            {
                return engageAirDistanceX / AttackStartupTime;
            }
        }

        public float engageAirSpeedUp
        {
            get
            {
                return engageAirDistanceUp / AttackStartupTime;
            }
        }

        public float engageAirSpeedDU
        {
            get
            {
                return engageAirDistanceDU / AttackStartupTime;
            }
        }

        public float engageAirSpeedDown
        {
            get
            {
                return engageAirDistanceDown / AttackStartupTime;
            }
        }

        public float engageAirSpeedDD
        {
            get
            {
                return engageAirDistanceDD / AttackStartupTime;
            }
        }

        [Header("KNOCKBACK")]

        public float KnockBackTime = 0.1f;
        public float KnockBackDistance = 0.1f;

        public float KnockBackSpeed
        {
            get
            {
                return 2f * KnockBackDistance / KnockBackTime;
            }
        }

        public float KnockNoInputDecelerationTime = 0.1f;
        public float KnockTurnAroundTime = 0.1f;

        public float KnockNoInputDeceleration
        {
            get
            {
                return KnockBackSpeed / KnockNoInputDecelerationTime;
            }
        }

        public float KnockTurnAroundDeceleration
        {
            get
            {
                return KnockBackSpeed / KnockTurnAroundTime;
            }
        }

        public float KnockAirNoInputDecelerationTime = 0.1f;
        public float KnockAirTurnAroundTime = 0.1f;

        public float KnockAirNoInputDeceleration
        {
            get
            {
                return KnockBackSpeed / KnockAirNoInputDecelerationTime;
            }
        }

        public float KnockAirTurnAroundDeceleration
        {
            get
            {
                return KnockBackSpeed / KnockAirTurnAroundTime;
            }
        }


        [Header("POGO")]
        public float PogoHeight = 5f;

        public float DiagonalPogoHeight = 5f;

        public float HorizonatalPogoHeight = 5f;

        public float PogoSpeed
        {
            get
            {
                return Mathf.Sqrt(2f * (PogoHeight - FloatHeight) * JumpGravity);
            }
        }

        public float DiagonalPogoSpeed
        {
            get
            {
                return Mathf.Sqrt(2f * (DiagonalPogoHeight - FloatHeight) * JumpGravity);
            }
        }

        public float HorizonalPogoSpeed
        {
            get
            {
                return Mathf.Sqrt(2f * (HorizonatalPogoHeight - FloatHeight) * JumpGravity);
            }
        }

        public float PogoDecelerationTime = 0.5f;
        public float PogoTurnAroundDecelerationTime = 0.5f;

        public float PogoDeceleration
        {
            get
            {
                return MaxMovementSpeed / PogoDecelerationTime;
            }
        }

        public float PogoTurnAroundDeceleration
        {
            get
            {
                return MaxMovementSpeed / PogoTurnAroundDecelerationTime;
            }
        }

        [Header("DAMAGE")]
        public float DamagedTime = 1;
        public float DamageKnockbackDistanceX = 1;

        public float DamageKnockbackSpeedX = 20f;

        public float DamageKnockbackSpeedY = 27f;

        public float DamageDecelerationTime = 0.5f;
        public float DamageTurnAroundDecelerationTime = 0.5f;

        public float DamageDeceleration
        {
            get
            {
                return DamageKnockbackSpeedX / DamageDecelerationTime;
            }
        }

        public float DamageTurnAroundDeceleration
        {
            get
            {
                return DamageKnockbackSpeedX / DamageTurnAroundDecelerationTime;
            }
        }

        [Header("Invulnerable")]
        public float FlashFrequency = 4f;
        public float InvulnerableTime = 1f;
        public float InvulnerableAirTime = 2f;
        public float HitFreeze = 0.1f;
        public float HitShakeIntensity = 0.1f;
        public float HitShakeDuration = 0.1f;

        public float InvulnerableFreeze = 0.1f;
        public float InvulnerableIntensity = 0.1f;
        public float InvulnerableDuration = 0.1f;

        public int MaxHP = 2;
        public int MaxHeal = 4;

        [Header("RESPAWN")]
        public float DeathTime = 0.5f;
        public float DeathDeceleration = 10f;
        public float RespawnTime = 0.5f;

        [Header("JUICE")]
        public float AttackFreeze = 0.1f;
        public float AttackShakeIntensity = 0.1f;
        public float AttackShakeDuration = 0.1f;

        public float AttackFreezeNonTarget = 0.1f;
        public float AttackShakeIntensityNonTarget = 0.1f;
        public float AttackShakeDurationNonTarget = 0.1f;

        public float DashFreeze = 0.1f;
        public float DashShakeIntensity = 0.1f;
        public float DashShakeDuration = 0.1f;
    }
}
