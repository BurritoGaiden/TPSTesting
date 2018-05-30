using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateDestroyer : MonoBehaviour {

    public GameObject brokenCratePrefab;

	// Update is called once per frame
	public void ExplodeCrate() {
        Instantiate(brokenCratePrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }
	
}
