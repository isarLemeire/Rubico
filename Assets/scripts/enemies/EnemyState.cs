using UnityEngine;


public abstract class EnemyState : State
{
    protected IStateRequester requester;
    new protected EnemyContext ctx => (EnemyContext)base.ctx;

    protected EnemyState(EnemyContext ctx, IStateRequester requester) : base(ctx)
    {
        this.requester = requester;
    }

}
