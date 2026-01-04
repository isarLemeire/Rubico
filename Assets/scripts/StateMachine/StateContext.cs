using UnityEngine;
using UnityEngine.InputSystem;

public class StateContext
{
    public Vector3 speed;
    public Vector2 position;
    public bool faceRight;

    public virtual void Update(float deltaTime)
    {

    }
}

