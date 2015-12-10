using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class UserTracking : MonoBehaviour {

    GameObject[] cubes;
    List<GameObject> players;
    bool Calibrated;

    List<List<GameObject>> calibratedPlayers;
	// Use this for initialization
	void Start () {
        Calibrated = false;
        cubes = GameObject.FindGameObjectsWithTag("Player");
        addToPlayers(cubes);
        players = new List<GameObject>() { };
        
    }
	
	// Update is called once per frame
	void Update () {

	
	}
    public void addToPlayers(GameObject[] cubes)
    {
        for (int i = 0; i < cubes.Length; i++)
        {
            players.Add(cubes[i]);
        }
            
    }
    [Server]
    public void checkForUsers()
    {
        int i = 0;
        while(players.Count >= 1){
            for (int j = 0; j < players.Count; j++)
            {
                if(i != j)
                {
                    if(players[i].transform.position.magnitude - players[j].transform.position.magnitude < 0.2)
                    {
                        List<GameObject> player = new List<GameObject>();
                        player.Add(players[i]);
                        player.Add(players[j]);
                        calibratedPlayers.Add(player);
                        players.Remove(players[i]);
                        players.Remove(players[j]);
                        break;
                    }
                }
                else if(players.Count == 1)
                {
                    List<GameObject> player = new List<GameObject>();
                    player.Add(players[i]);
                    player.Add(players[j]);
                    calibratedPlayers.Add(player);
                    players.Remove(players[i]);
                    break;
                }
            }
        }
    }
    [Server]
    public void checkIfUsersAreTracked()
    {
        int i = 0;
        bool tracked;
        while (i < players.Count)
        {
            tracked = false;
            for (int j = 0; j < calibratedPlayers.Count; j++)
            {
                for(int n = 0; n < calibratedPlayers[j].Count; n++)
                {
                    if(players[i] == calibratedPlayers[j][n])
                    {
                        players.Remove(players[i]);
                        tracked = true;
                        break;
                    }
                    else if( players[i].transform.position.magnitude - calibratedPlayers[j][n].transform.position.magnitude < 0.2)
                    {
                        calibratedPlayers[j].Add(players[i]);
                        players.Remove(players[i]);
                        tracked = true;
                        break;
                    }
                }
            }
            if (!tracked)
            {
                i++;
                
            }
        }
    }
    [Server]
    public void checkIfPlayerIsTracked()
    {
        for (int j = 0; j < calibratedPlayers.Count; j++)
        {
            for (int i = 0; i < calibratedPlayers[i].Count; i++)
            {
                if (calibratedPlayers[i][j].transform.position.x > 100)
                {
                    calibratedPlayers[i].Remove(calibratedPlayers[i][j]);
                    if(calibratedPlayers[i].Count == 0)
                    {
                        calibratedPlayers.Remove(calibratedPlayers[i]);
                    }
                }
            }
        }
    }

}
