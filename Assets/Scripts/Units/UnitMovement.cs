using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour {

    //Visual
    [Header("Visual")] 
    [SerializeField] Color spriteColor_;
    [SerializeField] UnitShadowPosition shadowPosition_;
    
    SpriteRenderer spriteRenderer_;
    
    //Movement
    [Header("Movements")] 
    [SerializeField] float speedMovements_;
    [SerializeField] float stoppingDistance_ = 0.1f;
    
    Vector3 targetPosition_;
    
    //Attack
    [Header("Attacks")]
    [SerializeField] SpriteRenderer spriteAttackArea_;

    enum State {
        MOVING,
        IDLE
    }

    State state_ = State.IDLE;
    
    // Start is called before the first frame update
    void Start() {
        spriteRenderer_ = GetComponent<SpriteRenderer>();

        spriteRenderer_.color = spriteColor_;
        spriteAttackArea_.color = spriteColor_;

        targetPosition_ = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var position = transform.position;
        switch (state_) {
            case State.MOVING:
                if (Vector3.Distance(position, targetPosition_) > stoppingDistance_) {
                    position += speedMovements_ * Time.deltaTime * (targetPosition_ - position).normalized;
                    transform.position = position;
                    
                    shadowPosition_.UpdatePosition(position);
                } else {
                    state_ = State.IDLE;
                }
                break;
            case State.IDLE:
                if (Vector3.Distance(position, targetPosition_) > stoppingDistance_) {
                    state_ = State.MOVING;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SetTargetPosition(Vector3 targetPosition) {
        targetPosition_ = targetPosition;
        
        shadowPosition_.ActivateAt(targetPosition_);
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireSphere(targetPosition_, 1.0f);
    }
}
