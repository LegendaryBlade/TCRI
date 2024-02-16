using System;
using TMPro;
using UnityEngine;

public class PassthroughManager : MonoBehaviour
{
    //                            VR    AV    W-AV  F-AR  AR
    private float[] Opacities = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

    private System.Object[,] States = {
        //VR    AV    W-AV  F-AR  AR    Avatar  Name
        { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, true, "VR" },
        { 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, true, "AV" },
        { 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, true, "WAV" },
        { 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, true, "FAR" },
        { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, false, "AR" },
        { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, "R" },
        { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, false, "Align" },
    };

    public enum Environments { VR, AV, WAV, FAR, AR, R, Align }

    //Used for precise current Environment
    private Environments currentEnv;

    //Used for longterm Environment measurements
    private Environments longtermCurrentEnv = Environments.R;    //Gets updated, when user is at least 'timeThreshold' seconds in that environment
    private Environments longtermPreviousEnv;   //Gets updated, when user is at least 'timeThreshold' seconds in that environment
    private float timeThreshold = 2.5f;         //User needs to stay 2.5 seconds in the environment to count it as longterm transition
    private float currentTransitionTimer = 0.0f;    //Time the user spend in 'currentEnv' since last shortterm transition
    private float currentEnvironmentTimer = 0.0f;   //Time the user spend in 'currentEnv' since last longterm transition
    private bool longtermTransitionPending = false;

    //Used to activate and deactivate logging the measurements
    private bool measurementsActive = false;

    //Used to count transitions
    private int[,] TransitionCounts = {    //from,to
        {0, 0, 0, 0, 0, 0, },   //from VR
        {0, 0, 0, 0, 0, 0, },   //from AV
        {0, 0, 0, 0, 0, 0, },   //from WAV
        {0, 0, 0, 0, 0, 0, },   //from FAR
        {0, 0, 0, 0, 0, 0, },   //from AR
        {0, 0, 0, 0, 0, 0, },   //from R
    };

    //Used to measure time spend in each environment
    private float[] TimeSpendInEnv = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

    public float TransitionDuration = 2.5f;
    //--------------------------------

    public Material[] VRMaterials;
    public Material[] AVMaterials;
    public Material[] WalllessAVMaterials;
    public Material[] FurnitureARMaterials;
    public Material[] ARMaterials;

    //Used for those GameObjects that cannot be faded out (e.g. fonts)
    public GameObject[] VRGameObjects;
    public GameObject[] AVGameObjects;
    public GameObject[] WallessAVGameObjects;
    public GameObject[] FurnitureARGameObjects;
    public GameObject[] ARGameObjects;

    //--------------------------------

    public GameObject PlayerBody;
    
    public Material UserHandsMaterial;
    public GameObject[] HandWrists;
    public GameObject[] HandTriggers;


    private bool AvatarsVisible = false;

    //--------------------------------

    private float Percent = 1;

    //#### Aligning Modes ###
    private bool Aligning = false;  //User is currently aligning world space
    public bool IsAligned = false; //World space has been aligned

    public GameObject Slider;

    //Events
    public delegate void OnShorttermTransition(Environments from, Environments to);
    public static event OnShorttermTransition onShorttermTransition;
    
    public delegate void OnLongtermTransition(Environments from, Environments to);
    public static event OnLongtermTransition onLongtermTransition;

    // Start is called before the first frame update
    void Start()
    {
        Logger.Log("RVCManager", "Starting RVCManager");

        //Set all materials to 0 opacity upon startup
        System.Object[] Materials = { VRMaterials, AVMaterials, WalllessAVMaterials, FurnitureARMaterials, ARMaterials };
        for (int i = 0; i < Materials.Length; i++) {
            Material[] mat_arr = (Material[])Materials[i];
            foreach (Material m in mat_arr) {
                    m.SetFloat("_Transparency", 0);
            }
        }
    }

    private void OnEnable() {
        
    }

    private void OnDisable() {
        //Stop animation and immediately apply opacities

        //#####TODO#####

        //Materials
        System.Object[] Materials = { VRMaterials, AVMaterials, WalllessAVMaterials, FurnitureARMaterials, ARMaterials };
        for (int i = 0; i < Materials.Length; i++) {
            Material[] mat_arr = (Material[])Materials[i];
            float target_opacity = Opacities[i];

            foreach (Material m in mat_arr) {
                m.SetFloat("_Transparency", target_opacity);
            }
        }

        //GameObjects
        System.Object[] GameObjects = { VRGameObjects, AVGameObjects, WallessAVGameObjects, FurnitureARGameObjects, ARGameObjects };
        for (int i = 0; i < GameObjects.Length; i++) {
            GameObject[] curr_gameObjs = (GameObject[])GameObjects[i];
            foreach (GameObject obj in curr_gameObjs) {
                if (Opacities[i] == 0.0f) {
                    obj.SetActive(false);
                } else {
                    obj.SetActive(true);
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {

        //Environment measurements
        if(measurementsActive) {
            currentEnvironmentTimer += Time.deltaTime;
        }
        
        //Longterm transition timers:
        if(currentTransitionTimer > timeThreshold && longtermTransitionPending) {
            //user has spend enough time in this environment to count it as transition
            LongtermTransition();
        } else {
            currentTransitionTimer += Time.deltaTime;
        }

        //Passthrough measurements
        if(!IsAligned && !Aligning) {
            ShorttermTransition(Environments.R);      //The world space has not yet been aligned and the user is not currently doing it. (First state upon startup)

        } else if(!IsAligned && Aligning) {
            ShorttermTransition(Environments.Align);  //The world space has not yet been aligned but the user currently does it. (Second state upon startup)

        } else if(IsAligned) {
            //RVC Management
            float xPos = Slider.transform.localPosition.x - 0.2f;
            Percent = xPos / -0.4f;

            if (Percent < 0.166f) {
                //R
                ShorttermTransition(Environments.R);

            } else if (Percent >= 0.166f && Percent < 0.332f) {
                //AR
                ShorttermTransition(Environments.AR);
            } else if (Percent >= 0.332f && Percent < 0.498f) {
                //AV
                ShorttermTransition(Environments.FAR);
            } else if (Percent >= 0.498f && Percent < 0.664f) {
                //AV
                ShorttermTransition(Environments.WAV);
            } else if (Percent >= 0.664f && Percent < 0.83f) {
                //AV
                ShorttermTransition(Environments.AV);
            } else if (Percent >= 0.83f) {
                //VR
                ShorttermTransition(Environments.VR);
            }
        }

        //Materials
        System.Object[] Materials = { VRMaterials, AVMaterials, WalllessAVMaterials, FurnitureARMaterials, ARMaterials };
        for (int i = 0; i < Materials.Length; i++) {
            Material[] mat_arr = (Material[]) Materials[i];
            float target_opacity = Opacities[i];

            foreach (Material m in mat_arr) {
                float currentOpacity = m.GetFloat("_Transparency");
                if (target_opacity == 0 && currentOpacity > 0) {
                    m.SetFloat("_Transparency", currentOpacity - (1 / TransitionDuration) * Time.deltaTime);
                } else if (target_opacity == 1 && currentOpacity < 1) {
                    m.SetFloat("_Transparency", currentOpacity + (1 / TransitionDuration) * Time.deltaTime);
                } else {
                    m.SetFloat("_Transparency", target_opacity);
                }
            }
        }

        //GameObjects
        System.Object[] GameObjects = { VRGameObjects, AVGameObjects, WallessAVGameObjects, FurnitureARGameObjects, ARGameObjects};
        for(int i = 0; i < GameObjects.Length; i++) {
            GameObject[] curr_gameObjs = (GameObject[]) GameObjects[i];
            foreach (GameObject obj in curr_gameObjs) {
                if (Opacities[i] == 0.0f) {
                    obj.SetActive(false);
                } else {
                    obj.SetActive(true);
                }
            }
        }

        //Avatars:
        try {
            PlayerBody.GetComponentInChildren<Renderer>().enabled = AvatarsVisible;
        } catch (Exception e) { }

        //User Hands:
        if (currentEnv >= Environments.AR) {

            float min_distance = 1000;

            foreach(GameObject obj in HandTriggers) {
                foreach(GameObject hand in HandWrists) {
                    float distance = Vector3.Distance(obj.transform.position, hand.transform.position);
                    if(distance < min_distance) {
                        min_distance = distance;
                    }
                }
            }

            if(min_distance < 0.3f) {
                //Near a hand trigger. So, go ahead:
                UserHandsMaterial.SetFloat("_Transparency", 1.0f);
            } else {
                UserHandsMaterial.SetFloat("_Transparency", 0.0f);
            }
        } else {
            UserHandsMaterial.SetFloat("_Transparency", 0.0f);
        }
    }

    public Environments GetCurrentEnvironment() {
        return longtermCurrentEnv;
    }

    public string GetCurrentEnvAsString() {
        switch (longtermCurrentEnv) {
            case Environments.R: return "R";
            case Environments.AR: return "AR";
            case Environments.FAR: return "FAR";
            case Environments.WAV: return "WAV";
            case Environments.AV: return "AV";
            case Environments.VR: return "VR";
            default: return "None";
        }
    }

    public void SetEnvironment(Environments env) {
        float targetPercent = 0.0f;
        switch (env) {
            case Environments.R: targetPercent = 0f; break;
            case Environments.AR: targetPercent = 0.249f; break;
            case Environments.FAR: targetPercent = 0.415f; break;
            case Environments.WAV: targetPercent = 0.581f; break;
            case Environments.AV: targetPercent = 0.747f; break;
            case Environments.VR: targetPercent = 1; break;
        }
        float targetX = targetPercent * -0.4f + 0.2f; //Dont ask
        Slider.transform.localPosition = new Vector3(targetX, 0, 0);

    }

    public void NextEnvironment() {
        float targetPercent = 0.0f;
        switch(currentEnv) {
            case Environments.R: targetPercent = 0.249f; break;
            case Environments.AR: targetPercent = 0.415f; break;
            case Environments.FAR: targetPercent = 0.581f; break;
            case Environments.WAV: targetPercent = 0.747f; break;
            case Environments.AV: targetPercent = 1; break;
            case Environments.VR: targetPercent = 1; break;
        }
        float targetX = targetPercent * -0.4f + 0.2f; //Dont ask
        Slider.transform.localPosition = new Vector3(targetX, 0, 0);

    }

    public void PreviousEnvironment() {
        float targetPercent = 0.0f;
        switch (currentEnv) {
            case Environments.R: targetPercent = 0; break;
            case Environments.AR: targetPercent = 0; break;
            case Environments.FAR: targetPercent =  0.249f; break;
            case Environments.WAV: targetPercent =  0.415f; break;
            case Environments.AV: targetPercent =  0.581f; break;
            case Environments.VR: targetPercent =  0.747f; break;
        }
        float targetX = targetPercent * -0.4f + 0.2f; //Dont ask
        Slider.transform.localPosition = new Vector3(targetX, 0, 0);
    }

    //#### Aligning Modes ###

    public void StartAlign() {
        Aligning = true;
        IsAligned = false;  //if user realigns the space
        Logger.Log("RVCManager", "Starting Alignment Mode");
    }

    public void EndAlign() {
        Aligning = false;
        IsAligned = true;
        Logger.Log("RVCManager", "End of Alignment Mode");
    }

    public void ActivateMeasurements() {
        measurementsActive = true;
        currentEnvironmentTimer = 0.0f;
        Logger.Log("RVCManager", "Activating measurements");
    }

    public void DeactivateMeasurements() {
        //Store time since latest longterm transition. IMPORTANT!
        TimeSpendInEnv[(int)longtermCurrentEnv] += currentEnvironmentTimer;
        measurementsActive = false;
        currentEnvironmentTimer = 0.0f;
        Logger.Log("RVCManager", "Deactivating measurements");
    }

    public void DumpMeasurementData() {
        //Store time since latest longterm transition. IMPORTANT!
        TimeSpendInEnv[(int)longtermCurrentEnv] += currentEnvironmentTimer;

        int totalTransitions = 0;
        string dataOut = "";
        for (int i = 0; i < 6; i++) {
            dataOut += "\nFrom " + States[i, 6] + ": ";
            for (int j = 0; j < 6; j++) {
                dataOut += "to " + States[j, 6] + "=" + TransitionCounts[i, j] + "; ";
                totalTransitions += TransitionCounts[i, j];
            }
        }
        Logger.Log("RVCManager", "Data dump: TransitionCounts" + dataOut);
        Logger.Log("RVCManager", "Data dump: Total transitions " + totalTransitions);
        Logger.Log("RVCManager", "Data dump: Time spend in Env: VR=" + TimeSpendInEnv[0] + "s; AV=" + TimeSpendInEnv[1] + "s; WAV=" + TimeSpendInEnv[2] + "s; FAR=" + TimeSpendInEnv[3] + "s; AR=" + TimeSpendInEnv[4] + "s; R=" + TimeSpendInEnv[5] + "s");
    }

    private void ShorttermTransition(Environments to) {
        Opacities[0] = (float)States[(int)to, 0];
        Opacities[1] = (float)States[(int)to, 1];
        Opacities[2] = (float)States[(int)to, 2];
        Opacities[3] = (float)States[(int)to, 3];
        Opacities[4] = (float)States[(int)to, 4];
        AvatarsVisible = (bool)States[(int)to, 5];
        if (currentEnv != to) {
            Logger.Log("RVCManager", "Shortterm transitioning from " + (string)States[(int)currentEnv, 6] + " to " + (string)States[(int)to, 6]);

            //Notify subscribers about the transition:
            if(onShorttermTransition != null) {
                onShorttermTransition(currentEnv, to);
            }

            //Start timer to see, if user stays in this environment long enough to count it as transition
            currentTransitionTimer = 0.0f;
            longtermTransitionPending = true;
        }
        currentEnv = to;
    }

    private void LongtermTransition() {
        longtermPreviousEnv = longtermCurrentEnv;
        longtermCurrentEnv = currentEnv;

        //Log event
        Logger.Log("RVCManager", "Longterm transitioning from " + (string)States[(int)longtermPreviousEnv, 6] + " to " + (string)States[(int)longtermCurrentEnv, 6] + " after " + currentEnvironmentTimer + "s");

        //Save that data into the measurement fields:
        if (measurementsActive && currentEnvironmentTimer != 0) {    //Prevents the starting transition to be logged
            TransitionCounts[(int)longtermPreviousEnv, (int)longtermCurrentEnv] += 1;
            TimeSpendInEnv[(int)longtermPreviousEnv] += currentEnvironmentTimer;
        }

        //Notify subscribers about the transition:
        if(onLongtermTransition != null) {
            onLongtermTransition(longtermPreviousEnv, longtermCurrentEnv);
        }

        //Reset environment counter
        currentEnvironmentTimer = 0.0f;
        longtermTransitionPending = false;
    }
}
