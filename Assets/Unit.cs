using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    //Visual
    [Header("Visual")] 
    [SerializeField] Color spriteColor_;
    
    SpriteRenderer spriteRenderer_;
    
    //Movement
    [Header("Movements")] 
    [SerializeField] float speedMovements_;
    
    Vector3 targetPosition_;
    
    // Start is called before the first frame update
    void Start() {
        spriteRenderer_ = GetComponent<SpriteRenderer>();

        spriteRenderer_.color = spriteColor_;

        targetPosition_ = transform.position;
    }

    // Update is called once per frame
    void Update()
    { 
        var position = transform.position;
        if (Vector3.Distance(position, targetPosition_) > 0.5f) {
            position += speedMovements_ * Time.deltaTime * (targetPosition_ - position).normalized;
            transform.position = position;
        }
    }

    public void SetTargetPosition(Vector3 targetPosition) {
        targetPosition_ = targetPosition;
//        Debug.Log("Target position = " + targetPosition_);
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireSphere(targetPosition_, 1.0f);
    }
}
