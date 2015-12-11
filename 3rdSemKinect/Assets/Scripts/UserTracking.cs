using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class UserTracking : NetworkBehaviour {

    public GameObject[] cubes;
    public List<GameObject> players;
    bool Calibrated;
    bool Check;
    bool isFirst;

    public List<List<GameObject>> calibratedPlayers;
    public List<GameObject> viewFirstIndex;
	// Use this for initialization
	void Start () {
        Calibrated = false;
        cubes = GameObject.FindGameObjectsWithTag("Player");
        addToPlayers(cubes);
        players = new List<GameObject>() { };
        calibratedPlayers = new List<List<GameObject>>();
        Check = false;
        isFirst = true;
        
    }
	
	// Update is called once per frame
	void Update () {
        if (Check && isServer)
        {
            check();
            viewFirstIndex = calibratedPlayers[0];
        }



    }
    public void check()
    {
        if (isServer && isFirst)
        {
            //Debug.Log("1 Hello");
            cubes = GameObject.FindGameObjectsWithTag("Player");
            addToPlayers(cubes);
            checkForUsers();
            isFirst = false;
        }
        else if (isServer && !isFirst)
        {
            //Debug.Log("2 Hello");
            cubes = GameObject.FindGameObjectsWithTag("Player");
            addToPlayers(cubes);
            checkIfPlayerIsTracked();
            if(cubes != null)
            {
                checkIfUsersAreTracked();
                checkForUsers();
                Check = true;

            }

        }
    }
    public void addToPlayers(GameObject[] cubes)
    {
        //Debug.Log("3 Hello");
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
                        List<GameObject> player = new List<GameObject>() {players[i],players[j] };
                        calibratedPlayers.Add(player);
                        players.Remove(players[i]);
                        players.Remove(players[j]);
                        break;
                    }
                }
                else if(players.Count == 1)
                {
                    //Debug.Log("adding a single one");
                    List<GameObject> player = new List<GameObject>() { players[i]};
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
                        //Debug.Log("Removing player");
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

        for (int i = 0; i < calibratedPlayers.Count; i++)
        {
            if (calibratedPlayers[i] != null)
            {
                //Debug.Log("4 Hello");
                for (int j = 0; j < calibratedPlayers[i].Count; j++)
                {
                    if (calibratedPlayers[i][j] != null && calibratedPlayers[i][j].transform.position.x > 20)
                    {
                        //Debug.Log("5 Hello");
                        calibratedPlayers[i].Remove(calibratedPlayers[i][j]);
                        if (calibratedPlayers[i].Count == 0)
                        {
                            //Debug.Log("Removing Player");
                            calibratedPlayers.Remove(calibratedPlayers[i]);
                        }
                    }
                }
            }
        }
    }

}
