using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHealth : MonoBehaviour {

    public static float health = 100f;

    // Use this for initialization
    void Start () {
        EnemyAPC.HitPlayer += TakeDamage;
	}

    void TakeDamage()
    {
        if (health > 0)
            health -= 8f;
        if (health <= 0)
        {
        }
    }
}
