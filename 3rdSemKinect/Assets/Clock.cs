using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Clock : NetworkBehaviour {
    double time;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log("Network time is " + Network.time);
        if (isServer)
        {
            int i = 0;
            Debug.Log("Hellu");
            while (i < Network.connections.Length)
            {
                Debug.Log("Player "+ i + " has " + Network.GetLastPing(Network.connections[i]) + " ms ping");
                i++;
            }
        }
        
	}
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
        }
        else
        {
            time = Network.time - info.timestamp;
            Debug.Log(time);
        }
    }
}
