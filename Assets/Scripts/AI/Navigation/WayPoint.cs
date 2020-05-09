using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AI {

[Serializable]
public struct Link {
    public WayPoint wayPoint;
    public float weight;
}
public class WayPoint : MonoBehaviour {

    public List<Link> neighbors;
    
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    
#if UNITY_EDITOR
    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Handles.color = new Color(0.0f, 1.0f, 1.0f, 0.5f);
        
        Handles.DrawWireDisc(transform.position, Vector3.up, 0.5f);
        
        if (neighbors == null) return;
        Handles.color = new Color(0.0f, 1.0f, 1.0f, 0.75f);
        Vector3 position = transform.position;
        
        foreach (Link neighbor in neighbors) {
            Vector3 neighborPos = neighbor.wayPoint.transform.position;
            Vector3 dir = (position - neighborPos).normalized;
            
            Handles.DrawDottedLine(position + Vector3.Cross(dir, Vector3.up) * 0.1f, neighborPos + Vector3.Cross(dir, Vector3.up) * 0.1f, 4.0f);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Handles.color = new Color(0.0f, 1.0f, 1.0f, 1f);
        
        Handles.DrawSolidDisc(transform.position, Vector3.up, 0.5f);
        
        if (neighbors == null) return;
        Handles.color = new Color(0.0f, 1.0f, 1.0f, 1f);
        Vector3 position = transform.position;
        
        foreach (Link neighbor in neighbors) {
            Vector3 neighborPos = neighbor.wayPoint.transform.position;
            Handles.Label((position + neighborPos) / 2.0f, neighbor.weight.ToString());
            Vector3 dir = (position - neighborPos).normalized;
            
            Handles.DrawLine(position + Vector3.Cross(dir, Vector3.up) * 0.1f, neighborPos + Vector3.Cross(dir, Vector3.up) * 0.1f);
        }
    }
#endif
}
}
