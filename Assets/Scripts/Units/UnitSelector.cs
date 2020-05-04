using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScreenAABB {
    public Vector2 bottomLeft;
    public Vector2 topRight;
}

public struct WorldAABB {
    public Vector3 bottomLeft;
    public Vector3 topRight;
}

public class UnitSelector : MonoBehaviour {
    BoxCollider collider_;
    Camera camera_;

    Unit unit_;

    // Start is called before the first frame update
    void Start() {
        camera_ = Camera.main;
        collider_ = GetComponent<BoxCollider>();

        FindObjectOfType<UnitSelectorController>().Register(this);

        unit_ = GetComponent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ScreenAABB GetScreenAABB() {
        ScreenAABB screenAabb;

        Bounds bounds = collider_.bounds;
        screenAabb.bottomLeft = camera_.WorldToScreenPoint(bounds.min);
        screenAabb.topRight = camera_.WorldToScreenPoint(bounds.max);

        return screenAabb;
    }
    
    public WorldAABB GetWorldAABB() {
        WorldAABB screenAabb;

        Bounds bounds = collider_.bounds;
        screenAabb.bottomLeft = bounds.min;
        screenAabb.topRight = bounds.max;

        return screenAabb;
    }

    public void SetTargetPositionFromMousePosition(Vector3 mouseScreenPosition) {
        
        RaycastHit hit;
        var ray = camera_.ScreenPointToRay(Input.mousePosition);
  
        Vector3 targetPosition = transform.position;
        if (Physics.Raycast(ray, out hit, 1000, ~LayerMask.NameToLayer("Ground"))) {
            targetPosition = hit.point;
            
            targetPosition.y += 0.1f;
        }
        
        unit_.SetTargetPosition(targetPosition);
    }

    void OnDrawGizmos() {
        
    }
}
