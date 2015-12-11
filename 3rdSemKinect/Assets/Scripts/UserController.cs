﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserController : NetworkBehaviour
{

    private KinectWrapper.NuiSkeletonFrame skeletonFrame;
    public KinectManager manager;

    public GameObject[] users = new GameObject[6];
    public GameObject prefab;

    readonly Vector3 initialPosVector3 = new Vector3(50, 0, 0);

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

    [ClientCallback]
    void Update()
    {
        manager = KinectManager.Instance;

        if (Logging)
        {
            timePassed += Time.deltaTime;
            timeReset = timePassed% timeStep;
        }


        if (manager != null && manager.KinectInitialized && isLocalPlayer)
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
                        Logger.LogData("Tracking Continued: ", skeletonPos, userId, timePassed);
                        timeBool = false;
                    }
                    if (timeReset > 1)
                    {
                        timeBool = true;
                    }
                    userSyncPosition.MoveWithUser(skeletonPos, userId);
                }
                else
                {
                    if (Logging && allUsers.Contains(userId))
                    {
                        Logger.LogData("Tracking Lost: ", skeletonPos, userId, timePassed);
                        allUsers.Remove(userId);
                    }
                    userSyncPosition.MoveWithUser(initialPosVector3);
                }
            }
        }
    }

    private bool timeBool = true;


    public bool MirroredMovement { get; set; }

    public bool Offset { get; set; }
}
