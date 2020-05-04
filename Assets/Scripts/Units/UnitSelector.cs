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

    UnitMovement unitMovement_;

    // Start is called before the first frame update
    void Start() {
        camera_ = Camera.main;
        collider_ = GetComponent<BoxCollider>();

        FindObjectOfType<UnitSelectorController>().Register(this);

        unitMovement_ = GetComponent<UnitMovement>();
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
  
        if (Physics.Raycast(ray, out hit, 1000, 1 << LayerMask.NameToLayer("Ground"))) {
            Vector3 targetPosition = targetPosition = hit.point;
            
            targetPosition.y += 0.1f;
            unitMovement_.SetTargetPosition(targetPosition);
        }
        
    }

    void OnDestroy() {
        FindObjectOfType<UnitSelectorController>().Unregister(this);
    }

    void OnDrawGizmos() {
        
    }
}
