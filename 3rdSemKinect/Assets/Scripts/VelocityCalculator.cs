using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class VelocityCalculator : NetworkBehaviour {

    Vector3[] positions;
    public Vector3[] velocities;
    int frames;
    Vector3 standard;
    int frameCounter;
    public bool full;
    public Vector3 avgVel;
	// Use this for initialization
	void Start () {
        full = false;
        frameCounter = 0;
        //how many frames we want to calculate over
        frames = 20;
        positions = new Vector3[frames];
        velocities = new Vector3[frames-1];
        standard = new Vector3(50, 50, 50);
	}
	
	// Update is called once per frame
	void Update () {
        if (isServer)
        {
            //if(gameObject.transform.position.magnitude < standard.magnitude)
            if(true)
            {
                calcVel();
                if(frameCounter < frames)
                {
                    frameCounter++;
                    if(frameCounter == frames)
                    {
                        full = true;
                    }
                }
            }
            else
            {
                positions = new Vector3[frames];
                velocities = new Vector3[frames-1];
                frameCounter = 0;
                full = false;
            }
        }
	}
    private void calcVel()
    {
        Vector3 temp = new Vector3(0, 0, 0);
        positions[0] = gameObject.transform.position;
        for(int i = frames-1; i >= 1; i--)
        {
            velocities[i - 1] = positions[i-1] - positions[i];
        }
        for (int i = frames - 1; i >= 1; i--)
        {
            positions[i] = positions[i - 1];
        }
        for(int i = 0; i < velocities.Length; i++)
        {
            temp += velocities[i];
        }
        avgVel = temp / velocities.Length;
    }
}
