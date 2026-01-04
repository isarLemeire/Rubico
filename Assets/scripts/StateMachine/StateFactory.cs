using UnityEngine;
using System.Collections.Generic;


public interface IStateFactory<TState>
{
    State GetState(TState type);
}