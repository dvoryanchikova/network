using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class PlayerMove : NetworkBehaviour {

	public GameObject bulletPrefab;
	public GameObject shadow;
	public bool _youAreNewHost = false;
	PeerInfoMessage _info = new PeerInfoMessage();

	// Update is called once per frame
	void Update () {

		if (!isLocalPlayer) {
			//GetComponentInChildren<SpriteRenderer> ().color = Color.white;

			//GetComponent<MeshRenderer>().material.color = Color.green;
			return;
		}

		var x = Input.GetAxis ("Horizontal") * 0.1f;
		var y = Input.GetAxis ("Vertical") * 0.1f;

		transform.Translate (new Vector3 (x, y, 0));

		if(Input.GetKeyDown(KeyCode.Space))
		{
			CmdFire ();
		}
	}

	public override void OnStartLocalPlayer()
	{
		shadow.GetComponent<SpriteRenderer> ().enabled = true;



		if (_info.isHost == true) {
			_youAreNewHost = false;
		} else
			_youAreNewHost = true;

			//GetComponentInChildren<SpriteRenderer> ().color = Color.white;
		//GetComponent<MeshRenderer>().material.color = Color.red;
	}



	[Command]
	void CmdFire()
	{
		var bullet = (GameObject)Instantiate (
			             bulletPrefab, 
			transform.position - transform.up,
			             Quaternion.identity);

		bullet.GetComponent<Rigidbody> ().velocity = -transform.up * 4f;

		NetworkServer.Spawn (bullet);
		Destroy (bullet, 2f);
	}
}
