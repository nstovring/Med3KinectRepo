using UnityEngine;
using System.Collections;
using System.IO;

//A class only for the purpose of logging data about the tests
public class Logger : MonoBehaviour
{

    private bool isLogging;

    void Update()
    {

    }

	// Use this for initialization
	void Start () {
        //The system deletes any file named LogTracking.txt at this path
        System.IO.File.Delete(@"C:\Users\nstovring\Documents\GitHub\Med3KinectRepo\3rdSemKinect\Assets\TestingLogs\LogTracking.txt");

        //The system should begin to write to this path
        using (StreamWriter file =
              new StreamWriter(@"C:\Users\nstovring\Documents\GitHub\Med3KinectRepo\3rdSemKinect\Assets\TestingLogs\LogTracking.txt", true))
        {
            //the system writes this as the first line in the .txt document
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
    // A method that logs the position, rotation, id and time every time a new cube is tracked. Is not currently used anywhere
    static public void LogData(string tracking, Vector3 position, Vector3 rotation, string id, string time)
    {
        //system writes to specific path
        using (StreamWriter file =
               new StreamWriter(@"C:\Users\nstovring\Documents\GitHub\Med3KinectRepo\3rdSemKinect\Assets\TestingLogs\LogTracking.txt", true))
        {
            //System should write first how it's tracking, then a tab or four spaces "    ", then position, etc.
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
    //another method for logging, that only logs position, id and time, though not rotation like the other. This method is the one used in the test
    static public void LogData(string tracking, Vector3 position, string id, float time)
    {
        //system writes to specific path
        using (StreamWriter file =
               new StreamWriter(@"C:\Users\nstovring\Documents\GitHub\Med3KinectRepo\3rdSemKinect\Assets\TestingLogs\LogTracking.txt", true))
        {
            //
            Debug.Log(tracking + "\t" + position.x +"\t"+ position.z + "\t UserID:" + "\t" + id + "\t" + "Time:" + "\t" + time);

            //System should write first how it's tracking, then a tab or four spaces "    ", then position, etc.
            file.WriteLine(tracking + "\t" + position.x + "\t" + position.z + "\t UserID:" + "\t" + id + "\t" + "Time:" + "\t" + time);
        }
    }
}
