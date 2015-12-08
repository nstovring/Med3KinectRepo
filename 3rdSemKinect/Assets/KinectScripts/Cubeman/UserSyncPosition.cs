﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class UserSyncPosition : NetworkBehaviour
{

    [SyncVar] private Vector3 syncPos;
    [SyncVar] private Vector3 syncRot;

    [SyncVar] public bool Offset;
    [SyncVar] private Color userColor;
    [SyncVar] private string objectName;

    [SerializeField] private Transform myTransform;
    [SerializeField] private float lerpRate = 15;

    public bool MirroredMovement;

    private KinectManager manager;
    private OffsetCalculator offsetCalculator;

    // Update is called once per frame
    [ClientCallback]
    void FixedUpdate () {
	    TransmitPosition();
        LerpPosition();
	}

    void LerpPosition()
    {
        if (!isLocalPlayer)
        {
            myTransform.position = Vector3.Lerp(myTransform.position, syncPos, Time.deltaTime*lerpRate);
            myTransform.rotation = Quaternion.Lerp(myTransform.rotation, Quaternion.Euler(syncRot), Time.deltaTime * lerpRate);
            transform.GetComponent<MeshRenderer>().material.color = userColor;
            transform.name = objectName;
        }
    }

    [Command]
    void CmdProvidePositionToServer(Vector3 pos, Vector3 rot)
    {
        syncPos = pos;
        syncRot = rot;
    }

    [Command]
    void Cmd_ChangeIdentity(Color col, string objectName)
    {
        this.objectName = objectName;
        userColor = col;
    }


    void TransmitPosition()
    {
        if (isLocalPlayer)
        {
            if (manager == null)
            {
                manager = KinectManager.Instance;
                
            }
            else
            {
                MoveWithUser();
                CmdProvidePositionToServer(myTransform.position, myTransform.rotation.eulerAngles);
            }
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        string objectName = "User " + (GetComponent<NetworkIdentity>().netId.Value -1);
        userColor = new Color(Random.value, Random.value, Random.value);
        transform.GetComponent<MeshRenderer>().material.color = userColor;
        transform.name = objectName;
        Cmd_ChangeIdentity(userColor, objectName);
    }

    [Client]
    private void MoveWithUser()
    {
        offsetCalculator = OffsetCalculator.offsetCalculator;
        uint playerID = manager != null ? manager.GetPlayer1ID() : 0;
        Vector3 posPointMan = manager.GetUserPosition(playerID);
        posPointMan.z = !MirroredMovement ? -posPointMan.z : posPointMan.z;
        posPointMan.x *= 1;
        if (Offset)
        {
            Quaternion direction = Quaternion.AngleAxis(offsetCalculator.rotationalOffset.y, Vector3.up);
            transform.position = (direction*posPointMan) != Vector3.zero ? (direction*posPointMan) : posPointMan;
        }
        else
        {
            transform.position = posPointMan;
        }
        RotateWithUser();
    }

    [Client]
    private void RotateWithUser()
    {
        if (manager.IsUserDetected())
        {
            uint userId = manager.GetPlayer1ID();

            if (manager.IsJointTracked(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft) &&
                manager.IsJointTracked(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight))
            {
                Vector3 posLeftShoulder = manager.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
                Vector3 posRightShoulder = manager.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);

                posLeftShoulder.z = -posLeftShoulder.z;
                posRightShoulder.z = -posRightShoulder.z;

                Vector3 dirLeftRight = posRightShoulder - posLeftShoulder;
                dirLeftRight -= Vector3.Project(dirLeftRight, Vector3.up);

                Quaternion rotationShoulders = Quaternion.FromToRotation(Vector3.right, dirLeftRight);

                myTransform.rotation = rotationShoulders;
            }
        }
    }
}
