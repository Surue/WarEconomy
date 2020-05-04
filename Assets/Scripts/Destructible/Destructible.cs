using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Destructible : MonoBehaviour {
    /// <summary>
    /// 
    /// </summary>
    /// <returns>true if the destructible is destroyed/dead</returns>
    public abstract bool TakeDamage(int dmg);
}
