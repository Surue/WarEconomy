using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VisualDebugger : MonoBehaviour {

    [SerializeField] GameObject objectA_;

    [SerializeField] GameObject objectB_;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.DrawLine(
            objectA_.transform.position,   // Position 1
            objectB_.transform.position,   // Position 2
            Color.red,                     // Color
            5                              // Time
        );
    }

    void Update()
    {
//        Debug.DrawLine(
//            objectA_.transform.position,   // Position 1
//            objectB_.transform.position,   // Position 2
//            Color.red                      // Color
//            );
    }

    void OnDrawGizmos() {
        Handles.color = Color.white;
        
        Vector3 posA = objectA_.transform.position;
        Vector3 posB = objectB_.transform.position;
        Handles.Label(
            (posA + posB) / 2.0f,                                           // Position
            "Distance = " + Vector3.Distance(posA, posB).ToString("0.00")   // Text
            );
        
        Handles.color = Color.red;
        Handles.DrawDottedLine(
            posA, // Position 1
            objectB_.transform.position, // Position 2
            1.5f                         // Pixel space between segment
            );
    }
}




