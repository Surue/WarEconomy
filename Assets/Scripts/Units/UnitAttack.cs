using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttack : Destructible {

    [SerializeField] float attackRange_;
    [SerializeField] int manpower_;

    List<Destructible> targets_;

    float attackTickTime = 1;
    float time = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        targets_ = new List<Destructible>();
    }

    // Update is called once per frame
    void Update() {
        time += Time.deltaTime;

        if (time < attackTickTime) return;
        
        //Reset time
        time = 0;
        
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
        
        return manpower_ <= 0;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Destructible")) {
            targets_.Add(other.GetComponent<Destructible>());
        }
    }
}
