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
    // Use this for initialization
    void Start () {
        desiredVel = new Vector3(2, 0, 4);
        desiredVel1 = new Vector3(4, 0, 2);
        OGPos = new Vector3(0, 0, 0);
        OGPos1 = new Vector3(0, 0, 0);
    }
	
	// Update is called once per frame
	void Update () {
        if (isServer)
        {
            OGPos += desiredVel;
            OGPos1 += desiredVel1;
            players[0].transform.position = OGPos;
            angleVec = new Vector3(angle, angle, angle);
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
                        Debug.Log(xAngles);
                        //Quaternion tempANG = Quaternion.Euler(new Vector3(0, -tempAng, 0));
                        Quaternion tempANG = Quaternion.Euler(xAngles);
                        //avgVelCalY = tempANG * avgVelCalY;
                        //tempAng = Vector3.Angle(avgUnitY, avgVelCalY);
                        //Debug.Log(tempAng);
                        //Debug.Log(Mathf.Atan(unitCam.avgVel.x / unitCam.avgVel.y)*Mathf.Rad2Deg);
                        //Debug.Log(Mathf.Atan(velCalc.avgVel.x / velCalc.avgVel.y)*Mathf.Rad2Deg);

                        //tempANG = Quaternion.Euler(new Vector3(-tempAng,0,0));
                        avgVelCalY = tempANG * velCalc.avgVel;
                        Debug.Log(avgVelCalY);
                        //avgAngles[i - 1] = Vector3.Angle(unitCam.avgVel, velCalc.avgVel);
                        avgAngles[i - 1] = Vector3.Angle(avgUnitY, avgVelCalY);
                        this.angles = angles;
                        this.avgAngles = avgAngles;
                    }
                }
            }
        }
    }
}
