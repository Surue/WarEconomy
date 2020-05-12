using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalker : MonoBehaviour {

    List<Vector3> path_;

    [SerializeField] float speed_ = 5.0f;
    [SerializeField] float stoppingDistance_ = 0.1f;
    Rigidbody body_;
    UnitMovement unitMovement_;
    
    // Start is called before the first frame update
    void Start() {
        body_ = GetComponent<Rigidbody>();
        unitMovement_ = GetComponent<UnitMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (path_ == null || path_.Count == 0) {
            path_ = PathFinder.Instance.GetPath(transform.position, new Vector3(Random.Range(-20, 20), 0, Random.Range(-10, -30)));
            
            unitMovement_.SetTargetPosition(path_[0]);
        } else {
            if (Vector3.Distance(transform.position, path_[0]) < stoppingDistance_) {
                path_.RemoveAt(0);

                if (path_.Count > 0) {
                    unitMovement_.SetTargetPosition(path_[0]);
                }
            } 
        }
    }

    void OnDrawGizmos() {
        if (path_ != null) {
            foreach (Vector3 vector3 in path_) {
                Gizmos.DrawCube(vector3, Vector3.one);
            }
        }
    }
}
