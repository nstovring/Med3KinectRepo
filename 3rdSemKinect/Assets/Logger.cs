using UnityEngine;
using System.Collections;
using System.IO;

public class Logger : MonoBehaviour
{

    private bool isLogging;

	// Use this for initialization
	void Start () {
        System.IO.File.Delete(@"C:\Users\nstovring\Documents\GitHub\DeathMapping\Kinects\TestingLogs\LogTracking.txt");

        using (StreamWriter file =
              new StreamWriter(@"C:\Users\nstovring\Documents\GitHub\DeathMapping\Kinects\TestingLogs\LogTracking.txt", true))
        {
            file.WriteLine("This is a Header \n");
        }
    }

    static public void LogData(string tracking, Vector3 position, Vector3 rotation, uint id, string time)
    {
        using (StreamWriter file =
               new StreamWriter(@"C:\Users\nstovring\Documents\GitHub\DeathMapping\Kinects\TestingLogs\LogTracking.txt", true))
        {
            file.WriteLine(tracking + "\t" + position + "\t Orientation:" + "\t" + rotation + "\t UserID:" + "\t" + id + "\t" + "Time:" + "\t" + time);
        }
    }
}
