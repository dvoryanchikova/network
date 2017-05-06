using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Networking {

	public class NewBehaviourScript : MonoBehaviour
	{

		NetworkMigrationManager netwr;

		public bool _newHost = GameObject.FindObjectOfType<PlayerMove>()._youAreNewHost;

		 void OnClientDisconnectedFromHost (NetworkConnection conn, out NetworkMigrationManager.SceneChangeOption sceneChange)
		{
			Debug.Log("Migration >> On Client Disconnected From Host");
			//OnClientDisconnectedFromHost
			OnClientDisconnectedFromHost(conn, out sceneChange);


			//netwr.PeerInfoMessage _info;
			//bool _newHost = false;


			//netwr.FindNewHost(out _info, out _youAreNewHost);

			if (_newHost == true )
			{
				netwr.BecomeNewHost(7777);
			}
			else
			{
				
			}
		}

	}

}