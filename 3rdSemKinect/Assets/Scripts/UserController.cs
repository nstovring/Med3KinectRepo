﻿using UnityEngine;
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
        if (Input.anyKeyDown)
        {
            Cmd_SpawnObjects();
        }

        manager = KinectManager.Instance;
        if (Logging)
        {
            timePassed += Time.deltaTime;
        }
        if (manager.KinectInitialized)
        {
            skeletonFrame = manager.skeletonFrame;
            for (int i = 0; i < skeletonFrame.SkeletonData.Length; i++)
            {
                if (users[i] == null)
                {
                    return;
                }
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
                    userSyncPosition.MoveWithUser(skeletonPos);
                    //userSyncPosition.CmdProvidePositionToServer(skeletonPos, Vector3.zero);
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
                    userSyncPosition.MoveWithUser(initialPosVector3);
                }
            }
        }
    }

    

    public bool MirroredMovement { get; set; }

    public bool Offset { get; set; }
}
