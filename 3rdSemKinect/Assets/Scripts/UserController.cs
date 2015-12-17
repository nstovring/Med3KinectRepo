using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

//The class user controller inherits from NetworkBehaviour which enables it to use methods related to networking
public class UserController : NetworkBehaviour
{
    //The skeletonFrame is a struct which stores every skeleton tracked by the kinect
    private KinectWrapper.NuiSkeletonFrame skeletonFrame;
    // Reference to the kinectmanager. Class which is downloaded f´rom the asset store for accesisble use of Kinects with unity
    public KinectManager manager;

    // Array containing every gameobject representation of the users tracked
    public GameObject[] users = new GameObject[6];
    // Gameobject displaying how a user looks like
    public GameObject prefab;

    //This is the position that the users are by defualt when not tracked by the kinect
    readonly Vector3 initialPosVector3 = new Vector3(50, 50, 50);

    // A list of all users who are currently being tracked
    public List<string> allUsers;
    // A reference to the class which handles calculations of the offset
    private OffsetCalculator offsetCalculator;

    //Float responsible for handling the seconnds increment during logging
    private float timeStep = 10.0f;
    //Timereset increases to a maximum of timestep and the resets
    private float timeReset;
    //Float containing the currentTimePassed;
    private float timePassed;
    //Bool handling wether or not the the class is logging data or not
    public bool Logging;

    //Buttons array holding the UI elements for the calibration
    public Button[] buttons = new Button[4];

    //Array of timers for each individual user in the user array
    public float[] waitTimers = new float[6];

    //Array containing every user in the scene including the other clients users
    public GameObject[] playersGameObjects = new GameObject[12];

    //List containing all users
    public List<Transform> usersList = new List<Transform>();

    //Bool responsible for handling when too log data at certain increments
    private bool timeBool = true;

    //Mirros the movement of the users on the x & z axis
    public bool MirroredMovement { get; set; }

    //Bool controlling wether allusers should move offset with calibration data saved in previous scene
    public bool Offset { get; set; }

    //The on start client method runs the moment a client connects to a server
    public override void OnStartClient()
    {
        //Call the code in the original method
        base.OnStartClient();
        // Initialize the list allUsers
        allUsers = new List<string>();
        //Find and assign the buttons to the buttons array
        buttons[0] = GameObject.Find("Apply Offset").GetComponent<Button>();
        buttons[1] = GameObject.Find("Activate Logging").GetComponent<Button>();
        buttons[2] = GameObject.Find("Deactivate Logging").GetComponent<Button>();
        buttons[3] = GameObject.Find("Spawn Objects").GetComponent<Button>();

        //Add methods to every button which runs once when the button is clicked
        buttons[0].onClick.AddListener(OffsetObjects);
        buttons[1].onClick.AddListener(StartLogging);
        buttons[2].onClick.AddListener(StopLogging);
        buttons[3].onClick.AddListener(SpawnObjects);

    }

    //The command attribute specifies that the method only runs on the server, but is called by the client
    [Command]
    // Cmd_SpawnObjects Instantiates the gamesobject which represent the tracked users
    void Cmd_SpawnObjects()
    {
        for (int i = 0; i < users.Length; i++)
        {
            //To instantiate on a network the gameobject prefab must be registered as a spawnable prefab
            ClientScene.RegisterPrefab(prefab);
            //The  prefab is instantiated and asssigned to the users array
            users[i] = Instantiate(prefab, initialPosVector3, Quaternion.identity) as GameObject;
            // Get the class UserSyncPosition is aquired from the prefab
            UserSyncPosition userSyncPosition = users[i].transform.GetComponent<UserSyncPosition>();
            //When this class is in use the calibration has been completed therefore the users are not calibration users
            userSyncPosition.isCalibrationUser = false;
            //Assign a RandomColor to the prefab
            Color rndColor = RandomColor();
            //Call the initialize method on the userSyncPosition class on the current user
            userSyncPosition.Initialize((GetComponent<NetworkIdentity>().netId.Value - 1)+ " " + i, rndColor);
            //Spawn the prefab on the server after initialization, enabliing us to call network methods from classes on it
            NetworkServer.SpawnWithClientAuthority(users[i],connectionToClient);
            //Call the Cmd_changeIdentity method, which recieves The networkidentity netids' value as well as a number from the loop
            userSyncPosition.Cmd_ChangeIdentity(rndColor, ("SubUser " + (GetComponent<NetworkIdentity>().netId.Value - 1) + " " + i));
        }
        //This method recieves the array of users previously filled with prefabs and is called on the clients
        Rpc_SpawnObjects(users);
    }

    //The ClientRpc Attribute means that this method is only called from the server, yet runs on all clients
    [ClientRpc]
    void Rpc_SpawnObjects(GameObject[] userGameObjects)
    {
        //If the client is the localPlayer that is to say refering to its' own instance only
        if (isLocalPlayer)
        {
            //The array recieved is assigned to this class
            users = userGameObjects;
            //Every gameobject in the array is set to the the child of the gameobject this script is attatched to
            foreach (var i in userGameObjects)
            {
                i.transform.parent = transform;
            }
        }
        else
        {
            //The other clients connected to the server also assign their user array to their own gameobject
            users = userGameObjects;
            foreach (var i in userGameObjects)
            {
                i.transform.parent = transform;
            }
        }
    }

    //Method which return a Random Color
    Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }


    //Method assigned to a UI button
    public void SpawnObjects()
    {
        if (isLocalPlayer)
        {
            Cmd_SpawnObjects();
        }
    }

    //Method assigned to a UI button
    public void OffsetObjects()
    {
        if (isLocalPlayer)
        {
            // For every users in the list set the offset booleans to true
            foreach (var i in users)
            {
                i.GetComponent<UserSyncPosition>().rotationalOffset = true;
                i.GetComponent<UserSyncPosition>().positionalOffset = true;
            }
        }
    }


    //Method assigned to a UI button sets the Logging variable to true
    public void StartLogging()
    {
        Logging = true;
    }

    //Method assigned to a UI button sets the Logging variable to false
    public void StopLogging()
    {
        Logging = false;
    }
    
    //The ClientCallBack attribute meanth that the method will only run on clients and no return errors when trying to run on the server
    [ClientCallback]
    void Update()
    {
        //Assign all gameObject with the name Player to the array
        playersGameObjects = GameObject.FindGameObjectsWithTag("Player");
        //Assign the KinectManager class in the scene to the manager variable
        manager = KinectManager.Instance;

        if (Logging)
        {
            //When logging starts, start increasing the time value
            timePassed += Time.deltaTime;
            //With modulus the timeReset value will reset to 0 every time timePassed rises above timeStep
            timeReset = timePassed% timeStep;
        }

        //Always reduce every value in the waitTimers array
        for(int i = 0 ; i < waitTimers.Length; i++)
        {
            if (waitTimers[i] >= 0)
            {
                waitTimers[i] -= Time.deltaTime;
            }
        }

        //Ensure that this class has acces to the KinectManager, the kinect is initialized aswell as being the local instance of the client on the server
        if (manager != null && manager.KinectInitialized && isLocalPlayer)
        {
            //Get the skeleton data from the KinectManager
            skeletonFrame = manager.skeletonFrame;
            //For every skeleton in the skeletonFrame
            for (int i = 0; i < skeletonFrame.SkeletonData.Length; i++)
            {
                //If the there is no gameobject to represent the user for this skeleton stop the method
                if (users[i] == null)
                {
                    return;
                }
                //Assign the Current skeletondata to to a new variable
                KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
                //Get the current user prefabs UserSyncPosition class instance
                UserSyncPosition userSyncPosition = users[i].GetComponent<UserSyncPosition>();
                //Get the position of the user and assign it to a vector3
                Vector3 skeletonPos = manager.kinectToWorld.MultiplyPoint3x4(skeletonData.Position);

                //Get the name of the current user
                string userId = userSyncPosition.transform.name;

                //Check if the current skeleton is even being tracked by the kinect
                if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
                {
                    //As long as the the name of this user is not in the list of allUsers
                    if (!allUsers.Contains(userId))
                    {
                        //Log when this user is being tracked if Logging is true
                        if (Logging)
                        {
                            //Save the string stating what type of log, the position of the user, the name and at which time during logging
                            Logger.LogData("Tracking Begun: ", skeletonPos, userId, timePassed);
                        }
                        //Add the user to the list
                        allUsers.Add(userId);
                    }
                    if (timeReset > 0 && timeReset < 1 && timeBool && Logging)
                    {
                        //In the split second where the timeReset is between 0 & 1 run a loop for every user tracked
                        for (int j = 0; j < users.Length; j++)
                        {
                            //If the users x value is below 10 that means it is within the scene therefore being tracked
                            if (users[j].transform.position.x < 10)
                            {
                                Logger.LogData("Tracking Continued: ", users[j].transform.position, users[j].name,timePassed);
                            }
                        }
                        //Set this to false to avoid multiple logs being saved during the same second
                        timeBool = false;
                    }
                    //After a second has passed set timeBool to true
                    if (timeReset > 1)
                    {
                        timeBool = true;
                    }
                    //Set this users waitTimer to 2 to show that it is currently being tracked aswell;
                    waitTimers[i] = 2;
                    //Finally move the prefab representing the user with the skeletonPos recieved from the kinect
                    userSyncPosition.MoveWithUser(skeletonPos);
                }
                //else If the current skeleton is not being tracked
                else
                {
                    //if the current user is not being tracked and is in the list allUsers
                    if (Logging && allUsers.Contains(userId))
                    {
                        //The waitTimers serve as a 2 second delay before removing a user from the tracked list
                        if (waitTimers[i] <= 0)
                        {
                            //When 2 seconds have passed without this user being tracked log the user removal from tracking
                            Logger.LogData("Tracking Lost: ", skeletonPos, userId, timePassed);
                            allUsers.Remove(userId);
                            waitTimers[i] = 0;
                        }
                    }
                    //If the current user who is not being track is not in the list of tracked users, move the prefab out of the scene
                    if (!allUsers.Contains(userId))
                    {
                        //Call the MoveWithUser method and tell the user to move to the initial position
                        userSyncPosition.MoveWithUser(initialPosVector3);
                    }
                }
            }
        }
    }
}
