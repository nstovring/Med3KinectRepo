using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Clock : NetworkBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log("Network time is " + Network.time);
        if (isServer)
        {
            for (int i = 0; i < Network.connections.Length; i++)
            {
                Debug.Log("Player "+ i + " has " + Network.GetLastPing(Network.connections[i]) + " ms ping");
            }
        }
        
	}
}
