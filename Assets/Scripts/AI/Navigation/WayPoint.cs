using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace AI {

[Serializable]
public struct Link {
    public WayPoint wayPoint;
    public float weight;
    public float distance;
}
public class WayPoint : MonoBehaviour {

    public List<Link> neighbors;
    
    // Start is called before the first frame update
    void Start() {
        PathFinder.Instance.RegisterWayPoint(this);
    }

    void OnDestroy() {
        PathFinder.Instance.UnregisterWayPoint(this);
    }

    void OnValidate() {
        for (int i = 0; i < neighbors.Count; i++) {
            Link neighbor = neighbors[i];
            neighbor.distance = (Vector3.Distance(transform.position, neighbor.wayPoint.transform.position));
            
            neighbors[i] = neighbor;
        }
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

        if (transform.hasChanged) {
            OnValidate();
        }
        
        if (neighbors == null) return;
        Handles.color = new Color(0.0f, 1.0f, 1.0f, 1f);
        Vector3 position = transform.position;
        
        foreach (Link neighbor in neighbors) {
            Vector3 neighborPos = neighbor.wayPoint.transform.position;
            Handles.Label((position + neighborPos) / 2.0f, neighbor.weight + " + " + neighbor.distance.ToString("0.00"));
            Vector3 dir = (position - neighborPos).normalized;
            
            Handles.DrawLine(position + Vector3.Cross(dir, Vector3.up) * 0.1f, neighborPos + Vector3.Cross(dir, Vector3.up) * 0.1f);
        }
    }
#endif
}
}
