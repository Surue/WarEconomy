using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitSelectorController : MonoBehaviour
{
    List<UnitSelector> selectables_;

    const int NULL_SELECTED = -1;
    int indexSelected_ = NULL_SELECTED;
    
    // Start is called before the first frame update
    void Awake()
    {
        selectables_ = new List<UnitSelector>();
    }

    // Update is called once per frame
    void Update()
    {
         //Select an unit
        if (Input.GetMouseButtonDown(0) && indexSelected_ == NULL_SELECTED) {
            Vector3 mousePos = Input.mousePosition;

            for (int index = 0; index < selectables_.Count; index++) {
                UnitSelector unitSelector = selectables_[index];
                ScreenAABB screenAabb = unitSelector.GetScreenAABB();

                if (!(mousePos.x > screenAabb.bottomLeft.x) || !(mousePos.x < screenAabb.topRight.x)) continue;
                if (mousePos.y > screenAabb.bottomLeft.y && mousePos.y < screenAabb.topRight.y) {
                    indexSelected_ = index;
                }
            }
        }

        //Move the unit
        if (Input.GetMouseButton(0) && indexSelected_ != NULL_SELECTED) {
            selectables_[indexSelected_].SetTargetPositionFromMousePosition(Input.mousePosition);
        }
        
        //Deselect the unit
        if (Input.GetMouseButtonUp(0) && indexSelected_ != NULL_SELECTED) {
            indexSelected_ = NULL_SELECTED;
        }
    }

    public void Register(UnitSelector selector) {
        selectables_.Add(selector);
    }

    public void Unregister(UnitSelector selector) {
        if (indexSelected_ != NULL_SELECTED && selectables_[indexSelected_] == selector) {
            //If the unit is currently selected, the the selection index is set to null
            indexSelected_ = NULL_SELECTED;
        } else {
            //Otherwise loop through each selectable object and if the destroyed object's index is small than the selected index, then reduce it by one
            for (int i = 0; i < selectables_.Count; i++) {
                if (selectables_[i] != selector) continue;
                if (indexSelected_ > i) {
                    indexSelected_--;
                }
                    
                break;
            }
        }
        
        selectables_.Remove(selector);
    }

    void OnDrawGizmos() {
        if (indexSelected_ != NULL_SELECTED) {
            UnitSelector unitSelector = selectables_[indexSelected_];
            WorldAABB worldAabb = unitSelector.GetWorldAABB();

            Vector3 extent = worldAabb.topRight - worldAabb.bottomLeft;
            
            Gizmos.DrawCube(worldAabb.topRight - (extent * 0.5f), extent);
        }
    }
}
