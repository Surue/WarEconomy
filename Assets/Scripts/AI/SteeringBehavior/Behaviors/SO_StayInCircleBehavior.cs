using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Behavior/StayInCircle")]
public class SO_StayInCircleBehavior : SO_SteeringBehavior {
    readonly Vector2 center_ = Vector2.zero;
    [SerializeField] float radius_;
    [SerializeField] float factorRadius_ = 0.8f;

    public override Vector2 CalculateMove(Agent agent, List<Transform> neighbors) {
        Vector2 centerOffset = center_ - (Vector2) agent.transform.position;

        float t = centerOffset.magnitude / radius_;

        Vector2 move = Vector2.zero;

        if (t > factorRadius_) {
            move = t * t * centerOffset;
        }

        return move;
    }
}