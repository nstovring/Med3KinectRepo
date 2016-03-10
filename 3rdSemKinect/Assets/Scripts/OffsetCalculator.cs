using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine.UI;

//This class is responsible for calculating offsets on the server, and apply them on the clients
public class OffsetCalculator : NetworkBehaviour {
    //Varibles are defined
    private Vector3 player2Offset;
    private float player2angleOffset;

    //The players array is useed to contain the cubes in the scene
    private GameObject[] players;
    private GameObject[] skeletonCreators;

    private float player1AngleFromKinect;

    //[SyncVar] is an attribute given to variables, which enables them to syncronize from server to client but not the other way around
    [SyncVar] public Vector3 positionalOffset;
    //These two variables are used to give all clients the offsets, both rotational and positional, that are calculated on the server
    [SyncVar] public Vector3 rotationalOffset;

    //This static Offsetcalculator is used to reference a single instance of the offsetcalculator script
    public static OffsetCalculator offsetCalculator;
    bool calcMove;
    public Vector3[] oldCords;
    public Vector3[] vel;
    public float[] angles;
    private int amount;
    public Vector3[] angleSum;
    public Vector3[] avgNewAngles;
    public int kinectAmount;
    bool run_once = true;
    bool continuedRun = false;


    void Start()
    {
        if(kinectAmount != 0)
        {
            angleSum = new Vector3[kinectAmount - 1];
            avgNewAngles = new Vector3[kinectAmount-1];
        }
        //calcMove is used to see if the velocity calibration should be used or if it is the postional vector calibration.
        calcMove = false;
        oldCords = new Vector3[2];
        vel = new Vector3[2];
        angles = new float[1];

        //Here the offsetcalculator variable is set to this instance of the script, making other scripts able to easily get this script
        offsetCalculator = this;
        Button button = GameObject.FindGameObjectWithTag("AngleCalc").GetComponent<Button>();
        button.onClick.AddListener(runSelectedVectorAngles);
    }

    void Update()
    {
        //calls the velocity calibration method
        if (calcMove)
        {
            //realVelocityAngles();
            selectedVectorAngles(new int[3] { 0, 4, 8 });
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
        float[][] angles = new float[players.Length - 1][];
        float[] avgAngles = new float[players.Length - 1];
        if (players.Length >= 2)
        {
            if (unitCam.full)
            {
                for (int i = 1; i < players.Length; i++)
                {
                    velCalc = players[i].GetComponent<VelocityCalculator>();
                    if (velCalc.full)
                    {
                        angles[i - 1] = new float[unitCam.velocities.Length];
                        for (int j = 0; j < unitCam.velocities.Length; j++)
                        {
                            angles[i - 1][j] = Vector3.Angle(unitCam.velocities[j], velCalc.velocities[j]);
                        }
                        avgAngles[i - 1] = Vector3.Angle(unitCam.avgVel, velCalc.avgVel);
                    }
                }
            }
        }
    }
    public void realVelocityAngles()
    {
        skeletonCreators = GameObject.FindGameObjectsWithTag("SkeletonCreator");
        if (skeletonCreators.Length >= 2)
        {
            List<List<int>> commonJoints = new List<List<int>>();
            Vector3[][] vectors = new Vector3[(skeletonCreators.Length-1)*2][];
            foreach (var i in skeletonCreators)
            {
                commonJoints.Add(i.GetComponent<skeletonCreator>().trackedJoints);
            }
            commonJoints = findCommonJoints(commonJoints);
            if (lengthsAreAbove(3, commonJoints))
            {
                for (int i = 0; i < vectors.GetLength(0); i++)
                {
                    GameObject[] skel = skeletonCreators[i].GetComponent<skeletonCreator>().players;
                    vectors[i] = new Vector3[2] { skel[commonJoints[i][1]].transform.position - skel[commonJoints[i][0]].transform.position, skel[commonJoints[i][2]].transform.position - skel[commonJoints[i][0]].transform.position };
                }
                differentVectorAngles(vectors,2);
            }
        }
    }
    public void selectedVectorAngles(int[] jointsWeWant)
    {
        skeletonCreators = GameObject.FindGameObjectsWithTag("SkeletonCreator");
        GameObject[][] allPlayers = new GameObject[skeletonCreators.Length][];
        if (skeletonCreators.Length >= 2)
        {
            List<List<int>> commonJoints = new List<List<int>>();
            if(jointsAreTracked(jointsWeWant, commonJoints))
            {
                Vector3[][] vectors = new Vector3[skeletonCreators.Length][];
                foreach (var i in skeletonCreators)
                {
                    commonJoints.Add(i.GetComponent<skeletonCreator>().trackedJoints);
                }
                commonJoints = findCommonJoints(commonJoints);
                if (lengthsAreAbove(3, commonJoints))
                {
                    for (int i = 0; i < vectors.GetLength(0); i++)
                    {
                        GameObject[] skel = skeletonCreators[i].GetComponent<skeletonCreator>().players;
                        allPlayers[i] = skel;
                        vectors[i] = new Vector3[2] { skel[jointsWeWant[1]].transform.position - skel[jointsWeWant[0]].transform.position, skel[jointsWeWant[2]].transform.position - skel[jointsWeWant[0]].transform.position };
                    }
                    sameVectorAngles(vectors);
                }
                if (amount >= 200 && run_once)
                {
                    getAndSetVectorOffsets(allPlayers, jointsWeWant, skeletonCreators);
                    reset();
                    run_once = false;
                    continuedRun = true;
                }
                if(amount >= 200 && continuedRun)
                {
                    adjustOffsets(allPlayers, jointsWeWant, skeletonCreators);
                    reset();
                    continuedRun = false;
                }
            }

        }
        calcMove = true;
    }
    public void reset()
    {
        angleSum = new Vector3[kinectAmount - 1];
        avgNewAngles = new Vector3[kinectAmount - 1];
        amount = 0;
    }
    public void getAndSetVectorOffsets(GameObject[][] vectorArrays, int[] commonJoints, GameObject[] skeletonCreators)
    {
        Vector3[] positionalOffsets;
        rotationalOffset = avgNewAngles[0];
        Quaternion rotations = Quaternion.Euler(avgNewAngles[0]);
        positionalOffsets = getJointsPosOffset(vectorArrays, commonJoints, rotations, skeletonCreators);
        //positionalOffsets = getJointsPosOffset(skeletonCreators, new Quaternion[] { rotations });
        positionalOffset = positionalOffsets[0];

        for (int i = 1; i < vectorArrays.GetLength(0); i++)
        {
            skeletonCreators[i].transform.GetComponent<UserSyncPosition>().rotationalOffset = true;
            skeletonCreators[i].transform.GetComponent<UserSyncPosition>().positionalOffset = true;
            for (int j = 0; j < vectorArrays[i].Length; j++)
            {
                vectorArrays[i][j].transform.GetComponent<UserSyncPosition>().rotationalOffset = true;
                vectorArrays[i][j].transform.GetComponent<UserSyncPosition>().positionalOffset = true;
            }
        }
    }
    public void adjustOffsets(GameObject[][] vectorArrays, int[] commonJoints, GameObject[] skeletonCreators)
    {
        Vector3[] positionalOffsets;
        //rotationalOffset += avgNewAngles[0];
        Quaternion rotations = Quaternion.Euler(rotationalOffset);
        //positionalOffsets = getJointsPosOffset(vectorArrays, commonJoints, rotations, skeletonCreators);
        positionalOffsets = getJointsPosOffset(skeletonCreators, new Quaternion[] { rotations });
        positionalOffset += positionalOffsets[0];
    }
    public Vector3[] getJointsPosOffset(GameObject[][] vectorArrays, int[] commonJoints, Quaternion angles, GameObject[] skeletonCreators)
    {
        int averageAdjust = 1;
        Vector3[] output = new Vector3[vectorArrays.GetLength(0) - 1];
        for (int i = 1; i < vectorArrays.GetLength(0); i++)
        {
            output[i - 1] = (skeletonCreators[0].transform.position - (angles * skeletonCreators[i].transform.position)) * averageAdjust;
            for (int j = 0; j < commonJoints.Length; j++)
            {
                output[i - 1] += vectorArrays[0][j].transform.position - (angles*vectorArrays[i][j].transform.position);
            }
            output[i - 1] = output[i - 1] / (commonJoints.Length + averageAdjust);
        }
        return output;
    }
    public Vector3[] getJointsPosOffset(GameObject[] skeletonCreators, Quaternion[] angles)
    {
        Vector3[] output = new Vector3[skeletonCreators.Length - 1];
        for (int i = 1; i < skeletonCreators.Length; i++)
        {
            output[i - 1] = (skeletonCreators[0].transform.position - skeletonCreators[i].transform.position);
        }
        return output;
    }
    public void runSelectedVectorAngles()
    {
        selectedVectorAngles(new int[3] { 0, 4, 8});
    }
    bool jointsAreTracked(int[] jointNum, List<List<int>> joints)
    {
        foreach(int i in jointNum)
        {
            if (!jointsAreTracked(i, joints))
            {
                return false;
            }
        }
        return true;
    }
    bool jointsAreTracked(int jointNum, List<List<int>> joints)
    {
        for(int i = 0; i < joints.Count; i++)
        {
            if (!jointsAreTracked(jointNum, joints[i]))
            {
                return false;
            }
        }
        return true;
    }
    bool jointsAreTracked(int jointNum, List<int> joints)
    {
        if (joints.Contains(jointNum))
        {
            return true;
        }
        return false;
    }
    bool lengthsAreAbove(int num, List<List<int>> lengths)
    {
        for (int i = 0; i < lengths.Count; i++)
        {
            if (lengths[i].Count < num)
            {
                return false;
            }
        }
        return true;
    }
    public List<List<int>> findCommonJoints(List<List<int>> joints)
    {

        List<List<int>> output = new List<List<int>>();
        for(int i = 1; i < joints.Count; i++)
        {
            List<int> tempJoints = new List<int>();
            for (int j = 0; j < joints[i].Count; j++)
            {
                    if (joints[0].Contains(joints[i][j]))
                    {
                        tempJoints.Add(joints[i][j]);
                        
                    }
                if(tempJoints.Count >= 3)
                {
                    break;
                }
            }
            output.Add(tempJoints);
        }
        return output;
    }
    public float[][] Times3x3(float[][] m1, float[][] m2)
    {
        float[][] result = new float[3][];
        result[0] = new float[3];
        result[1] = new float[3];
        result[2] = new float[3];
        for (int i = 0; i < result.GetLength(0); i++)
        {
            for (int j = 0; j < result[i].Length; j++)
            {
                result[i][j] = m1[0][j] * m2[i][0] + m1[1][j] * m2[i][1] + m1[2][j] * m2[i][2];

            }
        }
        // Debug.Log("result 0 0 = " + result[1][1]);
        return result;
    }
    public float[][] invert3x3(float[][] m)
    {

        float[][] A = m; //The matrix that is entered from the data.
        //Debug.Log("A " + 0 + " " + 0 + " = " + A[0][0]);
        float[][] B = new float[3][]; //The transpose matrix of A
        float[][] C = new float[3][]; //The adjoint matrix of A adj(A)
        float[][] X = new float[3][];
        B[0] = new float[3]; B[1] = new float[3]; B[2] = new float[3];
        C[0] = new float[3]; C[1] = new float[3]; C[2] = new float[3];
        X[0] = new float[3]; X[1] = new float[3]; X[2] = new float[3];
        //The inverse of A (adj(A)/det)
        float det; //The determinant of A
        // Calculate the determinant of A (det)
        float a = A[0][0] * (A[1][1] * A[2][2] - A[2][1] * A[1][2]);
        //Debug.Log("a = " + a);
        //Debug.Log((A[1][1] * A[2][2] - A[2][1] * A[1][2]));
        float b = A[0][1] * (A[1][0] * A[2][2] - A[2][0] * A[1][2]);
        //Debug.Log("b = " + b);
        float c = A[0][2] * (A[1][0] * A[2][1] - A[2][0] * A[1][1]);
        //Debug.Log("c = " + c);
        det = a - b + c;
        //Debug.Log("det = " + det);
        // Find the transpose matrix (B)/> of A
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {

                B[i][j] = A[j][i];
            }
        }

        //Calculate the adjoint matrix (C) of A
        C[0][0] = B[1][1] * B[2][2] - B[2][1] * B[1][2];
        C[0][1] = -(B[1][0] * B[2][2] - B[2][0] * B[1][2]);
        C[0][2] = B[1][0] * B[2][1] - B[2][0] * B[1][1];
        C[1][0] = -(B[0][1] * B[2][2] - B[2][1] * B[0][2]);
        C[1][1] = B[0][0] * B[2][2] - B[2][0] * B[0][2];
        C[1][2] = -(B[0][0] * B[2][1] - B[2][0] * B[0][1]);
        C[2][0] = B[0][1] * B[1][2] - B[1][1] * B[0][2];
        C[2][1] = -(B[0][0] * B[1][2] - B[1][0] * B[0][2]);
        C[2][2] = B[0][0] * B[1][1] - B[1][0] * B[0][1];
        // Calculate the inverse matrix of A (adj(A)/det)
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {

                X[i][j] = C[i][j] / det;

            }
        }
        return X;
    }
    public Vector3[] differentVectorAngles(Vector3[][] sortedVectors, int num)
    {
        Vector3[] angles = new Vector3[sortedVectors.GetLength(0)/2];
        if (sortedVectors.GetLength(0) >= 2)
        {
            if (allAreFilled(sortedVectors))
            {
                Vector3[] tempAngles = new Vector3[sortedVectors.GetLength(0) - 1];
                Vector3 v3 = Vector3.Cross(sortedVectors[0][0], sortedVectors[0][1]);
                float[][] m1 = convertTo3x3(sortedVectors[0][0], sortedVectors[0][1], v3);
                for (int i = 1; i < sortedVectors.GetLength(0); i++)
                {
                    Vector3 w3 = Vector3.Cross(sortedVectors[i][0], sortedVectors[i][1]);
                    float[][] m2 = convertTo3x3(sortedVectors[i][0], sortedVectors[i][1], w3);
                    float[][] m3 = Times3x3(m2, invert3x3(m1));
                    tempAngles[i - 1] = new Vector3(Mathf.Atan2(m3[2][1], m3[2][2]) * Mathf.Rad2Deg, Mathf.Atan2(-m3[2][0], Mathf.Sqrt(Mathf.Pow(m3[2][1], 2) + Mathf.Pow(m3[2][2], 2))) * Mathf.Rad2Deg, Mathf.Atan2(m3[1][0], m3[0][0]) * Mathf.Rad2Deg);
                }
                amount++;
                for (int i = 0; i < angles.Length; i+= num)
                {
                    angles[i] = moreVectorAngles(new Vector3[][] { sortedVectors[i], sortedVectors[i + 1] }, tempAngles[i].y, 0);
                    angleSum[i] += angles[i];
                    avgNewAngles[i] = angleSum[i] / amount;
                }
            }
        }
        return angles;
    }
    public Vector3[] sameVectorAngles(Vector3[][] sortedVectors)
    {
        Vector3[] angles = new Vector3[sortedVectors.GetLength(0) - 1];
        if (sortedVectors.GetLength(0) >= 2)
        {
            if (allAreFilled(sortedVectors))
            {
                Vector3[] tempAngles = new Vector3[sortedVectors.GetLength(0) - 1];
                Vector3 v3 = Vector3.Cross(sortedVectors[0][0], sortedVectors[0][1]);
                float[][] m1 = convertTo3x3(sortedVectors[0][0], sortedVectors[0][1], v3);
                for (int i = 1; i < sortedVectors.GetLength(0); i++)
                {
                    Vector3 w3 = Vector3.Cross(sortedVectors[i][0], sortedVectors[i][1]);
                    float[][] m2 = convertTo3x3(sortedVectors[i][0], sortedVectors[i][1], w3);
                    float[][] m3 = Times3x3(m2, invert3x3(m1));
                    tempAngles[i - 1] = new Vector3(Mathf.Atan2(m3[2][1], m3[2][2]) * Mathf.Rad2Deg, Mathf.Atan2(-m3[2][0], Mathf.Sqrt(Mathf.Pow(m3[2][1], 2) + Mathf.Pow(m3[2][2], 2))) * Mathf.Rad2Deg, Mathf.Atan2(m3[1][0], m3[0][0]) * Mathf.Rad2Deg);
                }
                amount++;
                for (int i = 0; i < angles.Length; i++)
                {
                    angles[i] = moreVectorAngles(new Vector3[][] { sortedVectors[0], sortedVectors[i + 1] }, tempAngles[i].y, 0);
                    angleSum[i] += angles[i];
                    avgNewAngles[i] = angleSum[i] / amount;
                }
            }
        }
        return angles;
    }

    public Vector3 moreVectorAngles(Vector3[][] sortedVectors, float yAngle, int number)
    {

        //Debug.Log(number);

        Vector3 v3 = Vector3.Cross(sortedVectors[0][0], sortedVectors[0][1]);
        Vector3[] tempArray = timesArray(sortedVectors[1], Quaternion.Euler(Vector3.up * yAngle));
        Vector3 w3 = Vector3.Cross(tempArray[0], tempArray[1]);
        float[][] m1 = convertTo3x3(sortedVectors[0][0], sortedVectors[0][1], v3);
        float[][] m2 = convertTo3x3(tempArray[0], tempArray[1], w3);
        float[][] m3 = Times3x3(m2, invert3x3(m1));
        Vector3 angle = new Vector3(Mathf.Atan2(m3[2][1], m3[2][2]) * Mathf.Rad2Deg, Mathf.Atan2(-m3[2][0], Mathf.Sqrt(Mathf.Pow(m3[2][1], 2) + Mathf.Pow(m3[2][2], 2))) * Mathf.Rad2Deg, Mathf.Atan2(m3[1][0], m3[0][0]) * Mathf.Rad2Deg);
        //float yAngle2 = yAngle + angle.y;
        yAngle += angle.y;
        //Debug.Log(yAngle2);
        //Debug.Log(angle.y);
        if ((angle.y >= -0.00001f && angle.y <= 0.00001f) || number >= 100)
        {

            return new Vector3(angle.x, yAngle, angle.z);
        }

        return moreVectorAngles(sortedVectors, yAngle, number + 1);

    }
    Vector3[] timesArray(Vector3[] vectors, Quaternion angle)
    {
        Vector3[] output = new Vector3[vectors.Length];
        for (int i = 0; i < vectors.Length; i++)
        {
            output[i] = angle * vectors[i];
        }
        return output;
    }
    bool allAreFilled(Vector3[][] array)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            if (array[i].Length < 2)
            {
                return false;
            }
        }
        return true;
    }
    public float[][] convertTo3x3(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3[] vectors = new Vector3[] { v1, v2, v3 };
        float[][] result = new float[3][];
        for (int i = 0; i < result.GetLength(0); i++)
        {
            result[i] = new float[3];
            result[i][0] = vectors[i].x;
            result[i][1] = vectors[i].y;
            result[i][2] = vectors[i].z;
        }
        return result;

    }
    public Vector3 getIntersectionPoint(Vector3 d1, Vector3 d2, Vector3 c)
    {
        float t = 0;
        Vector3 intersectionPoint = Vector3.zero;
        if ((d1.z * d2.x - d1.x * d2.z) != 0)
        {
            t = (c.x * d2.z - c.z * d2.x) / (d1.z * d2.x - d1.x * d2.z);
            intersectionPoint = d1 * t;
            intersectionPoint = new Vector3(intersectionPoint.x, 0, intersectionPoint.z);
        }
        return intersectionPoint;
    }
    public void vectorIntersectionPoint(float angle1, float angle2)
    {
        //replace the parameters with the sound angles from kinects
        if(rotationalOffset.z < 10)
        {
            Quaternion a1 = Quaternion.Euler(0, angle1, 0);
            Quaternion a2 = Quaternion.Euler(0, angle2 + rotationalOffset.y, 0);
            Vector3 d1 = a1 * Vector3.forward;
            Vector3 d2 = a2 * Vector3.forward;
            Vector3 intersectionPoint = getIntersectionPoint(d1, d2, new Vector3(positionalOffset.x, 0, positionalOffset.z));
        }
    }
    public Vector3 findHeight(float depth, float angle)
    {
        Vector3 height = Vector3.zero;
        //angle should be the angle difference
        if(rotationalOffset.z > 5)
        {
            float length = depth / (float)Math.Sin(40);
            Vector3 heightMax = Quaternion.Euler(0, 0, rotationalOffset.z) * new Vector3(length, 0, 0);
            height = angle / 50 * heightMax;
        }
        return height;
    }
    public float soundAngleFromTimeDelay(double deltaT, float length)
    {
        float angle = 0;
        float circumference = (length * 340) * 2;
        float dia = circumference / (float)Math.PI;
        float r = dia / 2;
        float pheta = (float)deltaT / r;
        Vector2 Point = new Vector2((float)Math.Cos(pheta) * r, (float)Math.Sin(pheta) * r);
        float a = Point.y;
        float b1 = dia - (dia - Point.x);
        float b2 = dia - Point.x;
        float c1 = Mathf.Sqrt(Mathf.Pow(b1, 2) + Mathf.Pow(a, 2));
        float c2 = Mathf.Sqrt(Mathf.Pow(b2, 2) + Mathf.Pow(a, 2));
        float A1 = Mathf.Atan2(b1, a);
        angle = 90 - A1;
        return angle;
    }
}
