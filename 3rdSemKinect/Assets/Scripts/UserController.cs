using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserController : NetworkBehaviour
{

    private KinectWrapper.NuiSkeletonFrame skeletonFrame;
    public KinectManager manager;

    public GameObject[] users = new GameObject[6];
    public GameObject prefab;

    readonly Vector3 initialPosVector3 = new Vector3(50, 50, 50);

    public List<string> allUsers;
    private OffsetCalculator offsetCalculator;

    private float timeStep = 10.0f;
    private float timeReset;
    public Button[] buttons = new Button[4];
    // Use this for initialization

    public override void OnStartClient()
    {
        base.OnStartClient();
        allUsers = new List<string>();
        buttons[0] = GameObject.Find("Apply Offset").GetComponent<Button>();
        buttons[1] = GameObject.Find("Activate Logging").GetComponent<Button>();
        buttons[2] = GameObject.Find("Deactivate Logging").GetComponent<Button>();
        buttons[3] = GameObject.Find("Spawn Objects").GetComponent<Button>();

        buttons[0].onClick.AddListener(OffsetObjects);
        buttons[1].onClick.AddListener(StartLogging);
        buttons[2].onClick.AddListener(StopLogging);
        buttons[3].onClick.AddListener(SpawnObjects);

    }
    [Command]
    void Cmd_SpawnObjects()
    {
        for (int i = 0; i < users.Length; i++)
        {
            ClientScene.RegisterPrefab(prefab);
            users[i] = Instantiate(prefab, initialPosVector3, Quaternion.identity) as GameObject;
            UserSyncPosition userSyncPosition = users[i].transform.GetComponent<UserSyncPosition>();
            userSyncPosition.isCalibrationUser = false;
            Color rndColor = RandomColor();
            userSyncPosition.Initialize((GetComponent<NetworkIdentity>().netId.Value - 1)+ " " + i, rndColor);
            NetworkServer.SpawnWithClientAuthority(users[i],connectionToClient);
            userSyncPosition.Cmd_ChangeIdentity(rndColor, ("SubUser " + (GetComponent<NetworkIdentity>().netId.Value - 1) + " " + i));
        }
        Rpc_SpawnObjects(users);
    }

    [ClientRpc]
    void Rpc_SpawnObjects(GameObject[] userGameObjects)
    {
        if (isLocalPlayer)
        {
            users = userGameObjects;
            foreach (var i in userGameObjects)
            {
                i.transform.parent = transform;
            }
        }
        else
        {
            users = userGameObjects;
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

    private float timePassed;
    public bool Logging;

    // Update is called once per frame

    public void SpawnObjects()
    {
        if (isLocalPlayer)
        {
            Cmd_SpawnObjects();
        }
    }

    public void OffsetObjects()
    {
        if (isLocalPlayer)
        {
            foreach (var i in users)
            {
                i.GetComponent<UserSyncPosition>().rotationalOffset = true;
                i.GetComponent<UserSyncPosition>().positionalOffset = true;
            }
        }
    }

    public void StartLogging()
    {
        Logging = true;
    }

    public void StopLogging()
    {
        Logging = false;
    }
    public float[] waitTimers = new float[6];

    public GameObject[] playersGameObjects = new GameObject[12];

    public List<Transform> usersList = new List<Transform>();
    float test3Timer = 120f;
    [ClientCallback]
    void Update()
    {
        if (Logging)
        {
            test3Timer -= Time.deltaTime;
        }
        if (test3Timer <= 0)
        {
            Logging = false;
        }

        playersGameObjects = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playersGameObjects.Length; i++)
        {
            for (int j = 0; j < playersGameObjects.Length; j++)
            {
                if (Mathf.Abs(playersGameObjects[i].transform.position.x - playersGameObjects[j].transform.position.x) < 20f &&
                    Mathf.Abs(playersGameObjects[i].transform.position.x - playersGameObjects[j].transform.position.x) > 0 && playersGameObjects[i].transform.position.x < 5)
                    {
                    //if ( Vector3.Distance(playersGameObjects[i].transform.position, playersGameObjects[j].transform.position) < 20f && 
                    //    Vector3.Distance(playersGameObjects[i].transform.position, playersGameObjects[j].transform.position) > 0)
                    //{
                    if (!usersList.Contains(playersGameObjects[i].transform))
                    {
                        usersList.Add(playersGameObjects[i].transform);
                    }
                }
                else
                {
                    if (usersList.Contains(playersGameObjects[i].transform))
                    {
                        usersList.Remove(playersGameObjects[i].transform);
                    }
                }
            }
        }
        manager = KinectManager.Instance;

        if (Logging)
        {
            timePassed += Time.deltaTime;
            timeReset = timePassed% timeStep;
        }

        for(int i = 0 ; i < waitTimers.Length; i++)
        {
            if (waitTimers[i] >= 0)
            {
                waitTimers[i] -= Time.deltaTime;
            }
        }

        if (manager != null && manager.KinectInitialized && isLocalPlayer)
        {
            skeletonFrame = manager.skeletonFrame;
            int fuckI = 0;
            for (int i = 0; i < skeletonFrame.SkeletonData.Length; i++)
            {
                if (users[i] == null)
                {
                    return;
                }
                KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
                UserSyncPosition userSyncPosition = users[i].GetComponent<UserSyncPosition>();
                Vector3 skeletonPos = manager.kinectToWorld.MultiplyPoint3x4(skeletonData.Position);

                string userId = userSyncPosition.transform.name;

                if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
                {
                    if (!allUsers.Contains(userId))
                    {
                        if (Logging)
                        {
                            Logger.LogData("Tracking Begun: ", skeletonPos, userId, timePassed);
                        }
                        allUsers.Add(userId);
                    }
                    if (timeReset > 0 && timeReset < 1 && timeBool && Logging)
                    {
                        Debug.Log("Continued tracking");
                        for (int j = 0; j < users.Length; j++)
                        {
                            if (users[j].transform.position.x < 10)
                            {
                                //Logger.LogData("Tracking Continued: ", users[j].transform.position, users[j].name,timePassed);
                            }
                        }
                        //Logger.LogData("Tracking Continued: ", skeletonPos, userId, timePassed);
                        timeBool = false;
                    }
                    if (timeReset > 1)
                    {
                        timeBool = true;
                    }
                    waitTimers[i] = 2;
                    userSyncPosition.MoveWithUser(skeletonPos, userId);
                }
                else
                {
                    if (Logging && allUsers.Contains(userId))
                    {
                        if (waitTimers[i] <= 0)
                        {
                            Logger.LogData("Tracking Lost: ", skeletonPos, userId, timePassed);
                            allUsers.Remove(userId);
                            waitTimers[i] = 0;
                        }
                    }
                    if (!allUsers.Contains(userId))
                    {
                        userSyncPosition.MoveWithUser(initialPosVector3);
                    }
                }
            }
        }
    }

    private bool timeBool = true;


    public bool MirroredMovement { get; set; }

    public bool Offset { get; set; }
}
