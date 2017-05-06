using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	void OnCollisionEnter(Collision coll)
	{
		var hit = coll.gameObject;
		var hitCombat = hit.GetComponent<Combat> ();
		if (hitCombat != null) {

			hitCombat.TakeDamage (10);
			Destroy (gameObject);
		}
	}
}
