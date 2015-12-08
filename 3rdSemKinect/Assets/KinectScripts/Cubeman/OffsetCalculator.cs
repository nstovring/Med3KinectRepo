using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class OffsetCalculator : NetworkBehaviour {

    private Vector3 player2Offset;
    private float player2angleOffset;

    private GameObject[] players;
    private float player1AngleFromKinect;
    [SyncVar] public Vector3 positionalOffset;
    [SyncVar] public Vector3 rotationalOffset;

    public  static OffsetCalculator offsetCalculator;

	void Start ()
	{
	    offsetCalculator = this;
	}


	// Update is called once per frame
    [Server]
	void Update () {
        /*players = GameObject.FindGameObjectsWithTag("Player");

        if (this.players.Length >= 2) {
            player2Offset = players[0].transform.position - players[1].transform.position;
            player1AngleFromKinect = Mathf.Abs(players[0].transform.GetComponent<CubeController>().angleFromKinect);

            CubeController player2Controller = players[1].transform.GetComponent<CubeController>();

            player2angleOffset = player1AngleFromKinect + Mathf.Abs(player2Controller.angleFromKinect) + Mathf.Abs(player2Controller.angleBetweenKinects);
            SetPositionOffset();
            SetRotationOffset();
        }*/
	}

    public void CalculateOffset()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        /*if (this.players.Length >= 2)
        {
            player2Offset = GetPositionOffset();
            player1AngleFromKinect = Mathf.Abs(players[0].transform.GetComponent<CubeController>().angleFromKinect);

            CubeController player2Controller = players[1].transform.GetComponent<CubeController>();

            player2angleOffset = player1AngleFromKinect + Mathf.Abs(player2Controller.angleFromKinect) + Mathf.Abs(player2Controller.angleBetweenKinects);
        }*/
    }

    public Vector3 GetPositionOffset()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        positionalOffset = (players[0].transform.position - players[1].transform.position);
        return positionalOffset;
    }

    public Vector3 GetRotationOffset()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        rotationalOffset = new Vector3(
          Vector3.Angle(players[0].transform.right, players[1].transform.right),
          Vector3.Angle(players[0].transform.up, players[1].transform.up),
          Vector3.Angle(players[0].transform.forward, players[1].transform.forward));
        return rotationalOffset;
    }


    [Server]
    private void SetRotationOffset()
    {
        //Debug.Log(players[0].transform.forward  +" & "+ players[1].transform.forward);
        //Debug.Log(Vector3.Angle(players[0].transform.forward, players[1].transform.forward));

       // players[1].GetComponent<CubeController>().tempRotation = Vector3.Angle(players[0].transform.forward,
          //players[1].transform.forward);
    }
    [Server]
    private void SetPositionOffset()
    {
       // players[1].GetComponent<CubeController>().positionOffset = this.player2Offset;
       // players[1].GetComponent<CubeController>().otherAngleFromKinect = this.player1AngleFromKinect;
    }

}
