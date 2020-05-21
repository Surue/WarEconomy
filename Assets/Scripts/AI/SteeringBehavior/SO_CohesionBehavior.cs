using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Behavior/Cohesion")]
public class SO_CohesionBehavior : SO_SteeringBehavior
{
    Vector2 velocity_ = Vector2.one;
    
    public override Vector2 CalculateMove(Agent agent, List<Transform> neighbors) {

        Vector2 move = Vector2.zero;
        if (neighbors.Count == 0) {
            return move;
        }

        foreach (Transform transform in neighbors) {
            move += (Vector2)transform.position;
        }

        //Center point
        move /= neighbors.Count;

        Transform t = agent.transform;
        move -= (Vector2)t.position;

        move = Vector2.SmoothDamp(t.up, move, ref velocity_, 0.5f);

        return move;
    }
}
