using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEngine.Networking;

public class UserController : NetworkBehaviour {

    private KinectWrapper.NuiSkeletonFrame skeletonFrame;
    public KinectManager manager;

    public GameObject[] users = new GameObject[6];
    public GameObject prefab;

    Vector3 initialPosVector3 = new Vector3(300,0,0);

    public List<uint> allUsers;
    private OffsetCalculator offsetCalculator;

    // Use this for initialization

    public override void OnStartClient()
    {
        base.OnStartClient();
        //int = 0;
        
        allUsers = new List<uint>();
    }
    [Command]
    void Cmd_SpawnObjects()
    {
        /*for (int i = 0; i < users.Length; i++)
        {
            users[i] = Instantiate(prefab, initialPosVector3, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(users[i]);
            UserSyncPosition userSyncPosition = users[i].transform.GetComponent<UserSyncPosition>();
            userSyncPosition.isCalibrationUser = false;
            userSyncPosition.Initialize(" " + (GetComponent<NetworkIdentity>().netId.Value - 1), RandomColor());
        }*/
        Rpc_SpawnObjects();
    }

    [ClientRpc]
    void Rpc_SpawnObjects()
    {
        for (int i = 0; i < users.Length; i++)
        {
            users[i] = Instantiate(prefab, initialPosVector3, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(users[i]);
            UserSyncPosition userSyncPosition = users[i].transform.GetComponent<UserSyncPosition>();
            userSyncPosition.isCalibrationUser = false;
            userSyncPosition.Initialize(" " + (GetComponent<NetworkIdentity>().netId.Value - 1) + " " + i, RandomColor());
        }
    }

    Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }

    private float timePassed;
    public bool Logging;

    // Update is called once per frame
    [Client]
    void Update ()
    {
        manager = KinectManager.Instance;

        if (Input.GetKeyUp(KeyCode.S) && isLocalPlayer)
        {
            Cmd_SpawnObjects();
        }
        if (Input.GetKeyUp(KeyCode.O) && isLocalPlayer)
        {
            foreach (var i in users)
            {
                i.GetComponent<UserSyncPosition>().Offset = true;
            }
        }
        if (Logging)
        {
            timePassed += Time.deltaTime;
        }
        if (manager.KinectInitialized && isLocalPlayer)
        {
            Debug.Log("Kinect initialized and local player");
            skeletonFrame = manager.skeletonFrame;
            for (int i = 0; i < skeletonFrame.SkeletonData.Length; i++)
            {
                Debug.Log("Checking users");
                if (users[i] == null)
                {
                    return;
                }

                Debug.Log("Users exist");

                KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
                UserSyncPosition userSyncPosition = users[i].GetComponent<UserSyncPosition>();
                Vector3 skeletonPos = manager.kinectToWorld.MultiplyPoint3x4(skeletonData.Position);

                uint userId = skeletonData.dwTrackingID;

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
                    Debug.Log("Moving Cube!!");
                    userSyncPosition.MoveWithUser(skeletonPos);
                }
                else
                {
                    if (allUsers.Contains(userId))
                    {
                        if (Logging)
                        {
                            Logger.LogData("Tracking Lost: ", skeletonPos, userId, timePassed);
                        }
                        allUsers.Remove(userId);
                    }
                    Debug.Log("Lost Cube!!");
                    userSyncPosition.MoveWithUser(initialPosVector3);
                }
            }
        }
    }

    

    public bool MirroredMovement { get; set; }

    public bool Offset { get; set; }
}
