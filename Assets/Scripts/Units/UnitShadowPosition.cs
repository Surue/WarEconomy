using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitShadowPosition : MonoBehaviour {
    [Header("Visual")] 
    
    [SerializeField] SpriteRenderer positionSprite_;
    [SerializeField] Color positionColor_;
    [SerializeField] SpriteRenderer attackAreaSprite_;
    [SerializeField] Color attackAreaColor_;
    [SerializeField] float fadingDistance_;
    [SerializeField] float disableDistance_;
    
    Vector3 targetPosition_;
    
    // Start is called before the first frame update
    void Start() {
        positionSprite_.color = positionColor_;
        attackAreaSprite_.color = attackAreaColor_;
        
        positionSprite_.gameObject.SetActive(false);
    }

    public void ActivateAt(Vector3 targetPosition) {
        positionSprite_.gameObject.SetActive(true);
        
        positionSprite_.transform.position = targetPosition;
        attackAreaSprite_.transform.position = targetPosition;
        
        positionSprite_.color = positionColor_;
        attackAreaSprite_.color = attackAreaColor_;

        targetPosition_ = targetPosition;
    }

    public void UpdatePosition(Vector3 currentPosition) {
        transform.position = targetPosition_;
        
        float distance = Vector3.Distance(targetPosition_, currentPosition);

        if (distance <= fadingDistance_) {
            Color newPositionColor = positionColor_;
            newPositionColor.a = Mathf.Lerp(0, positionColor_.a, distance / fadingDistance_);
            positionSprite_.color = newPositionColor;
            
            Color newAttackAreaColor = attackAreaColor_;
            newAttackAreaColor.a = Mathf.Lerp(0, attackAreaColor_.a, distance / fadingDistance_);
            attackAreaSprite_.color = newAttackAreaColor;
        }

        if (distance <= disableDistance_) {
            positionSprite_.gameObject.SetActive(false);
        }
    }
}
