using UnityEngine;
using System.Collections.Generic;

public interface IStatePriority<TState>
{
    public static readonly Dictionary<TState, int> priority;
}