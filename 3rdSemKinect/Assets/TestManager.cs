using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TestManager : NetworkBehaviour {

    public GameObject[] players;
    public float angle;
    Vector3 OGPos;
    public float[][] angles;
    public float[] avgAngles;
    Vector3 desiredVel;
    Vector3 randVec;
    Vector3 angleVec;
    Quaternion QAnglesX;
    Quaternion QAnglesY;
    Quaternion QAnglesZ;
    Quaternion QAngles;
    // Use this for initialization
    void Start () {
        desiredVel = new Vector3(2, 2, 2);
        OGPos = new Vector3(0, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
        if (isServer)
        {
            OGPos += desiredVel;
            for (int i = 0; i < players.Length; i++)
            {
                angleVec = new Vector3(10f*i,10f*i, 10f*i);
                QAnglesX = Quaternion.AngleAxis(angleVec.x, Vector3.left);
                QAnglesX = Quaternion.AngleAxis(angleVec.y, Vector3.up);
                QAnglesX = Quaternion.AngleAxis(angleVec.z, Vector3.forward);
                QAngles = QAnglesX*QAnglesY*QAnglesZ;
                //QAngles = Quaternion.Euler(angleVec);
                randVec = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                players[i].transform.position = QAngles*(OGPos);
            }
            VelocityAngles();
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
                        this.angles = angles;
                        this.avgAngles = avgAngles;
                    }
                }
            }
        }
    }
}
