using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using System;

public class StartUpController : MonoBehaviour
{
    public TextMeshPro HeadlineText;
    public TextMeshPro InstructionText;
    public PassthroughManager passthroughManager;
    public WristTrigger wristTrigger;

    public GameObject[] Buttons;

    public GameObject OpenMenuButton;

    public GameObject Task11;
    public GameObject Task12;
    public GameObject Task13;
    public GameObject Task14;

    public GameObject Task2;
    public GameObject Task3;

    private System.DateTime startTimeOfTask;
    private System.DateTime endTimeOfTask;

    private enum State { Setup, Task11, Task12, Task13, Task14, Task2, Task3 }
    private State currentState = State.Setup;

    private enum SetupState { Welcome, ControllersGrabbed, Aligning, Aligned, ControllersDropped }
    private SetupState currentSetupState = SetupState.Welcome;

    private enum Task1State { CollectFirstObject, BringFirstObjectHome, ProceedWith11, ProceedWith12, ProceedWith13, ProceedWith14, LearnWristTrigger, LearnTransition, ProceedWith15, Done }
    private Task1State currentTask1State = Task1State.CollectFirstObject;
    //Search and collect

    private enum Task2State { WorkInProgress, Done }
    private Task2State currentTask2State = Task2State.WorkInProgress;
    private bool Task2ReachedAR = false;
    private bool Task2ReachedFAR = false;
    private bool Task2ReachedVR = false;
    //Sorting Cubeshapes

    private enum Task3State { WorkInProgress, Done}
    private Task3State currentTask3State = Task3State.WorkInProgress;
    private bool Task3ReachedAR = false;
    private bool Task3ReachedVR = false;
    //Solving Rubik's Cube

    // Start is called before the first frame update
    void Start()
    {
        HeadlineText.text = "Welcome to the User Study";
        InstructionText.text = "First, you need to align the virtual room with the real room. Please grab your controllers and press both middlefinger triggers once.";

        //Task 1 contains the Rubiks Cube and Kociemba Algorithm.
        //We call Solve in the CubeController just to load the files into memory
        //before application launch to prevent massive frame drops when loading during runtime
        Solver.Scramble();

        //Sign up for the events needed:
        AlignManager.onAligning += CurrentlyAligning;
        AlignManager.onAlign += IsAligned;
        WristTrigger.onWristTriggered += WristTriggered;
        PassthroughManager.onLongtermTransition += LongtermTransitionOccured;

        TaskExecution.onCollected += ObjectCollected;
        TaskExecution.onBroughtHome += ObjectBroughtHome;
        TaskExecution.onTaskFinished += TaskFinished;

        OpenMenuButton.GetComponent<FollowingManager>().enabled = false;
        wristTrigger.enabled = false;

        Logger.Log("StartUpController", "Application StartUp");
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.IsControllerConnected(OVRInput.Controller.Hands)) {
            //Handtracking activated
            ControllersDropped();
        } else {
            //Handtracking deactivated
            ControllersGrabbed();
        }
    }

    void OnApplicationPause() {
        //User has removed the headset

        //The application can tell the end of task1. Thus, when removing the HMD after task 1, don't do anything.
        //However, the end of task 2 and 3 are registered here, when the user removes the HMD
        if ((currentState == State.Task11 || currentState == State.Task12 || currentState == State.Task13 || currentState == State.Task14) && currentTask1State != Task1State.Done) {
            //Apparently the HMD was removed before the task was finished.
            Logger.Log("StartUpController", "The HMD was removed before Task1 was finished. Thus, no speed measurements available.");
            //Don't do anything more. User could theoretically resume the task
        }
        if (currentState == State.Task2 && currentTask2State != Task2State.Done) {
            //Since we don't receive an end call once task 2 is finished, we call it from here:
            currentTask2State = Task2State.Done;
            TaskFinished(2);
        }
        if (currentState == State.Task3 && currentTask3State != Task3State.Done) {
            //Since we don't receive an end call once task 3 is finished, we call it from here:
            currentTask3State = Task3State.Done;
            TaskFinished(3);
        }
        Logger.Log("StartUpController", "Application paused after " + Time.time + " seconds");
    }

    public void ToggleButtonPressed() {
        FollowingManager fm = GetComponent<FollowingManager>();
        if(fm.enabled) {
            OpenMenuButton.transform.position = transform.position;
            OpenMenuButton.GetComponent<FollowingManager>().enabled = true;
            fm.enabled = false;
        } else {
            transform.position = OpenMenuButton.transform.position;
            fm.enabled = true;
            OpenMenuButton.GetComponent<FollowingManager>().enabled = false;
        }
    }

    private void OpenMenu() {
        FollowingManager fm = GetComponent<FollowingManager>();
        if (!fm.enabled) {
            transform.position = OpenMenuButton.transform.position;
            fm.enabled = true;
            OpenMenuButton.GetComponent<FollowingManager>().enabled = false;
        }
    }

    public void ButtonPressed(int task) {
        if(currentSetupState != SetupState.ControllersDropped) {
            return;
            //Don't react to Button presses, if setup isn't complete
        }

        //#####
        if(task == 11) {
            Logger.Log("StartUpController", "Starting Task 1.1");
            passthroughManager.SetEnvironment(PassthroughManager.Environments.R);

            //Change texts
            HeadlineText.text = "Task 1.1";
            InstructionText.text = "In this task, you are asked to collect five objects in the room. Look on right wrist. There you can see a compass. Its red needle points to the object you have to collect. Walk there and grab the real object. You can close this menu anytime by pressing the button with the red 'X'. It will open up again, once you receive a new notification.";
            currentState = State.Task11;
            currentTask1State = Task1State.CollectFirstObject;

            Task11.SetActive(true);
            StartTask();
            wristTrigger.enabled = false;
        }
        if (task == 12) {
            Logger.Log("StartUpController", "Starting Task 1.2");
            passthroughManager.SetEnvironment(PassthroughManager.Environments.FAR);

            //Change texts
            HeadlineText.text = "Task 1.2";
            InstructionText.text = "In this task, you are asked to collect five objects in the room. Look on right wrist. There you can see a compass. Its red needle points to the object you have to collect. Walk there and grab the real object. You can close this menu anytime by pressing the button with the red 'X'. It will open up again, once you receive a new notification.";
            currentState = State.Task12;
            currentTask1State = Task1State.ProceedWith12;

            Task12.SetActive(true);
            StartTask();
            wristTrigger.enabled = false;
        }
        if (task == 13) {
            Logger.Log("StartUpController", "Starting Task 1.3");
            passthroughManager.SetEnvironment(PassthroughManager.Environments.VR);

            //Change texts
            HeadlineText.text = "Task 1.3";
            InstructionText.text = "In this task, you are asked to collect five objects in the room. Look on right wrist. There you can see a compass. Its red needle points to the object you have to collect. Walk there and grab the real object. You can close this menu anytime by pressing the button with the red 'X'. It will open up again, once you receive a new notification.";
            currentState = State.Task13;
            currentTask1State = Task1State.ProceedWith13;

            Task13.SetActive(true);
            StartTask();
            wristTrigger.enabled = false;
        }
        if (task == 14) {
            Logger.Log("StartUpController", "Starting introduction to Task 1.4");
            passthroughManager.SetEnvironment(PassthroughManager.Environments.AR);

            //Change texts
            HeadlineText.text = "Task 1.4";
            InstructionText.text = "In this task, you are asked to collect ten objects. However, this time you have an additional tool. Please look onto your left wrist.";
            currentState = State.Task14;
            currentTask1State = Task1State.LearnWristTrigger;

            wristTrigger.enabled = true;
        }


        //#####
        if (task == 2) {
            currentState = State.Task2;
            Logger.Log("StartUpController", "Starting Task 2");
            passthroughManager.SetEnvironment(PassthroughManager.Environments.R);

            //Change texts
            HeadlineText.text = "Task 2";
            InstructionText.text = "Please walk to the table and sit down. In this task, you are asked to sort the shapes made out of cubes in front of you into boxes. You need to find out how to sort them yourself. Use the slider to transition to different environments. Each environment contains helpful guidance to support you with this task.";
            currentState = State.Task2;
            currentTask2State = Task2State.WorkInProgress;

            Task2.SetActive(true);
            StartTask();
            wristTrigger.enabled = true;
        }
        if (task == 3) {
            currentState = State.Task3;
            Logger.Log("StartUpController", "Starting Task 3");
            passthroughManager.SetEnvironment(PassthroughManager.Environments.R);

            //Change texts
            HeadlineText.text = "Task 3";
            InstructionText.text = "Please walk to the table and sit down. In this task, you are asked to solve the real Rubik's Cube that lays on the table in front of you. Never solved one before? No problem. Use the slider to transition to different environments. Each environment contains helpful guidance to support you with this task.";
            currentState = State.Task3;

            Task3.SetActive(true);
            StartTask();
            wristTrigger.enabled = true;
        }
            
        //Deactive buttons again
        foreach (GameObject btn in Buttons) {
            btn.SetActive(false);
        }
    }

    //####  SETUP

    private void ControllersGrabbed() {
        if (currentState == State.Setup && currentSetupState == SetupState.Welcome) {
            InstructionText.text = "Please wait until the virtual representation of the controllers matches the real physical location of the controllers. Then, walk towards the biggest window in the middle of the room. Once there, press and hold the oculus button while looking outside. Then, press the index trigger on the right controller to start the alignment mode.";
            OpenMenu();
            currentSetupState = SetupState.ControllersGrabbed;
        }
    }

    private void CurrentlyAligning() {
        if (currentState == State.Setup && currentSetupState == SetupState.ControllersGrabbed) {
            InstructionText.text = "Try to lay the virtual content directly over the real window frame. \n -Use the right controller to adjust the position. \n -Use the left controller to adjust the rotation. \n - You can use the pink paper strip on the frame as a reference point. \n -Once satisfied, press the right index trigger again.";
            OpenMenu();
            currentSetupState = SetupState.Aligning;
        }
    }

    private void IsAligned() {
        if (currentState == State.Setup && currentSetupState == SetupState.Aligning) {
            InstructionText.text = "Well done! The room is aligned. Put your controllers away now.";
            OpenMenu();
            currentSetupState = SetupState.Aligned;
        }
    }

    private void ControllersDropped() {
        if (currentState == State.Setup && currentSetupState == SetupState.Aligned) {
            InstructionText.text = "The setup is now complete. Please work your way through the tasks by pressing a corresponding button. When you finished a task, just remove the VR-headset. The experimenter will take care of it. After each task, you are asked to fill out some questionnaires. You can close this menu anytime. It will open again, once you receive a new notification.";
            currentSetupState = SetupState.ControllersDropped;
            OpenMenu();
            Logger.Log("StartUpController", "Setup complete");
            foreach (GameObject btn in Buttons) {
                btn.SetActive(true);
            }
        }
    }


    //####  TASKS
    private void WristTriggered() {
        if (currentState == State.Task14 && currentTask1State == Task1State.LearnWristTrigger) {
            //Successfully triggered the wrist
            InstructionText.text = "Congratulations! You can now see a slider in front of you. With this slider you can transition between multiple environments; from reality all the way to virtual reality. Try it out: \n - Grab the red sphere with your hand. \n - Or use the arrow buttons. \n - Move the red sphere to VR.";
            OpenMenu();
            currentTask1State = Task1State.LearnTransition;
        }
    }

    private void LongtermTransitionOccured(PassthroughManager.Environments from, PassthroughManager.Environments to) {
        //###Task 1###
        if (currentState == State.Task14 && currentTask1State == Task1State.LearnTransition && to == PassthroughManager.Environments.VR) {
            InstructionText.text = "Perfect! From now on, until the end of the user study, you can use this slider at ANY TIME to FREELY transition between the environments at your convenience. Keep that in mind! You can close the slider by pressing the red X in the middle. Now, please collect the items again, as you did earlier and close this menu.";
            OpenMenu();
            Logger.Log("StartUpController", "Finished introduction to Task 1.4");
            Task14.SetActive(true);
            StartTask();
            currentTask1State = Task1State.ProceedWith15;
        }
        //###Task 2###
        if (currentState == State.Task2 && Task2ReachedVR == false && to == PassthroughManager.Environments.VR) {
            //Successfully reached VR for the first time
            InstructionText.text = "In this environment, the label of the boxes changed! Also, one of the boxes turns green when you grab a cubeshape. That is the box this cubeshape belongs in. You can grab the cubeshapes by pinching your thumb and index finger. Drop it in the correct box!";
            OpenMenu();
            Task2ReachedVR = true;
        }
        if (currentState == State.Task2 && Task2ReachedFAR == false && to <= PassthroughManager.Environments.FAR && to != PassthroughManager.Environments.VR) {
            //Successfully reached VR for the first time
            InstructionText.text = "In this environment, one of the boxes turns green when you grab a cubeshape. That is the box this cubeshape belongs in. You can grab the cubeshapes by pinching your thumb and index finger. Drop it in the correct box!";
            OpenMenu();
            Task2ReachedFAR = true;
        }
        if (currentState == State.Task2 && Task2ReachedAR == false && to == PassthroughManager.Environments.AR) {
            //Successfully reached AR
            InstructionText.text = "In this environment, when dropping a cubeshape into a box, you get a sound effect that tells you, if you sorted the shape correctly or not. If it was wrong, the shape gets teleported to its starting position. You can grab the cubeshapes by pinching your thumb and index finger.";
            OpenMenu();
            Task2ReachedAR = true;
        }
        //###Task 3###
        if (currentState == State.Task3 && Task3ReachedVR == false && to <= PassthroughManager.Environments.FAR) {
            //Reached VR for the first time
            Task3ReachedVR = true;
            InstructionText.text = "The blue arrows on the Rubik's Cube indicate the next turn you have to do in order to solve the cube. There are two types of arrows: \n- A 90 degree arrow: turn this side 90 degrees \n- A 180 degree arrow: turn this side 180 degrees. \n Grab the cube with one hand (making a fist) and pinch index finger and thumb of the other hand to select and turn a side.";
            OpenMenu();
        }
        if (currentState == State.Task3 && Task3ReachedAR == false && to == PassthroughManager.Environments.AR) {
            Task3ReachedAR = true;
            InstructionText.text = "In this environment, you can't freely turn the cube: Above the cube there are two buttons. The right one executes the next turn on the virtual Rubik's Cube. The left one undos a previous move.";
            OpenMenu();
        }
    }

    private void ObjectCollected() {
        if (currentState == State.Task11 && currentTask1State == Task1State.CollectFirstObject) {
            InstructionText.text = "Good, you are there. Now grab and hold the real object that lays there. The compass now poinst towards a table in the room. Bring the object there! Please close the menu again.";
            OpenMenu();
            currentTask1State = Task1State.BringFirstObjectHome;
        }
    }

    private void ObjectBroughtHome() {
        if (currentState == State.Task11 && currentTask1State == Task1State.BringFirstObjectHome) {
            InstructionText.text = "Perfect! Now there are four more other objects in the room. Proceed with them in the same way: Collect them and bring them back to the table. The app will notify you once you collected all objects. You can now close the menu.";
            OpenMenu();
            currentTask1State = Task1State.ProceedWith11;
        }
    }

    private void StartTask() {
        passthroughManager.ActivateMeasurements();
        startTimeOfTask = System.DateTime.Now;
    }

    private void TaskFinished(int taskNo) {

        if(taskNo == 1) {
            currentTask1State = Task1State.Done;
        }

        passthroughManager.DeactivateMeasurements();
        endTimeOfTask = System.DateTime.Now;

        InstructionText.text = "You finished the task! You can now remove the headset.";
        OpenMenu();
        passthroughManager.DumpMeasurementData();

        Logger.Log("StartUpController", "Ending task " + taskNo);
        Logger.Log("StartUpController", "Duration of " + Enum.GetName(typeof(State), currentState) + ": " + (endTimeOfTask - startTimeOfTask).TotalSeconds + "s");
    }
}
