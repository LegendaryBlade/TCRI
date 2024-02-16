using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignManager : MonoBehaviour
{
    public GameObject AnchorR;
    public GameObject AnchorL;
    public PassthroughManager passthroughManager;

    public delegate void OnAligning();
    public static event OnAligning onAligning;
    public delegate void OnAlign();
    public static event OnAlign onAlign;

    private bool Aligning = false;
    private bool PreviouslyPressed = false;

    private void Update() {

        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger)) {
            PreviouslyPressed = true;
        }
        if(!OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && PreviouslyPressed) {
            //Button released
            PreviouslyPressed = false;
            if(!Aligning) {
                //Start Align
                Aligning = true;
                onAligning();
                passthroughManager.StartAlign();
            } else {
                //End Align
                Aligning = false;
                onAlign();
                passthroughManager.EndAlign();
            }
        }

        if(Aligning) {
            //Move room to controller anchors
            Vector3 rOrigin = AnchorR.transform.position;
            Vector3 lOrigin = AnchorL.transform.position;

            Vector3 dir = (lOrigin - rOrigin);
            Vector3 dirProjected = new Vector3(dir.x, 0, dir.z).normalized;

            Quaternion rot = Quaternion.LookRotation(dirProjected) * Quaternion.AngleAxis(-90, Vector3.up);

            transform.position = rOrigin;
            transform.rotation = rot;
        }
    }
}
