using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Behavior/Blending")]
public class SO_BlendingBehavior : SO_SteeringBehavior
{
    [SerializeField] List<SO_SteeringBehavior> behaviors_;
    [SerializeField] List<float> weights_;
    
    public override Vector2 CalculateMove(Agent agent, List<Transform> neighbors) {

        Vector2 move = Vector2.zero;
        
        for(int i = 0; i < behaviors_.Count; i++) {
            Vector2 partialMove = behaviors_[i].CalculateMove(agent, neighbors);

            if (partialMove.sqrMagnitude > weights_[i] * weights_[i]) {
                partialMove = partialMove.normalized * weights_[i];
            }

            move += partialMove;
        }

        return move;
    }
}
