using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

//This class is responsible for keeping track of several users and group them together
public class UserTracking : NetworkBehaviour {
    //Variables are defined
    public GameObject[] cubes;
    bool Calibrated;
    bool Check;
    bool isFirst;

    //A List is a type that combines the best of the ArrayList and the Array. Indexes can dynamically be added and removed
    //like in an arraylist, while indexes can also be accessed with an index number, like arrays.
    //This List is used to store the cubes
    public List<GameObject> players;
    //This List stores more Lists of gameobjects, each of these lists is a real user which different cubes have been assigned to
    public List<List<GameObject>> calibratedPlayers;
    public List<GameObject> viewFirstIndex;

	void Start () {
        //initializing different variables
        Calibrated = false;
        cubes = GameObject.FindGameObjectsWithTag("Player");
        //addToPlayers is called in order to put the cubes array into the players List
        addToPlayers(cubes);
        players = new List<GameObject>() { };
        calibratedPlayers = new List<List<GameObject>>();
        Check = false;
        //isFirst ensures that some code only runs once
        isFirst = true;
        
    }
	
	void Update () {
        //uses the network to check if the script is on the server
        if (Check && isServer)
        {
            //runs check every frame if it's been initialized
            check();
        }



    }
    //method which automatically checks if cube objects are in the CalibratedPlayers List and adds them if they aren't
    //is basicly responsible for all calls in this class
    public void check()
    {
        //again checks if the method is called on the server, and if it is the first time it is being called
        if (isServer && isFirst)
        {
            //adds all cubes to the cubes array
            cubes = GameObject.FindGameObjectsWithTag("Player");
            //adds cube array to players List
            addToPlayers(cubes);
            //calls checkForUsers() method which groups cubes together in the calibratedPlayers List
            checkForUsers();
            //sets isFirst to false so this part is only called once
            isFirst = false;
        }
        //same check as before, though it checks if isFirst is false instead of true
        else if (isServer && !isFirst)
        {
            //adds all cubes to the cubes array
            cubes = GameObject.FindGameObjectsWithTag("Player");
            //adds cube array to players List
            addToPlayers(cubes);
            //calls checkIfPlayerIsTracked() method, which checks if cubes are already being tracked
            checkIfPlayerIsTracked();
            //checks if there are still objects in the cubes array
            if(cubes != null)
            {
                //calls checkIfUsersAreTracked() method, which checks if cubes are still attached to a person, or if they've lost track of him/her
                checkIfUsersAreTracked();
                //calls checkForUsers which adds cubes to the CalibratedPlayers List and group them together
                checkForUsers();
                //sets Check to true, so that the method check is run every frame on the server
                Check = true;

            }

        }
    }
    //method that adds objects from an array to the players List
    public void addToPlayers(GameObject[] cubes)
    {
        //runs through the inputted array
        for (int i = 0; i < cubes.Length; i++)
        {
            //adds each index to the players List
            players.Add(cubes[i]);

        }
            
    }
    //a method which groups together cubes by adding them in a specific list in the calibratedPlayers List if they are close together
    public void checkForUsers()
    {
        //an int which keeps track of which index in the players array should be checked, as the array is constantly being resized by removing indexes
        int i = 0;
        //a while loop is used instead of a for loop as the players.count varies as the code is run, and we only want the code to run while there are things in the List
        while(players.Count >= 1){
            //runs through the players List
            for (int j = 0; j < players.Count; j++)
            {
                //checks if the current indexes are referening to the same object
                if(players[i]!= players[j])
                {
                    //cheks if the cubes are close together
                    if(players[i].transform.position.magnitude - players[j].transform.position.magnitude < 0.2)
                    {
                        //a new list is made so that it can be added to the calibratedPlayers List of Lists
                        List<GameObject> player = new List<GameObject>() {players[i],players[j] };
                        //adds the list to the calibratedPlayers List
                        calibratedPlayers.Add(player);
                        //removes the two cubes from the players List
                        players.Remove(players[i]);
                        players.Remove(players[j - 1]);
                        //breaks so that the for loop doesn't get an indexOutOfBounds as the List resizes to its new length
                        break;
                    }
                }
                //checks if there's only one object left in the List
                else if(players.Count == 1)
                {
                    //adds it to its own list in calibratedPlayers and removes it from players. This also stops the while loop as the count now will be 0
                    List<GameObject> player = new List<GameObject>() { players[i]};
                    calibratedPlayers.Add(player);
                    players.Remove(players[i]);
                    break;
                }
            }
        }
    }
    //A method which checks if the cubes are already in the calibratedPlayers List
    public void checkIfUsersAreTracked()
    {
        //an int which keeps track of which index in the players array should be checked, as the array is constantly being resized by removing indexes
        int i = 0;
        //a bool which decides wether to increment i or stay at the current index
        bool tracked;
        //again //a while loop is used instead of a for loop as the players.count varies as the code is run, and we only want the code to run while there are things in the List
        while (i < players.Count)
        {
            //starts by setting tracked to false so it increments if the player object is not in the calibratedPlayers List already
            tracked = false;
            //runs through List
            for (int j = 0; j < calibratedPlayers.Count; j++)
            {
                // runs through List at index J
                for(int n = 0; n < calibratedPlayers[j].Count; n++)
                {
                    //checks if the players[i] is the same object as the one at the specific indexes in calibratedPlayers List
                    if(players[i] == calibratedPlayers[j][n])
                    {
                        // removes the player, sets tracked to true so i doesn't increment and breaks so the for loop doesn't get exceptions
                        players.Remove(players[i]);
                        tracked = true;
                        break;

                    }
                    //checks if the cubes are close to each other and calls the checkIfIsInList() method which checks if the obejct is in the list
                    else if( players[i].transform.position.magnitude - calibratedPlayers[j][n].transform.position.magnitude < 0.2 && checkIfIsInList(players[i], j))
                    {
                        //adds the object to the specific list in calibratedPlayers, removes the object from players, sets tracked to true and breaks
                        calibratedPlayers[j].Add(players[i]);
                        players.Remove(players[i]);
                        tracked = true;
                        break;
                    }
                }
            }
            //if the cube is not in the calibratedPlayers List already, then increment i which then checks the next index in the players List
            if (!tracked)
            {
                i++;
                
            }
        }
    }
    //Method which checks if an object is in a specific list in calibratedPlayers
    private bool checkIfIsInList(GameObject player, int num)
    {
        //runs through the specific list 
        for (int j = 0; j < calibratedPlayers[num].Count; j++)
        {
                //checks if the objects are refering to the same object
                if(player == calibratedPlayers[num][j])
                {
                    
                    return false;
                }
        }
        return true;
    }
    //A method which checks if the cube is still assigned to a tracked person, else, remove them from the list
    public void checkIfPlayerIsTracked()
    {
        //runs through List
        for (int i = 0; i < calibratedPlayers.Count; i++)
        {
            //checks if there is a List at that index of the list
            if (calibratedPlayers[i] != null)
            {
                //runs through the list at the specific index of the calibratedPlayers List
                for (int j = 0; j < calibratedPlayers[i].Count; j++)
                {
                    //checks if there is an object at that index and if that objects x coordinate is over 5 as this means
                    //that the kinect has lost track and the cube has been reset to its original position
                    if (calibratedPlayers[i][j] != null && calibratedPlayers[i][j].transform.position.x > 5)
                    {
                        //removes this index of the List
                        calibratedPlayers[i].Remove(calibratedPlayers[i][j]);
                        //checks if there are any objects left in the List
                        if (calibratedPlayers[i].Count == 0)
                        {
                            //removes the List or "player" from calibratedPlayers
                            calibratedPlayers.Remove(calibratedPlayers[i]);
                        }
                    }
                }
            }
        }
    }

}
