using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawn : NetworkBehaviour {

	public GameObject enemyPrefab;
	public int numEnemies;

	public override void OnStartServer()
	{
		

		
		for(int i =0; i < numEnemies; i++ )
		{
			var pos = new Vector3(
				Random.Range(-5.0f, 5.0f),
				Random.Range(-4.0f, 4.0f),
				0f
			);

			//var rotation = Quaternion.Euler( Random.Range(0,180), Random.Range(0,180), Random.Range(0,180));


			var enemy = (GameObject)Instantiate (enemyPrefab, pos, Quaternion.identity);
			NetworkServer.Spawn (enemy);
		}
	}
}
