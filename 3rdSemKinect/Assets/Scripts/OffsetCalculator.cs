using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

//This class is responsible for calculating offsets on the server, and apply them on the clients
public class OffsetCalculator : NetworkBehaviour {
    
    //Varibles are defined
    private Vector3 player2Offset;
    private float player2angleOffset;

    //The players array is useed to contain the cubes in the scene
    private GameObject[] players;

    private float player1AngleFromKinect;

    //[SyncVar] is an attribute given to variables, which enables them to syncronize from server to client but not the other way around
    [SyncVar] public Vector3 positionalOffset;
    //These two variables are used to give all clients the offsets, both rotational and positional, that are calculated on the server
    [SyncVar] public Vector3 rotationalOffset;

    //This static Offsetcalculator is used to reference a single instance of the offsetcalculator script
    public  static OffsetCalculator offsetCalculator;
    bool calcMove;

	void Start ()
	{
        //calcMove is used to see if the velocity calibration should be used or if it is the postional vector calibration.
        calcMove = false;
        oldCords = new Vector3[2];
        vel = new Vector3[2];
        angles = new float[1];

        //Here the offsetcalculator variable is set to this instance of the script, making other scripts able to easily get this script
        offsetCalculator = this;
        
	}

    void Update()
    {
        //calls the velocity calibration method
        if (calcMove)
        {
            MovementDiff();
        }
    }
    //This method is used if the offsets have already been calculated and been saved in the file PlayerPrefs which is inheriant to Unity
    public void GetPositionalValuesFromPlayerPrefs()
    {
        //the offsetvalues are read from the PlayerPrefs file
        Vector3 offsetPosVector3 = new Vector3(PlayerPrefs.GetFloat("PositionalOffsetX"), PlayerPrefs.GetFloat("PositionalOffsetY"), PlayerPrefs.GetFloat("PositionalOffsetZ"));
        Vector3 offsetRotVector3 = new Vector3(PlayerPrefs.GetFloat("RotationalOffsetX"), PlayerPrefs.GetFloat("RotationalOffsetY"), PlayerPrefs.GetFloat("RotationalOffsetZ"));

        //the values that have been read are applied to the offsetvariables used by the rest of the script
        rotationalOffset = offsetRotVector3;
        positionalOffset = offsetPosVector3;
    }

    //this method is used when the offset should be calculated
    public void CalculateOffset()
    {
        //all objects with the tag "Player" are put into the players GameObject array
        players = GameObject.FindGameObjectsWithTag("Player");

        //the code must only run if there are more than two cubes in the scene, as you can't calculate offset on a single cube
        if (players.Length >= 2)
       {
            //the offsets are calculated using different methods that use the player array
            positionalOffset = GetPositionOffset();
            rotationalOffset = GetRotationOffset();
            //the offsets, both positional and rotational, are saved in the PlayerPrefabs, to be used by other methods and scripts
            PlayerPrefs.SetFloat("PositionalOffsetX", (positionalOffset.x));
            PlayerPrefs.SetFloat("PositionalOffsetY", (positionalOffset.y));
            PlayerPrefs.SetFloat("PositionalOffsetZ", (positionalOffset.z));
            PlayerPrefs.SetFloat("RotationalOffsetX", (rotationalOffset.x));
            PlayerPrefs.SetFloat("RotationalOffsetY", (rotationalOffset.y));
            PlayerPrefs.SetFloat("RotationalOffsetZ", (rotationalOffset.z));
            //Two booleans on the clients cube object are set to true, and the client will now begin applying the calculated offsets every frame
            players[1].transform.GetComponent<UserSyncPosition>().rotationalOffset = true;
            players[1].transform.GetComponent<UserSyncPosition>().positionalOffset = true;

        }
    }
    //this method calculates the positional offset between the cubes
    public Vector3 GetPositionOffset()
    {
        //it calculates the offset by simply subtracting one's position from the other's
        //this method is only accurate if the rotational offset has already been applied
        return (players[0].transform.position - players[1].transform.position);
    }

    //calculates the rotational offset between the cubes
    public Vector3 GetRotationOffset()
    {
        //returns a angle vector by taking the rotation of each cube and subtracting them from each other, in each axis
        //this is only possible when the cubes automatically rotate with the user
        return new Vector3(
          Vector3.Angle(players[0].transform.up, players[1].transform.up),
          Vector3.Angle(players[0].transform.forward, players[1].transform.forward),
          Vector3.Angle(players[0].transform.right, players[1].transform.right));
    }
    //This method calculates the positional offset
    public void ApplyPositionalOffset()
    {
        //all objects with the tag "Player" are put into the players GameObject array
        players = GameObject.FindGameObjectsWithTag("Player");
        //the positional offset is set to the result of the method GetPositionOffset()
        positionalOffset = GetPositionOffset();
        //writes the positional offset coordinates to the PlayerPrefs file
        PlayerPrefs.SetFloat("PositionalOffsetX", (positionalOffset.x));
        PlayerPrefs.SetFloat("PositionalOffsetY", (positionalOffset.y));
        PlayerPrefs.SetFloat("PositionalOffsetZ", (positionalOffset.z));
        //sets the boolean to true, thereby telling the client cube to apply the offset every frame
        players[1].transform.GetComponent<UserSyncPosition>().positionalOffset = true;
    }
    //does the same as ApplyPositionalOffset() but with angles instead
    public void ApplyRotationalOffset()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        rotationalOffset = GetRotationOffset();
        PlayerPrefs.SetFloat("RotationalOffsetX", (rotationalOffset.x));
        PlayerPrefs.SetFloat("RotationalOffsetY", (rotationalOffset.y));
        PlayerPrefs.SetFloat("RotationalOffsetZ", (rotationalOffset.z));
        players[1].transform.GetComponent<UserSyncPosition>().rotationalOffset = true;
    }

    //defining variables that are only used by the MovementDiff() method, which is the velocity calibration method
    public Vector3[] oldCords;
    public Vector3[] vel;
    public float[] angles;

    //the method calculates the difference in velocity of two cubes, and thereby the angle bewteen kinects
    public void MovementDiff()
    {
        //all objects with the tag "Player" are put into the players GameObject array
        players = GameObject.FindGameObjectsWithTag("Player");

        //checks if players has more than one cube, as an offset cannot be calculated with only one cube
        if (players.Length >= 2)
        {
            //runs through the player array
            for (int i = 0; i < players.Length; i++)
            {
                //the vel[i] is set to the difference between the cube's position last frame and the 
                //current position, which will give how much it has moved in the three axis
                vel[i] = oldCords[i] - players[i].transform.position;
                //the oldcords is set to the current cords, so that they can be used next frame
                oldCords[i] = players[i].transform.position;
            }
            //checks if the movement is due to an error or if it is a legitimate movement
            if (vel[0].magnitude > 0.2)
            {
                //runs through the vel array
                for (int i = 1; i < vel.Length; i++)
                {
                    //sets a value in the angles array to the angle between the first cube and cube[i]
                    angles[i - 1] = Vector3.Angle(vel[0], vel[i]);
                }
                //sets calcmove to true so the method will run every frame in update.
                calcMove = true;
            }
        }
    }
    public void VelocityAngles() //new method using the velocity calculator information
        //THIS SHOULD ONLY BE CALLED WHEN THERE IS ONE PERSON IN THE SCENE
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        VelocityCalculator unitCam = players[0].GetComponent<VelocityCalculator>();
        VelocityCalculator velCalc;
        float[][] angles = new float[players.Length-1][];
        float[] avgAngles = new float[players.Length-1];
        if (players.Length >= 2)
        {
            if (unitCam.full)
            {
                for(int i = 1; i < players.Length; i++)
                {
                    velCalc = players[i].GetComponent<VelocityCalculator>();
                    if (velCalc.full)
                    {
                        angles[i - 1] = new float[unitCam.velocities.Length];
                        for(int j = 0; j < unitCam.velocities.Length; j++)
                        {
                            angles[i - 1][j] = Vector3.Angle(unitCam.velocities[j], velCalc.velocities[j]);
                        }
                        avgAngles[i-1] = Vector3.Angle(unitCam.avgVel, velCalc.avgVel);
                    }
                }
            }
        }
    }
}
