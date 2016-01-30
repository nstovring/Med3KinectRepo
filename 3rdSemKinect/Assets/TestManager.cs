using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TestManager : NetworkBehaviour {

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
    // Use this for initialization
    void Start () {
        desiredVel = new Vector3(2, 0, 4);
        desiredVel1 = new Vector3(4, 0, 2);
        OGPos = new Vector3(0, 0, 0);
        OGPos1 = new Vector3(0, 0, 0);
        solForY = new Vector3[3];
        point1 = new Vector3(1, 1, 1);
        point2= new Vector3(5, 1, 1);
        point3 = new Vector3(10, 1, 1);
        planeAng = new Vector3[4][];
    }
	
	// Update is called once per frame
	void Update () {
        if (isServer)
        {
            OGPos += desiredVel;
            OGPos1 += desiredVel1;
            players[0].transform.position = OGPos;
            angleVec = new Vector3(angle,angle, angle);
            QAngles = Quaternion.Euler(angleVec);
            players[1].transform.position = QAngles * (OGPos);
            players[2].transform.position = QAngles * (OGPos1);
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
            VelocityAngles();
        }
	}
    public void VelocityAngles() //new method using the velocity calculator information
                                 //THIS SHOULD ONLY BE CALLED WHEN THERE IS ONE PERSON IN THE SCENE
    {
        //players = GameObject.FindGameObjectsWithTag("Player");
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
                        Vector3 avgUnitZ = new Vector3(unitCam.avgVel.x, unitCam.avgVel.y,0);
                        Vector3 avgVelCalZ = new Vector3(velCalc.avgVel.x, velCalc.avgVel.y,0);
                        float tempAng = Vector3.Angle(avgUnitX, avgVelCalX);
                        Vector3 xAngles = new Vector3(-Vector3.Angle(avgUnitX, avgVelCalX),0, 0);
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
                avgAngles[0] = Vector3.Angle(new Vector3( 0,solForY[0].y, solForY[0].z), Vector3.forward);
                avgAngles[1] = Vector3.Angle(new Vector3(0, solForY[1].y, solForY[1].z), Vector3.forward);
                planeAng[0] = new Vector3[planeAng.GetLength(0) + 2];
                for(int i = 0; i < planeAng[0].Length; i++)
                {
                    point1 = new Vector3(1+5*i, 1, 1);
                    planeAng[0][i] = solveForY(n, 0, point1);
                }
                planeAng[1] = new Vector3[planeAng.GetLength(0) + 2];
                for (int i = 0; i < planeAng[1].Length; i++)
                {
                    point1 = new Vector3(1, 1, 1+5*i);
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
                for(int i = 0; i < planeAng.GetLength(0); i++)
                {
                    for(int j = 1; j < planeAng[i].Length; j++)
                    {
                        planeAngles[(i * planeAng[0].Length + j)-1] = i == 0 ? Vector3.Angle(planeAng[i][j]- planeAng[i][j-1], Vector3.right) : i == 1 ? Vector3.Angle(planeAng[i][j] - planeAng[i][j - 1], Vector3.forward) : i == 2 ? Vector3.Angle(planeAng[i][j] - planeAng[i][j - 1], Vector3.forward) : i == 3 ? Vector3.Angle(planeAng[i][j] - planeAng[i][j - 1], Vector3.forward) : 0;
                    }
                }
                //avgAngles[0] = Vector3.Angle(planeAng[1][2]- planeAng[1][1], Vector3.forward);
                for (int i = 0; i < planeAng[1].Length; i++)
                {
                    Debug.Log(planeAng[2][i]);
                }
            }
        }
    }
    
    public Vector3 solveForY(Vector3 equation, int equals, Vector3 point)
    {
        float result = (equals-(point.x * equation.x + point.z*equation.z))/equation.y;
        return new Vector3(point.x, result, point.z);
    }
    public Vector3 solveForX(Vector3 equation, int equals, Vector3 point)
    {
        float result = (equals - (point.y * equation.y + point.z * equation.z)) / equation.x;
        return new Vector3(result,point.y, point.z);
    }
    public Vector3 solveForZ(Vector3 equation, int equals, Vector3 point)
    {
        float result = (equals - (point.x * equation.x + point.y * equation.y)) / equation.z;
        return new Vector3(point.x, point.y, result);
    }
}
