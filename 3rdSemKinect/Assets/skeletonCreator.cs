using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class skeletonCreator : NetworkBehaviour {

    public GameObject[] players;
    public GameObject prefab;
    readonly Vector3 initialPosVector3 = new Vector3(50, 50, 50);
    public List<int> trackedJoints;
    uint playerID;
    KinectManager manager;
    public Button button;
    float time;
    float sendRate;
    // Use this for initialization
    void Start () {
        players = new GameObject[20];
        sendRate = 1;
        time = 0;

    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        button = GameObject.FindGameObjectWithTag("spawn button").GetComponent<Button>();
        button.onClick.AddListener(spawnObjects);
        

    }
    public void spawnObjects()
    {
        if (hasAuthority)
        {
            Cmd_SpawnObjects();
        }
    }
    void FixedUpdate()
    {
        if (hasAuthority) {
            playerID = manager != null ? manager.GetPlayer1ID() : 0;
            trackedJoints = new List<int>();
            getTrackedJoints();
            if(time >= sendRate)
            {
                Cmd_sendTrackedJoints(trackedJoints);
                time = 0;
            }
            time += Time.deltaTime;
        }
    }
    void getTrackedJoints()
    {
        for (int i = 0; i < 2; i++)
        {
            if (manager == null)
            {
                manager = KinectManager.Instance;
            }
            else if (manager.IsJointTracked(playerID, i))
            {
                trackedJoints.Add(i);
            }
        }
            
    }
    [Command]
    void Cmd_sendTrackedJoints(List<int> joints)
    {
        trackedJoints = joints;
    }
    [Command]
    // Cmd_SpawnObjects Instantiates the gamesobject which represent the tracked users
    void Cmd_SpawnObjects()
    {
        for (int i = 0; i < players.Length; i++)
        {
            //To instantiate on a network the gameobject prefab must be registered as a spawnable prefab
            ClientScene.RegisterPrefab(prefab);
            //The  prefab is instantiated and asssigned to the users array
            players[i] = Instantiate(prefab, initialPosVector3, Quaternion.identity) as GameObject;
            // Get the class UserSyncPosition is aquired from the prefab
            UserSyncPosition userSyncPosition = players[i].transform.GetComponent<UserSyncPosition>();
            userSyncPosition.jointNum = i;
            userSyncPosition.isGivenJoint = true;
            Color rndColor = RandomColor();
            //Call the initialize method on the userSyncPosition class on the current user
            userSyncPosition.Initialize((GetComponent<NetworkIdentity>().netId.Value - 1) + " " + i, rndColor);
            //Spawn the prefab on the server after initialization, enabliing us to call network methods from classes on it
            NetworkServer.SpawnWithClientAuthority(players[i], connectionToClient);
            //Call the Cmd_changeIdentity method, which recieves The networkidentity netids' value as well as a number from the loop
            userSyncPosition.Cmd_ChangeIdentity(rndColor, ("SubUser " + (GetComponent<NetworkIdentity>().netId.Value - 1) + " " + i));
        }
        //This method recieves the array of users previously filled with prefabs and is called on the clients
        Rpc_SpawnObjects(players);
    }

    //The ClientRpc Attribute means that this method is only called from the server, yet runs on all clients
    [ClientRpc]
    void Rpc_SpawnObjects(GameObject[] userGameObjects)
    {
        //If the client is the localPlayer that is to say refering to its' own instance only
        if (isLocalPlayer)
        {
            //The array recieved is assigned to this class
            players = userGameObjects;
            //Every gameobject in the array is set to the the child of the gameobject this script is attatched to
            foreach (var i in userGameObjects)
            {
                i.transform.parent = transform;
            }
        }
        else
        {
            //The other clients connected to the server also assign their user array to their own gameobject
            players = userGameObjects;
            foreach (var i in userGameObjects)
            {
                i.transform.parent = transform;
            }
        }
    }
    Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }
}
