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

    private List<uint> allUsers;
    private OffsetCalculator offsetCalculator;

    // Use this for initialization

    public override void OnStartClient()
    {
        base.OnStartClient();
        for (int i = 0; i < users.Length; i++)
        {
            users[i] = Instantiate(prefab, initialPosVector3, Quaternion.identity) as GameObject;
            UserSyncPosition userSyncPosition = users[i].transform.GetComponent<UserSyncPosition>();
            userSyncPosition.isCalibrationUser = false;
            //userSyncPosition.OnStartLocalPlayer();
            //userSyncPosition.
            users[i].transform.GetComponent<MeshRenderer>().material.color = RandomColor();
            NetworkServer.Spawn(users[i]);
            //Instantiate a cube for each user tracked;
        }
        allUsers = new List<uint>();
    }

    Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }

    // Update is called once per frame
    [ClientCallback]
    void Update ()
    {
        if (manager.KinectInitialized)
        {
            skeletonFrame = manager.skeletonFrame;
            for (int i = 0; i < skeletonFrame.SkeletonData.Length; i++)
            {
                KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
                Vector3 skeletonPos = manager.kinectToWorld.MultiplyPoint3x4(skeletonData.Position);
                UserSyncPosition userSyncPosition = users[i].GetComponent<UserSyncPosition>();

                uint userId = skeletonData.dwTrackingID;

                if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
                {
                    if (!allUsers.Contains(userId))
                    {
                        allUsers.Add(userId);
                    }
                    //userSyncPosition.MoveWithUser(skeletonPos);
                    //userSyncPosition.CmdProvidePositionToServer(skeletonPos, Vector3.zero);
                    users[i].transform.position = skeletonPos;
                }
                else
                {
                    if (allUsers.Contains(userId))
                    {
                        allUsers.Remove(userId);
                    }
                    users[i].transform.position = initialPosVector3;
                }
            }
        }
    }

    

    public bool MirroredMovement { get; set; }

    public bool Offset { get; set; }
}
