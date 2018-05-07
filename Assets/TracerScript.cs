using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerScript : MonoBehaviour
{

    public float despawnTime;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(Despawn());
    }

    IEnumerator Despawn() {
        yield return new WaitForSeconds(despawnTime);
        Destroy(gameObject);
    }
}