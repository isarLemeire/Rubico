using UnityEngine;
using System;

public class DeathState : EnemyState
{
    private readonly float explosionSpeed;
    private readonly float explosionSpread;
    private readonly float explosionLifetime;
    private readonly GameObject enemy;
    private readonly GameObject explosion;

    public DeathState(
        EnemyContext ctx,
        IStateRequester requester,
        GameObject explosion,
        float explosionSpeed,
        float explosionSpread,
        float explosionLifetime,
        GameObject enemy
    ) : base(ctx, requester)
    {
        this.explosionSpeed = explosionSpeed;
        this.explosionSpread = explosionSpread;
        this.explosionLifetime = explosionLifetime;
        this.explosion = explosion;
        this.enemy = enemy;
    }

    public override void Enter()
    {
        Vector2 hitDir = ctx.speed.normalized;

        /*
            Instantiate(explosion, ctx.position, Quaternion.identity)
            .GetComponent<EnemyExplosion>()
            .Explode(hitDir, explosionSpeed, explosionSpread, explosionLifetime);
        */

        enemy.SetActive(false);
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
    }

    public override void FixedUpdate()
    {
    }

    public override void LateFixedUpdate()
    {
    }
}