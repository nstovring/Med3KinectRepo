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

    public void GetPositionalValuesFromPlayerPrefs()
    {
        Vector3 offsetPosVector3 = new Vector3(PlayerPrefs.GetFloat("PositionalOffsetX"), PlayerPrefs.GetFloat("PositionalOffsetY"), PlayerPrefs.GetFloat("PositionalOffsetZ"));
        Vector3 offsetRotVector3 = new Vector3(PlayerPrefs.GetFloat("RotationalOffsetX"), PlayerPrefs.GetFloat("RotationalOffsetY"), PlayerPrefs.GetFloat("RotationalOffsetZ"));

        rotationalOffset = offsetRotVector3;
        positionalOffset = offsetPosVector3;
    }

    public void CalculateOffset()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length >= 2)
       {
            positionalOffset = GetPositionOffset();
            rotationalOffset = GetRotationOffset();
            PlayerPrefs.SetFloat("PositionalOffsetX", (positionalOffset.x));
            PlayerPrefs.SetFloat("PositionalOffsetY", (positionalOffset.y));
            PlayerPrefs.SetFloat("PositionalOffsetZ", (positionalOffset.z));
            PlayerPrefs.SetFloat("RotationalOffsetX", (rotationalOffset.x));
            PlayerPrefs.SetFloat("RotationalOffsetY", (rotationalOffset.y));
            PlayerPrefs.SetFloat("RotationalOffsetZ", (rotationalOffset.z));
            players[1].transform.GetComponent<UserSyncPosition>().rotationalOffset = true;
            players[1].transform.GetComponent<UserSyncPosition>().positionalOffset = true;

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

    public void ApplyPositionalOffset()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        positionalOffset = GetPositionOffset();
        PlayerPrefs.SetFloat("PositionalOffsetX", (positionalOffset.x));
        PlayerPrefs.SetFloat("PositionalOffsetY", (positionalOffset.y));
        PlayerPrefs.SetFloat("PositionalOffsetZ", (positionalOffset.z));
        players[1].transform.GetComponent<UserSyncPosition>().positionalOffset = true;
    }

    public void ApplyRotationalOffset()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        rotationalOffset = GetRotationOffset();
        PlayerPrefs.SetFloat("RotationalOffsetX", (rotationalOffset.x));
        PlayerPrefs.SetFloat("RotationalOffsetY", (rotationalOffset.y));
        PlayerPrefs.SetFloat("RotationalOffsetZ", (rotationalOffset.z));
        players[1].transform.GetComponent<UserSyncPosition>().rotationalOffset = true;
    }

}
