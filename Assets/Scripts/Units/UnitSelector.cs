﻿using System;
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

    public void Select() {
        
    }

    public void UnSelect() {
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
            Vector3 targetPosition = hit.point;
            
            targetPosition.y += 0.1f;
            unitMovement_.SetTargetPosition(targetPosition);
        }
        
    }

    void OnDestroy() {
        UnitSelectorController unitSelectorController = FindObjectOfType<UnitSelectorController>();

        if (unitSelectorController != null) {
            unitSelectorController.Unregister(this);
        }
    }

    void OnDrawGizmos() {
        
    }
}
