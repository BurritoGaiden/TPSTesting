using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateDestroyer : MonoBehaviour {

    public GameObject brokenCratePrefab;
    public bool hasExploded;
    public GameObject explosion;

    public float radius;
    public float force;
    public float explosionRadius;

	// Update is called once per frame
	public void ExplodeCrate() {
        Instantiate(brokenCratePrefab, transform.position, transform.rotation);
        Explode();
        Destroy(gameObject);
    }

    public void Explode() {
        if (explosion != null)
        {
            Instantiate(explosion, transform.position, transform.rotation);

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

            foreach (Collider nearbyCrates in colliders)
            {
                Rigidbody rb = nearbyCrates.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(force, transform.position, radius);
                }
            }
        }
    }
	
}
