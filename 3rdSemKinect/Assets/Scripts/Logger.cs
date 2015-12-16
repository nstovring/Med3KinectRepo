using UnityEngine;
using System.Collections;
using System.IO;

public class Logger : MonoBehaviour
{

    private bool isLogging;

    void Update()
    {

    }

	// Use this for initialization
	void Start () {
        System.IO.File.Delete(@"C:\Users\nstovring\Documents\GitHub\Med3KinectRepo\3rdSemKinect\Assets\TestingLogs\LogTracking.txt");

        using (StreamWriter file =
              new StreamWriter(@"C:\Users\nstovring\Documents\GitHub\Med3KinectRepo\3rdSemKinect\Assets\TestingLogs\LogTracking.txt", true))
        {
            file.WriteLine("This is a Header \n");
        }
    }

    /// <summary>
    /// Log Data
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="id"></param>
    /// <param name="time"></param>
    static public void LogData(string tracking, Vector3 position, Vector3 rotation, string id, string time)
    {
        using (StreamWriter file =
               new StreamWriter(@"C:\Users\nstovring\Documents\GitHub\Med3KinectRepo\3rdSemKinect\Assets\TestingLogs\LogTracking.txt", true))
        {
            file.WriteLine(tracking + "\t" + position + "\t Orientation:" + "\t" + rotation + "\t UserID:" + "\t" + id + "\t" + "Time:" + "\t" + time);
        }
    }

    /// <summary>
    /// Log Data
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="position"></param>
    /// <param name="id"></param>
    /// <param name="time"></param>
    static public void LogData(string tracking, Vector3 position, string id, float time)
    {
        using (StreamWriter file =
               new StreamWriter(@"C:\Users\nstovring\Documents\GitHub\Med3KinectRepo\3rdSemKinect\Assets\TestingLogs\LogTracking.txt", true))
        {
            Debug.Log(tracking + "\t" + position.x +"\t"+ position.z + "\t UserID:" + "\t" + id + "\t" + "Time:" + "\t" + time);
            file.WriteLine(tracking + "\t" + position.x + "\t" + position.z + "\t UserID:" + "\t" + id + "\t" + "Time:" + "\t" + time);
        }
    }
}
