using UnityEngine;

[CreateAssetMenu]
public class JuiceStats : ScriptableObject
{
        [Header("SCREEN SHAKE")]
        public float ShakeFrequency = 25f; // samples/sec

        [Header("ATTACK")]
        public float AttackFreeze = 0.1f;
        public float AttackShakeIntensity = 0.1f;
        public float AttackShakeDuration = 0.1f;

        public float AttackFreezeNonTarget = 0.1f;
        public float AttackShakeIntensityNonTarget = 0.1f;
        public float AttackShakeDurationNonTarget = 0.1f;

        public float HitFlashDuration = 0.1f;

        [Header("DASH")]
        public float DashFreeze = 0.1f;
        public float DashShakeIntensity = 0.1f;
        public float DashShakeDuration = 0.1f;

        [Header("HIT")]
        public float FlashFrequency = 4f;
        public float HitFreeze = 0.1f;
        public float HitShakeIntensity = 0.1f;
        public float HitShakeDuration = 0.1f;

        public float InvulnerableFreeze = 0.1f;
        public float InvulnerableShakeIntensity = 0.1f;
        public float InvulnerableShakeDuration = 0.1f;
}
