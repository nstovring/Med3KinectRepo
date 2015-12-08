using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Xml.Serialization;

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


    public void CalculateOffset()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

       if (players.Length >= 2)
       {
           positionalOffset = GetPositionOffset();
           rotationalOffset = GetRotationOffset();
       }
    }

    public Vector3 GetPositionOffset()
    {
        return (players[0].transform.position - players[1].transform.position);
    }

    public Vector3 GetRotationOffset()
    {
        return new Vector3(
          Vector3.Angle(players[0].transform.up, players[1].transform.up),
          Vector3.Angle(players[0].transform.forward, players[1].transform.forward),
          Vector3.Angle(players[0].transform.right, players[1].transform.right));
    }

    public void ApplyOffset()
    {
        players[1].transform.GetComponent<UserSyncPosition>().Offset = true;
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
