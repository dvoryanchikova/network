using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Combat : NetworkBehaviour {

	public const int maxHealth = 100;

	public bool destroyOnDeath;

	private Vector3 startPos;
	void Start()
	{
		startPos = transform.position;
	}

	[SyncVar]
	public int health = maxHealth;

	public void TakeDamage(int amount)
	{
		if (!isServer)
			return;

		if(destroyOnDeath){
			Destroy (gameObject);
		}

		health -= amount;
		if(health <= 0) {
			if (gameObject.tag == "Enemy")
				Destroy (gameObject);
			health = maxHealth;
			RpcRespawn ();
		}
	}

	[ClientRpc]
	void RpcRespawn()
	{
		if (isLocalPlayer) {
			transform.position = startPos;
		}
	}
}
