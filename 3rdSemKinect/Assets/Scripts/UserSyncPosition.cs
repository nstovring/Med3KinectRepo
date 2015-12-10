using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class UserSyncPosition : NetworkBehaviour
{

    [SyncVar] public Vector3 syncPos;
    [SyncVar] public Vector3 syncRot;

    [SyncVar] public bool positionalOffset;
    [SyncVar] public bool rotationalOffset;

   // [SyncVar] public bool Offset;

    [SyncVar] private Color userColor;
    [SyncVar] private string objectName;

    [SerializeField] private Transform myTransform;
    [SerializeField] private float lerpRate = 15;

    public bool MirroredMovement;
    public bool isCalibrationUser = true;

    private KinectManager manager;
    private OffsetCalculator offsetCalculator;

    private UserController PlayerObject;

    // Update is called once per frame
    [ClientCallback]
    void FixedUpdate () {
        if (isCalibrationUser)
        {
            TransmitPosition();
        }
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
    public void CmdProvidePositionToServer(Vector3 pos, Vector3 rot)
    {
        syncPos = pos;
        syncRot = rot;
    }

    [Command]
    public void Cmd_ChangeIdentity(Color col, string objectName)
    {
        this.objectName = objectName;
        userColor = col;
        NetworkServer.Spawn(gameObject);
    }


    public void TransmitPosition()
    {
        if (isLocalPlayer && isCalibrationUser)
        {
            if (manager == null)
            {
                manager = KinectManager.Instance;
                
            }
            else
            {
                MoveWithUser();
                //TiltWithUser();
                //RotateWithUser();
                OrientWithUser();
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

    public void Initialize(string id, Color userColor)
    {
        if (!isLocalPlayer)
        {
            this.objectName = "SubUser " + id;
            this.userColor = userColor;
            transform.GetComponent<MeshRenderer>().material.color = userColor;
            transform.name = objectName;
        }
    }

    [Client]
    public void MoveWithUser()
    {
        offsetCalculator = OffsetCalculator.offsetCalculator;
        uint playerID = manager != null ? manager.GetPlayer1ID() : 0;
        Vector3 posPointMan = manager.GetUserPosition(playerID);
        posPointMan.z = !MirroredMovement ? -posPointMan.z : posPointMan.z;
        posPointMan.x *= 1;

        if (rotationalOffset)
        {
            Quaternion directionY = Quaternion.AngleAxis(offsetCalculator.rotationalOffset.y, Vector3.up);
            Quaternion directionX = Quaternion.AngleAxis(offsetCalculator.rotationalOffset.x, Vector3.right);
            Quaternion direction = directionX*directionY;
            posPointMan = (direction * posPointMan) != Vector3.zero ? (direction * posPointMan) : posPointMan;
            transform.position = posPointMan;
        }

        if (positionalOffset)
        {
            posPointMan += offsetCalculator.positionalOffset;
            transform.position = posPointMan;
        }
        else
        {
            transform.position = posPointMan;
        }
    }

    [Client]
    public void MoveWithUser(Vector3 posPointMan)
    {
        offsetCalculator = OffsetCalculator.offsetCalculator;
        posPointMan.z = !MirroredMovement ? -posPointMan.z : posPointMan.z;
        posPointMan.x *= 1;

        if (rotationalOffset)
        {
            Quaternion directionY = Quaternion.AngleAxis(offsetCalculator.rotationalOffset.y, Vector3.up);
            Quaternion directionX = Quaternion.AngleAxis(offsetCalculator.rotationalOffset.x, Vector3.right);
            Quaternion direction = directionX * directionY;
            posPointMan = (direction * posPointMan) != Vector3.zero ? (direction * posPointMan) : posPointMan;
            transform.position = posPointMan;
            posPointMan += offsetCalculator.positionalOffset;
            transform.position = posPointMan;
            CmdProvidePositionToServer(transform.position, Vector3.zero);
        }
        else
        {
            transform.position = posPointMan;
            CmdProvidePositionToServer(transform.position, Vector3.zero);
        }
    }

    [Client]
    private void OrientWithUser()
    {
        if (manager.IsUserDetected())
        {
            uint userId = manager.GetPlayer1ID();

            if (manager.IsJointTracked(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter) &&
                manager.IsJointTracked(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter) &&
                manager.IsJointTracked(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft) &&
                manager.IsJointTracked(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight))
            {
                Vector3 posHipCenter = manager.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter);
                Vector3 posShoulderCenter = manager.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter);

                Vector3 posLeftShoulder = manager.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
                Vector3 posRightShoulder = manager.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);

                posLeftShoulder.z = -posLeftShoulder.z;
                posRightShoulder.z = -posRightShoulder.z;

                posHipCenter.z = -posHipCenter.z;
                posShoulderCenter.z = -posShoulderCenter.z;

                Vector3 dirLeftRight = posRightShoulder - posLeftShoulder;
                dirLeftRight -= Vector3.Project(dirLeftRight, Vector3.up);

                Vector3 directionUpDown = posShoulderCenter - posHipCenter;
                directionUpDown -= Vector3.Project(directionUpDown, Vector3.right);

                Quaternion torsoTilt = Quaternion.FromToRotation(Vector3.up, directionUpDown);
                Quaternion rotationShoulders = Quaternion.FromToRotation(Vector3.right, dirLeftRight);

                torsoTilt.x = -torsoTilt.x;

                Quaternion userOrientation = torsoTilt * rotationShoulders;

                if (rotationalOffset)
                {
                    userOrientation.eulerAngles -= new Vector3(offsetCalculator.rotationalOffset.x, offsetCalculator.rotationalOffset.y, 0);
                    myTransform.rotation = userOrientation;
                }
                else
                {
                    myTransform.rotation = userOrientation;
                }
            }
        }
    }


}
