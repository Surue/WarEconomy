using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AI {

[Serializable]
public struct EditorLink {
     [SerializeField] public WayPoint wayPoint;
     public float weight;
     public float distance;
 }

[Serializable]
public struct Link {
    public int wayPointIndex;
    public float weight;
    public float distance;
}
public class WayPoint : MonoBehaviour {
    
    [SerializeField] List<EditorLink> neighbors_;
    [SerializeField] List<Link> links_;
    const int NULL_INDEX = -1;
    [SerializeField] int index = NULL_INDEX;

    public List<Link> Links => links_;

    public int Index => index;

    void Start() {
        ComputeLinks();
    }

    void OnValidate() {
        if(neighbors_ == null) return; 
        for (int i = 0; i < neighbors_.Count; i++) {
            EditorLink neighbor = neighbors_[i];
            neighbor.distance = (Vector3.Distance(transform.position, neighbor.wayPoint.transform.position));

            neighbors_[i] = neighbor;

            if (!neighbor.wayPoint.IsNeighbor(this)) {
                neighbor.wayPoint.AddNeighbor(this, neighbor.weight, neighbor.distance);
            }
        }
    }

    public bool IsNeighbor(WayPoint wayPoint) {
        if (neighbors_ == null) {
            neighbors_ = new List<EditorLink>();
        }
        foreach (EditorLink editorLink in neighbors_) {
            if (editorLink.wayPoint == wayPoint) {
                return true;
            }
        }

        return false;
    }

    public void AddNeighbor(WayPoint wayPoint, float weight, float distance) {
        neighbors_.Add((new EditorLink{wayPoint = wayPoint, weight =  weight, distance = distance}));
    }

    public void Reset(int newIndex, PathFinder pathFinder) {
        neighbors_?.RemoveAll(x => x.wayPoint == null);
        
        index = newIndex;
    }

    public void ComputeLinks() {
        links_ = new List<Link>(neighbors_.Count);
        
        for (int i = 0; i < neighbors_.Count; i++) {
            EditorLink neighbor = neighbors_[i];

            Link link;
            link.distance = neighbor.distance;
            link.weight = neighbor.weight;
            link.wayPointIndex = neighbor.wayPoint.Index;
            
            links_.Add(link);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Handles.color = new Color(0.0f, 1.0f, 1.0f, 0.5f);

        
        Handles.DrawWireDisc(transform.position, Vector3.up, 0.1f);
        
        if (neighbors_ == null) return;
        Handles.color = new Color(0.0f, 1.0f, 1.0f, 0.75f);
        Vector3 position = transform.position;
        
        foreach (EditorLink neighbor in neighbors_) {
            Vector3 neighborPos = neighbor.wayPoint.transform.position;
            Vector3 dir = (position - neighborPos).normalized;

            if (neighbor.weight == 1) {
                Handles.color = new Color(0.0f, 1.0f, 1.0f, 0.75f);
                Handles.DrawDottedLine(position + Vector3.Cross(dir, Vector3.up) * 0.05f, neighborPos + Vector3.Cross(dir, Vector3.up) * 0.05f, 4.0f);
            }else if (neighbor.weight == 2) {
                Handles.color = new Color(0.0f, 1.0f, 1.0f, 0.25f);
                Handles.DrawDottedLine(position + Vector3.Cross(dir, Vector3.up) * 0.01f, neighborPos + Vector3.Cross(dir, Vector3.up) * 0.01f, 1.0f);
            } else {
                Handles.DrawDottedLine(position + Vector3.Cross(dir, Vector3.up) * 0.1f, neighborPos + Vector3.Cross(dir, Vector3.up) * 0.1f, 4.0f);
            }
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
            
            if (neighbor.weight == 1) {
                Handles.color = new Color(0.0f, 1.0f, 1.0f, 0.75f);
                Handles.DrawLine(position + Vector3.Cross(dir, Vector3.up) * 0.05f, neighborPos + Vector3.Cross(dir, Vector3.up) * 0.05f);
            }else if (neighbor.weight == 2) {
                Handles.color = new Color(0.0f, 1.0f, 1.0f, 0.25f);
                Handles.DrawLine(position + Vector3.Cross(dir, Vector3.up) * 0.01f, neighborPos + Vector3.Cross(dir, Vector3.up) * 0.01f);
            } else {
                Handles.DrawLine(position + Vector3.Cross(dir, Vector3.up) * 0.1f, neighborPos + Vector3.Cross(dir, Vector3.up) * 0.1f);
            }
        }
    }
#endif
}
}
