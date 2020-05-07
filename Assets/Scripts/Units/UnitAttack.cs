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
    [SerializeField] ParticleSystem particleSystem_;

    List<Destructible> possibleTarget_;
    Destructible targets_;

    const float ATTACK_TICK_TIME = 1;
    float time_ = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(attackRange_, attackRange_, attackRange_);
        
        particleSystem_.Stop();
        
        possibleTarget_ = new List<Destructible>();
    }

    // Update is called once per frame
    void Update() {
        time_ += Time.deltaTime;
        
        if (targets_ == null) {
            if (possibleTarget_.Count > 0) {
                targets_ = possibleTarget_[0];

                particleSystem_.Play();
            } else {
                return;
            }
        }
        
        //Animation of the particle system
        particleSystem_.transform.forward = targets_.transform.position - transform.position;
        
        if (time_ < ATTACK_TICK_TIME) return;
        
        //Reset time
        time_ = 0;
        
        //Attack all targets
        if (targets_.TakeDamage((int) (manpower_ * 0.1f))) {
            particleSystem_.Stop();
        
            possibleTarget_.Remove(targets_);
            targets_ = null;
            
            Destroy(targets_);
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
            //TOODO Use priority to select target
            possibleTarget_.Add(other.GetComponent<Destructible>());
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Destructible")) {

            Destructible destructible = other.GetComponent<Destructible>();
            
            if (possibleTarget_.Contains(destructible)) {

                if (targets_ == destructible) {
                    targets_ = null;
                    particleSystem_.Stop();
                }
                
                possibleTarget_.Remove(destructible);
            }
        }
    }
}
