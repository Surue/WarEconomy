using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttack : Destructible {

    [Header("Fighting values")]
    [SerializeField] float attackRange_;
    [SerializeField] int manpower_;

    [Header("Visual")]
    SpriteRenderer spriteRenderer_;
    
    List<Destructible> targets_;

    const float ATTACK_TICK_TIME = 1;
    float time_ = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        targets_ = new List<Destructible>();
        
        transform.localScale = new Vector3(attackRange_, attackRange_, attackRange_);
    }

    // Update is called once per frame
    void Update() {
        time_ += Time.deltaTime;

        if (time_ < ATTACK_TICK_TIME) return;
        
        //Reset time
        time_ = 0;
        
        //Attack all targets
        List<Destructible> targetToRemove = new List<Destructible>();
        foreach (Destructible destructible in targets_) {
            if (destructible.TakeDamage((int) (manpower_ * 0.1f))) {
                targetToRemove.Add(destructible);
            }
        }

        foreach (Destructible destructible in targetToRemove) {
            targets_.Remove(destructible);
        }
    }
    
    public override bool TakeDamage(int dmg) {
        manpower_ -= dmg;

        if (manpower_ > 0) return false;
        Destroy(transform.parent.gameObject, 0.1f);
        return true;

    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Destructible")) {
            targets_.Add(other.GetComponent<Destructible>());
        }
    }
}
