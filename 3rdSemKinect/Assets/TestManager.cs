using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TestManager : NetworkBehaviour
{

    public GameObject[] players;
    public float angle;
    Vector3 OGPos;
    Vector3 OGPos1;
    public float[][] angles;
    public float[] avgAngles;
    Vector3 desiredVel;
    Vector3 desiredVel1;
    Vector3 randVec;
    Vector3 angleVec;
    Quaternion QAnglesX;
    Quaternion QAnglesY;
    Quaternion QAnglesZ;
    Quaternion QAngles;
    public Vector3 n;
    public Vector3 point1;
    Vector3 point2;
    Vector3 point3;
    public Vector3[] solForY;
    public float[] planeAngles;
    Vector3[][] planeAng;
    public Vector3 xAngle;
    public Vector3 finalAngle;
    public Vector3[] newAngles;
    Vector3[] angleSum;
    public Vector3[] avgNewAngles;
    int amount;
    Vector3 sum;
    public Vector3 finalFinalAngle;
    public Vector3 MathsSolution;
    int num;
    int num1;
    public Vector3[] moreAngles;
    // Use this for initialization
    void Start()
    {
        amount = 0;

        avgNewAngles = new Vector3[1];
        angleSum = new Vector3[1];
        sum = Vector3.zero;
        num = 0;
        num1 = 0;
        MathsSolution = Vector3.zero;
        desiredVel = new Vector3(2, 1, 4);
        desiredVel1 = new Vector3(4, 1, 2);
        OGPos = new Vector3(0, 0, 0);
        OGPos1 = new Vector3(0, 0, 0);
        solForY = new Vector3[3];
        point1 = new Vector3(1, 1, 1);
        point2 = new Vector3(5, 1, 1);
        point3 = new Vector3(10, 1, 1);
        planeAng = new Vector3[4][];
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {

            OGPos += desiredVel;
            OGPos1 += desiredVel1;
            players[0].transform.position = OGPos;
            players[1].transform.position = OGPos1;
            angleVec = new Vector3(angle, angle, angle);
            QAngles = Quaternion.Euler(angleVec);
            Debug.Log(QAngles);
            players[2].transform.position = QAngles * (OGPos);
            players[3].transform.position = QAngles * (OGPos1);
            finalAngle = Vector3.zero;
            if (num < 0)
            {
                if (num == 5)
                {
                    Debug.Log(players[2].GetComponent<VelocityCalculator>().avgVel);
                    Debug.Log(players[3].GetComponent<VelocityCalculator>().avgVel);
                }
                Quaternion realAngles = Quaternion.Euler(MathsSolution);
                players[2].transform.position = realAngles * players[2].transform.position;
                players[3].transform.position = realAngles * players[3].transform.position;

            }
            /*for (int i = 1; i < players.Length; i++)
            {
                angleVec = new Vector3(angle, angle, angle);
               /* QAnglesX = Quaternion.AngleAxis(angleVec.x, Vector3.left);
                QAnglesX = Quaternion.AngleAxis(angleVec.y, Vector3.up);
                QAnglesX = Quaternion.AngleAxis(angleVec.z, Vector3.forward);
                QAngles = QAnglesX*QAnglesY*QAnglesZ;/
                QAngles = Quaternion.Euler(angleVec);
                randVec = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                players[i].transform.position = QAngles*(OGPos + new Vector3(1000*(i-1), 0, 1000 * (i - 1)));
            }*/
            //VelocityAngles();
            /*if(num < 5 || num > 15)
            {
                VectorMathsSolution();
            }
            */
            /*if (players[0].GetComponent<VelocityCalculator>().full)
            {
                newAngles = VectorAngles(new Vector3[][] { new Vector3[] { players[0].GetComponent<VelocityCalculator>().avgVel, players[1].GetComponent<VelocityCalculator>().avgVel }, new Vector3[] { players[2].GetComponent<VelocityCalculator>().avgVel, players[3].GetComponent<VelocityCalculator>().avgVel } });
            }*/
            //VectorMathsSolution();
            //finalFinalAngle = Mathf.Rad2Deg*Quaternion.ToEulerAngles(Quaternion.FromToRotation(players[0].GetComponent<VelocityCalculator>().avgVel, players[2].GetComponent<VelocityCalculator>().avgVel));
        }
    }
    public Vector3 randomVector(float range)
    {
        return new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
    }
    public void VelocityAngles() //new method using the velocity calculator information
                                 //THIS SHOULD ONLY BE CALLED WHEN THERE IS ONE PERSON IN THE SCENE
    {
        //players = GameObject.FindGameObjectsWithTag("Player");
        VelocityCalculator unitCam = players[0].GetComponent<VelocityCalculator>();
        Debug.Log(players[0].name);
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
                    Debug.Log(players[i].name);
                    if (velCalc.full)
                    {
                        angles[i - 1] = new float[unitCam.velocities.Length];
                        for (int j = 0; j < unitCam.velocities.Length; j++)
                        {
                            Vector3 UnitX = new Vector3(unitCam.velocities[j].x, 0, unitCam.velocities[j].z);
                            Vector3 VelCalX = new Vector3(velCalc.velocities[j].x, 0, unitCam.velocities[j].z);
                            angles[i - 1][j] = Vector3.Angle(UnitX, VelCalX);
                        }
                        Vector3 avgUnitX = new Vector3(0, unitCam.avgVel.y, unitCam.avgVel.z);
                        Vector3 avgVelCalX = new Vector3(0, velCalc.avgVel.y, velCalc.avgVel.z);
                        //Vector3 avgUnitX = new Vector3(0, unitCam.avgVel.y, 1);
                        //Vector3 avgVelCalX = new Vector3(0, velCalc.avgVel.y, 1);
                        Vector3 avgUnitY = new Vector3(unitCam.avgVel.x, 0, unitCam.avgVel.z);
                        Vector3 avgVelCalY = new Vector3(velCalc.avgVel.x, 0, velCalc.avgVel.z);
                        Vector3 avgUnitZ = new Vector3(unitCam.avgVel.x, unitCam.avgVel.y, 0);
                        Vector3 avgVelCalZ = new Vector3(velCalc.avgVel.x, velCalc.avgVel.y, 0);
                        float tempAng = Vector3.Angle(avgUnitX, avgVelCalX);
                        Vector3 xAngles = new Vector3(-Vector3.Angle(avgUnitX, avgVelCalX), 0, 0);
                        //Debug.Log(xAngles);
                        //Quaternion tempANG = Quaternion.Euler(new Vector3(0, -tempAng, 0));
                        Quaternion tempANG = Quaternion.Euler(xAngles);
                        //avgVelCalY = tempANG * avgVelCalY;
                        //tempAng = Vector3.Angle(avgUnitY, avgVelCalY);
                        //Debug.Log(tempAng);
                        //Debug.Log(Mathf.Atan(unitCam.avgVel.x / unitCam.avgVel.y)*Mathf.Rad2Deg);
                        //Debug.Log(Mathf.Atan(velCalc.avgVel.x / velCalc.avgVel.y)*Mathf.Rad2Deg);
                        //tempANG = Quaternion.Euler(new Vector3(-tempAng,0,0));
                        //avgVelCalY = tempANG * velCalc.avgVel;
                        //Debug.Log(avgVelCalY);
                        //avgAngles[i - 1] = Vector3.Angle(unitCam.avgVel, velCalc.avgVel);
                        avgAngles[i - 1] = Vector3.Angle(avgUnitY, avgVelCalY);
                        this.angles = angles;
                        this.avgAngles = avgAngles;
                    }
                }
                VelocityCalculator velCalc1 = players[1].GetComponent<VelocityCalculator>();
                VelocityCalculator velCalc2 = players[2].GetComponent<VelocityCalculator>();
                n = Vector3.Cross(velCalc1.avgVel, velCalc2.avgVel);
                /*point1 = new Vector3(1,0,1);
                point2 = new Vector3(n.x, 0, 10);
                point3 = new Vector3(10, 0, n.z);*/
                solForY[0] = solveForY(n, 0, point1);
                solForY[1] = solveForY(n, 0, point2);
                solForY[2] = solveForY(n, 0, point3);
                avgAngles[0] = Vector3.Angle(new Vector3(0, solForY[0].y, solForY[0].z), Vector3.forward);
                avgAngles[1] = Vector3.Angle(new Vector3(0, solForY[1].y, solForY[1].z), Vector3.forward);
                planeAng[0] = new Vector3[planeAng.GetLength(0) + 2];
                for (int i = 0; i < planeAng[0].Length; i++)
                {
                    point1 = new Vector3(1 + 1 * i, 1, 1);
                    planeAng[0][i] = solveForY(n, 0, point1);
                }
                planeAng[1] = new Vector3[planeAng.GetLength(0) + 2];
                for (int i = 0; i < planeAng[1].Length; i++)
                {
                    point1 = new Vector3(1, 1, 1 + 1 * i);
                    planeAng[1][i] = solveForY(n, 0, point1);
                }
                planeAng[2] = new Vector3[planeAng.GetLength(0) + 2];
                for (int i = 0; i < planeAng[2].Length; i++)
                {
                    point1 = new Vector3(1, 1, 1 + 5 * i);
                    planeAng[2][i] = solveForX(n, 0, point1);
                    //planeAng[2][i] = new Vector3(planeAng[2][i].x, 0, planeAng[2][i].z);
                }
                planeAng[3] = new Vector3[planeAng.GetLength(0) + 2];
                for (int i = 0; i < planeAng[2].Length; i++)
                {
                    point1 = new Vector3(1 + 5 * i, 1, 1);
                    planeAng[3][i] = solveForZ(n, 0, point1);
                    //planeAng[3][i] = new Vector3(planeAng[3][i].x, 0, planeAng[3][i].z);
                }
                planeAngles = new float[planeAng[0].Length * planeAng.GetLength(0)];
                for (int i = 0; i < planeAng.GetLength(0); i++)
                {
                    for (int j = 1; j < planeAng[i].Length; j++)
                    {
                        planeAngles[(i * planeAng[0].Length + j) - 1] = i == 0 ? Vector3.Angle(planeAng[i][j] - planeAng[i][j - 1], Vector3.right) : i == 1 ? Vector3.Angle(planeAng[i][j] - planeAng[i][j - 1], Vector3.forward) : i == 2 ? Vector3.Angle(planeAng[i][j] - planeAng[i][j - 1], Vector3.forward) : i == 3 ? Vector3.Angle(planeAng[i][j] - planeAng[i][j - 1], Vector3.forward) : 0;
                        //planeAngles[(i * planeAng[0].Length + j) - 1] = i == 0 ? Vector3.Angle(planeAng[i][j], Vector3.forward) : i == 1 ? Vector3.Angle(planeAng[i][j], Vector3.right) : i == 2 ? Vector3.Angle(planeAng[i][j] - planeAng[i][j - 1], Vector3.forward) : i == 3 ? Vector3.Angle(planeAng[i][j] - planeAng[i][j - 1], Vector3.forward) : 0;
                    }
                }
                //avgAngles[0] = Vector3.Angle(planeAng[1][2]- planeAng[1][1], Vector3.forward);
                for (int i = 0; i < planeAng[1].Length; i++)
                {
                    //Debug.Log(planeAng[2][i]);
                }
            }
        }
    }

    public Vector3 solveForY(Vector3 equation, int equals, Vector3 point)
    {
        float result = (equals - (point.x * equation.x + point.z * equation.z)) / equation.y;
        return new Vector3(point.x, result, point.z);
    }
    public Vector3 solveForX(Vector3 equation, int equals, Vector3 point)
    {
        float result = (equals - (point.y * equation.y + point.z * equation.z)) / equation.x;
        return new Vector3(result, point.y, point.z);
    }
    public Vector3 solveForZ(Vector3 equation, int equals, Vector3 point)
    {
        float result = (equals - (point.x * equation.x + point.y * equation.y)) / equation.z;
        return new Vector3(point.x, point.y, result);
    }
    public void VectorMathsSolution()
    {
        VelocityCalculator unitCam1 = players[0].GetComponent<VelocityCalculator>();
        Vector3[] velocities = new Vector3[players.Length];
        Vector3 v1;
        Vector3 v2;
        Vector3 v3;
        Vector3 w1;
        Vector3 w2;
        Vector3 w3;
        if (unitCam1.full)
        {
            for (int i = 0; i < players.Length; i++)
            {
                velocities[i] = players[i].GetComponent<VelocityCalculator>().avgVel;
            }

            v1 = velocities[0];
            v2 = velocities[1];
            v3 = Vector3.Cross(v1, v2);
            w1 = velocities[2];
            w2 = velocities[3];
            w3 = Vector3.Cross(w1, w2);
            //Debug.Log("v1 and v2 cross = " + v3);
            //Debug.Log("w1 and w2 cross = " + w3);
            float[][] m1 = convertTo3x3(v1, v2, v3);
            float[][] m2 = convertTo3x3(w1, w2, w3);
            float[][] m3 = Times3x3(m2, invert3x3(m1));
            //m3 = invert3x3(m3);
            if (num == 5)
            {
                Debug.Log("[" + m3[0][0] + "]   " + "[" + m3[0][1] + "]   " + "[" + m3[0][2] + "]   ");
                Debug.Log("[" + m3[1][0] + "]   " + "[" + m3[1][1] + "]   " + "[" + m3[1][2] + "]   ");
                Debug.Log("[" + m3[2][0] + "]   " + "[" + m3[2][1] + "]   " + "[" + m3[2][2] + "]   ");
            }
            /*Debug.Log("m1 = "+m1[0][0]);
            Debug.Log("m2 = " + m2[0][0]);
            Debug.Log("m3 = " + m3[0][0]);*/

            MathsSolution.x = Mathf.Atan2(m3[2][1], m3[2][2]) * Mathf.Rad2Deg;
            MathsSolution.y = Mathf.Atan2(-m3[2][0], Mathf.Sqrt(Mathf.Pow(m3[2][1], 2) + Mathf.Pow(m3[2][2], 2))) * Mathf.Rad2Deg;
            MathsSolution.z = Mathf.Atan2(m3[1][0], m3[0][0]) * Mathf.Rad2Deg;
            moreAngles = new Vector3[225];
            xAngle = Vector3.up * MathsSolution.y;
            //xAngle = MathsSolution;
            for (int i = 0; i < moreAngles.Length; i++)
            {

                Quaternion timingX = Quaternion.Euler(xAngle);
                Vector3 q1 = timingX * w1;
                Vector3 q2 = timingX * w2;
                Vector3 q3 = Vector3.Cross(q1, q2);
                m2 = convertTo3x3(q1, q2, q3);
                m3 = Times3x3(m2, invert3x3(m1));
                moreAngles[i] = new Vector3(Mathf.Atan2(m3[2][1], m3[2][2]) * Mathf.Rad2Deg, Mathf.Atan2(-m3[2][0], Mathf.Sqrt(Mathf.Pow(m3[2][1], 2) + Mathf.Pow(m3[2][2], 2))) * Mathf.Rad2Deg, Mathf.Atan2(m3[1][0], m3[0][0]) * Mathf.Rad2Deg);
                xAngle += Vector3.up * moreAngles[i].y;
                //xAngle += moreAngles[i];
            }
            finalAngle = moreAngles[moreAngles.Length - 1] + xAngle;
            num++;
            sum += finalAngle;
            finalFinalAngle = sum / num;
        }

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
    public Vector3[] VectorAngles(Vector3[][] sortedVectors)
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

        Debug.Log(number);

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
}