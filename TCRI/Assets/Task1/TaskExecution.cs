using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class TaskExecution : MonoBehaviour
{
    public GameObject Compass;
    public GameObject CompassWristAnchor;
    public GameObject PalmAnchor;
    public PassthroughManager passthroughManager;

    //Speed measurements
    public GameObject CenterEyeAnchor;
    private Vector3 previousPosition;

    private float totalDistanceTraveled = 0.0f;
    private float[] distanceTraveledPerEnv = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, };

    private float maxSpeed = 0.0f;
    private float[] maxSpeedPerEnv = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, };

    private List<float>[] averageSpeedPerEnv = { new List<float>(), new List<float>(), new List<float>(), new List<float>(), new List<float>(), new List<float>() };
    private int[] elemCounters = { 0, 0, 0, 0, 0, 0 };

    private List<(float, string)> allSpeeds = new List<(float, string)>();

    //GameObjects for Collectables
    public GameObject TargetSpot;   //The location to where the objects should be brought to
    public GameObject[] Collectables;    //The location of the objects to collect. In order!
    public GameObject currentTarget;

    //Logic of collectables
    public int currentIndex = 1;   //The index of the current collectable sphere in the compass constraint manager
    private bool collecting = true; //True when collecting, false when bringing back
    private bool done = false;

    private AudioSource successAudio;

    public delegate void OnCollected();
    public static event OnCollected onCollected;

    public delegate void OnBroughtHome();
    public static event OnBroughtHome onBroughtHome;

    public delegate void OnTaskFinished(int taskNo);
    public static event OnTaskFinished onTaskFinished;

    private void OnEnable() {
        successAudio = GetComponent<AudioSource>();
        Compass.transform.SetParent(CompassWristAnchor.transform);
        Compass.transform.localPosition = Vector3.zero;
        Compass.transform.localRotation = Quaternion.identity;

        currentTarget = Collectables[0];

        InvokeRepeating("LogPosition", 0.5f, 0.5f); //Log Position and Speed
    }

    // Update is called once per frame
    void Update()
    {

        AimConstraint aim = Compass.GetComponentInChildren<AimConstraint>();

        if(!done) {
            //Task is not finished yet

            currentTarget = Collectables[currentIndex - 1]; //sic!
            
            if (collecting) {
                //Collecting item
                if (Vector3.Distance(PalmAnchor.transform.position, currentTarget.transform.position) < 0.4f) {
                    //Check for walls (e.g., regal)
                    RaycastHit hit;
                    Vector3 direction = currentTarget.transform.position - PalmAnchor.transform.position;
                    float distance = Vector3.Distance(PalmAnchor.transform.position, currentTarget.transform.position);

                    if (Physics.Raycast(PalmAnchor.transform.position, direction, out hit, distance, LayerMask.GetMask("Room"))) {
                        //There is furniture in the way. Cannot collect that item
                    } else {
                        //reached sphere
                        Logger.Log("Task1Executor", "Collected " + currentTarget.name);
                        successAudio.Play();
                        onCollected();

                        //Teleport Object to Hand
                        currentTarget.GetComponent<HandPoseTeleporter>().TeleportToHand();

                        //Deactivate on Compass
                        ConstraintSource s = aim.GetSource(currentIndex);
                        s.weight = 0;
                        aim.SetSource(currentIndex, s);

                        //Activate TargetSpot on Compass
                        ConstraintSource target_source = aim.GetSource(0);
                        target_source.weight = 1;
                        aim.SetSource(0, target_source);

                        collecting = false;
                    }
                }
            } else {
                //Bringing item back
                if (Vector3.Distance(PalmAnchor.transform.position, TargetSpot.transform.position) < 0.5f) {
                    //Reached table
                    Logger.Log("Task1Executor", "Reached TargetSpot");
                    successAudio.Play();
                    onBroughtHome();

                    //Teleport object to table:
                    currentTarget.GetComponent<HandPoseTeleporter>().TeleportToTable();

                    //Count to next collectable if available:
                    if (currentIndex < Collectables.Length) {
                        currentIndex += 1;

                        //Deactivate TargetSpot on Compass
                        ConstraintSource s = aim.GetSource(0);
                        s.weight = 0.0f;
                        aim.SetSource(0, s);

                        //Activate next Collectable, if there is one
                        ConstraintSource new_target = aim.GetSource(currentIndex);
                        new_target.weight = 1.0f;
                        aim.SetSource(currentIndex, new_target);

                        collecting = true;

                    } else {
                        //Reached end of task
                        CancelInvoke("LogPosition");
                        done = true;

                        Logger.Log("Task1Executor", "Task 1 finished.");
                        Logger.Log("Task1Executor", "Total distance traveled: " + totalDistanceTraveled + "m");
                        Logger.Log("Task1Executor", "Distance traveled per Env: VR=" + distanceTraveledPerEnv[0].ToString("0.00") + "m, AV=" + distanceTraveledPerEnv[1].ToString("0.00") + "m, WAV=" + distanceTraveledPerEnv[2].ToString("0.00") + "m, FAR=" + distanceTraveledPerEnv[3].ToString("0.00") + "m, AR=" + distanceTraveledPerEnv[4].ToString("0.00") + "m, R=" + distanceTraveledPerEnv[5].ToString("0.00") + "m");
                        Logger.Log("Task1Executor", "Max speed: " + maxSpeed + "m/s");
                        Logger.Log("Task1Executor", "Max speed per Env: VR=" + maxSpeedPerEnv[0].ToString("0.00") + "m/s, AV=" + maxSpeedPerEnv[1].ToString("0.00") + "m/s, WAV=" + maxSpeedPerEnv[2].ToString("0.00") + "m/s, FAR=" + maxSpeedPerEnv[3].ToString("0.00") + "m/s, AR=" + maxSpeedPerEnv[4].ToString("0.00") + "m/s, R=" + maxSpeedPerEnv[5].ToString("0.00") + "m/s");

                        float avgSpeed = 0.0f;
                        int counter = 0;
                        for (int i = 0; i < 6; i++) {
                            foreach (float f in averageSpeedPerEnv[i]) {
                                avgSpeed += f;
                            }
                            counter += elemCounters[i];
                        }
                        Logger.Log("Task1Executor", "Avg speed: " + avgSpeed / counter + "m/s");

                        float[] avgSpeeds = { -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, };
                        for (int i = 0; i < 6; i++) {
                            float sum = 0;
                            foreach (float f in averageSpeedPerEnv[i]) {
                                sum += f;
                            }
                            if (elemCounters[i] != 0) {
                                avgSpeeds[i] = sum / elemCounters[i];
                            }
                        }
                        Logger.Log("Task1Executor", "Avg speed per Env: VR=" + avgSpeeds[0].ToString("0.00") + "m/s, AV=" + avgSpeeds[1].ToString("0.00") + "m/s, WAV=" + avgSpeeds[2].ToString("0.00") + "m/s, FAR=" + avgSpeeds[3].ToString("0.00") + "m/s, AR=" + avgSpeeds[4].ToString("0.00") + "m/s, R=" + avgSpeeds[5].ToString("0.00") + "m/s");

                        string allSpeedsAsString = "[";
                        foreach ((float, string) tuple in allSpeeds) {
                            allSpeedsAsString += "[" + tuple.Item1 + "," + tuple.Item2 + "],";
                        }
                        allSpeedsAsString += "]";
                        Logger.Log("Task1Executor", "All speeds: " + allSpeedsAsString);

                        onTaskFinished(1);
                    }
                }
            }
        } else {
            //Stuff that happens regularly, when task is finished
        }
    }

    private void LogPosition() {
        Vector3 position = CenterEyeAnchor.transform.position;
        float speed = 0.0f; //m/s

        if(previousPosition != null && previousPosition != Vector3.zero) {
            //Calc stats. Distance traveled since last call:
            float Distance = Vector3.Distance(position, previousPosition);  //Distance traveled in the last 0.5 seconds in meters

            //add to the distances:
            totalDistanceTraveled += Distance;
            distanceTraveledPerEnv[(int)passthroughManager.GetCurrentEnvironment()] += Distance;

            //speeds:
            speed = Distance * 2;  //Distance traveled in 1 second
            //overall max speed
            if(speed > maxSpeed) {
                maxSpeed = speed;
            }
            //max speed per environment
            if(speed > maxSpeedPerEnv[(int)passthroughManager.GetCurrentEnvironment()]) {
                maxSpeedPerEnv[(int)passthroughManager.GetCurrentEnvironment()] = speed;
            }
            //average speed
            averageSpeedPerEnv[(int)passthroughManager.GetCurrentEnvironment()].Add(speed);
            elemCounters[(int)passthroughManager.GetCurrentEnvironment()] += 1;

            allSpeeds.Add((speed, passthroughManager.GetCurrentEnvAsString()));
            Logger.Log("Task1Executor", "PrevPos:" + previousPosition + "; CurPos: " + position + "; Dist: " + Distance + "; Env: " + passthroughManager.GetCurrentEnvAsString() + "; Speed: " + speed + "m/s");
        }

        previousPosition = position;    //Update previous position
    }
}
