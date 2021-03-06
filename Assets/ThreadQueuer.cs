﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using UnityEngine;


public class ThreadQueuer : MonoBehaviour {

	List<Action> functionsToRunInMainThread;

 	void Start () {
		Debug.Log("Start() -- Started.");

		functionsToRunInMainThread = new List<Action>();
		StartThreadedFunction( () => { SlowFunctionThatDoesAUnityThing( Vector3.zero, new float[4], new Color[100] ); } );


		Debug.Log("Start() -- Done.");

	}
	
 	void Update () {
		while(functionsToRunInMainThread.Count > 0)
		{
			// Grab the first/oldest function in the list
			Action someFunc = functionsToRunInMainThread[0];
			functionsToRunInMainThread.RemoveAt(0);

			// Now run it
			someFunc();
		}
	}
	public void StartThreadedFunction(Action someFunctionWithNoParametres)
	{
		Thread t = new Thread (new ThreadStart (someFunctionWithNoParametres));
		t.Start ();
	}

	public void QueueMainThreadFunction( Action someFunction )
	{
		// We need to make sure that someFunction is running from the
		// main thread

		//someFunction(); // This isn't okay, if we're in a child thread

		functionsToRunInMainThread.Add(someFunction);

	}
	void SlowFunctionThatDoesAUnityThing( Vector3 foo, float[] bar, Color[] pixels )
	{
		// First we do a really slow thing
		Thread.Sleep(2000); // Sleep for 2 seconds

		// Now we need to modify a Unity gameobject
		Action aFunction = () => {
			Debug.Log("The results of the child thread are being applied to a Unity GameObject safely.");
			this.transform.position = new Vector3(1,1,1);   // NOT ALLOWED FROM A CHILD THREAD
		};

		// NOTE: We still aren't allowed to call this from a child thread
		//aFunction();

		QueueMainThreadFunction( aFunction );
	}
}
