using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace AI {

[Serializable]
public struct EditorLink {
     public WayPoint wayPoint;
     public float weight;
     public float distance;
 }

public struct Link {
    public int wayPointIndex;
    public float weight;
    public float distance;
}
public class WayPoint : MonoBehaviour {
    
    [SerializeField] List<EditorLink> neighbors_;
    public Link[] links_;
    const int NULL_INDEX = -1;
    public int index = NULL_INDEX;

    void OnValidate() {
        for (int i = 0; i < neighbors_.Count; i++) {
            EditorLink neighbor = neighbors_[i];
            neighbor.distance = (Vector3.Distance(transform.position, neighbor.wayPoint.transform.position));

            neighbors_[i] = neighbor;

            if (!neighbor.wayPoint.IsNeighbor(this)) {
                neighbor.wayPoint.AddNeighbor(this, neighbor.weight, neighbor.distance);
            }
        }
    }

    bool IsNeighbor(WayPoint wayPoint) {
        foreach (EditorLink editorLink in neighbors_) {
            if (editorLink.wayPoint == wayPoint) {
                return true;
            }
        }

        return false;
    }

    void AddNeighbor(WayPoint wayPoint, float weight, float distance) {
        neighbors_.Add((new EditorLink{wayPoint = wayPoint, weight =  weight, distance = distance}));
    }

    public void Reset(int newIndex, PathFinder pathFinder) {
        index = newIndex;

        pathFinder.RegisterWayPoint(this);
    }

    public void ComputeLinks() {
        links_ = new Link[neighbors_.Count];
        
        for (int i = 0; i < neighbors_.Count; i++) {
            EditorLink neighbor = neighbors_[i];

            links_[i].distance = neighbor.distance;
            links_[i].weight = neighbor.weight;
            links_[i].wayPointIndex = neighbor.wayPoint.index;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Handles.color = new Color(0.0f, 1.0f, 1.0f, 0.5f);
        
        Handles.DrawWireDisc(transform.position, Vector3.up, 0.5f);
        
        if (neighbors_ == null) return;
        Handles.color = new Color(0.0f, 1.0f, 1.0f, 0.75f);
        Vector3 position = transform.position;
        
        foreach (EditorLink neighbor in neighbors_) {
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
        
        if (neighbors_ == null) return;
        Handles.color = new Color(0.0f, 1.0f, 1.0f, 1f);
        Vector3 position = transform.position;
        
        foreach (EditorLink neighbor in neighbors_) {
            Vector3 neighborPos = neighbor.wayPoint.transform.position;
            Handles.Label((position + neighborPos) / 2.0f, neighbor.weight + " + " + neighbor.distance.ToString("0.00"));
            Vector3 dir = (position - neighborPos).normalized;
            
            Handles.DrawLine(position + Vector3.Cross(dir, Vector3.up) * 0.1f, neighborPos + Vector3.Cross(dir, Vector3.up) * 0.1f);
        }
    }
#endif
}
}
