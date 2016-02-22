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
    public int[] tempJoints;
    string test;
    uint playerID;
    KinectManager manager;
    public Button button;
    public Button button2;
    float time;
    float sendRate;
    // Use this for initialization
    void Start () {
        players = new GameObject[20];
        sendRate = 0.1f;
        time = 0;
        //spawnObjects();

    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        button = GameObject.FindGameObjectWithTag("spawn button").GetComponent<Button>();
        button2 = GameObject.FindGameObjectWithTag("send").GetComponent<Button>();
        button.onClick.AddListener(spawnObjects);
        button2.onClick.AddListener(sendJoints);
        

    }
    public void sendJoints()
    {
        if (hasAuthority)
        {
            tempJoints = toArray(trackedJoints);
            Cmd_sendTrackedJoints(tempJoints);
        }
    }
    public void spawnObjects()
    {
        if (hasAuthority)
        {
            Cmd_SpawnObjects();
        }
    }
    [ClientCallback]
    void FixedUpdate()
    {
        if (hasAuthority) {
            playerID = manager != null ? manager.GetPlayer1ID() : 0;
            trackedJoints = new List<int>();
            getTrackedJoints();
            //if(time >= sendRate)
            if(true)
            {
                sendJoints();
                time = 0;
            }
            time += Time.deltaTime;
        }
    }
    void getTrackedJoints()
    {
        for (int i = 0; i < 20; i++)
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
    void Cmd_sendTrackedJoints(int[] joints)
    {
        trackedJoints = toList(joints);
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
            Color rndColor = RandomColor();
            //Call the initialize method on the userSyncPosition class on the current user
            userSyncPosition.Initialize((GetComponent<NetworkIdentity>().netId.Value - 1) + " " + i, rndColor);
            //Spawn the prefab on the server after initialization, enabliing us to call network methods from classes on it
            NetworkServer.SpawnWithClientAuthority(players[i], connectionToClient);
            userSyncPosition.jointNum = i;
            userSyncPosition.isGivenJoint = true;
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
    int[] toArray(List<int> list)
    {
        int[] temp = new int[list.Count];
        for(int i = 0; i < list.Count; i++)
        {
            temp[i] = list[i];
        }
        return temp;
    }
    List<int> toList(int[] list)
    {
        List<int> temp = new List<int>();
        for (int i = 0; i < list.Length; i++)
        {
            temp.Add(list[i]);
        }
        return temp;
    }
}
