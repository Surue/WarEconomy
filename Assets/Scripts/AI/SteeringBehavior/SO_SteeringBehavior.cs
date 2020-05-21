using System.Collections.Generic;
using UnityEngine;

public abstract class SO_SteeringBehavior : ScriptableObject
{
    public abstract Vector2 CalculateMove(Agent agent, List<Transform> neighbors);
}
