using UnityEngine;


namespace PlayerController
{
    public class PlayerStateMachine
    {
        private PlayerStateBase current;
        private PlayerStateContext ctx;


        public PlayerStateMachine(PlayerStateContext ctx)
        {
            this.ctx = ctx;
        }


        public void Update()
        {
            current?.Update();
        }


        public void FixedUpdate()
        {
            current?.FixedUpdate();
        }

        public void LateFixedUpdate()
        {
            current?.LateFixedUpdate();
        }


        public void ChangeState(PlayerStateBase newState)
        {
            current?.Exit();
            current = newState;
            current?.Enter();
        }
    }
}