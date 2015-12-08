using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEngine.Networking;

public class UserController : NetworkBehaviour {
    private KinectWrapper.NuiSkeletonFrame skeletonFrame;

    public GameObject[] users = new GameObject[6];
    public GameObject prefab;
    // Use this for initialization
    void Start () {
        for (int i = 0; i < users.Length; i++)
        {
            users[i] = Network.Instantiate(prefab, new Vector3(0,0,0), Quaternion.identity, 0) as GameObject;
            users[i].transform.GetComponent<MeshRenderer>().material.color= RandomColor();
            //Instantiate a cube for each user tracked;
        }
    }
    Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }

    // Update is called once per frame
    public KinectManager manager;

    void Update ()
    {
        if (manager.KinectInitialized)
        {
            skeletonFrame = manager.skeletonFrame;
            for (int i = 0; i < skeletonFrame.SkeletonData.Length; i++)
            {
                KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
                uint userId = skeletonData.dwTrackingID;
                if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
                {
                    users[i].transform.position = manager.kinectToWorld.MultiplyPoint3x4(skeletonData.Position);
                }
                //Move a cube to each users position;
            }
        }
    }
}
