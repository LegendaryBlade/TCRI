using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristTrigger : MonoBehaviour
{
    public GameObject triggerObject;

    public delegate void OnWristTriggered();
    public static event OnWristTriggered onWristTriggered;

    private FollowingManager followingManagerOfTriggerObject;
    private bool isTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        followingManagerOfTriggerObject = triggerObject.GetComponent<FollowingManager>();
        triggerObject.transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 1, LayerMask.GetMask("User"))) {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
            
            if(!isTriggered && !followingManagerOfTriggerObject.enabled) {  //If not already triggered and not already visible:
                triggerObject.transform.position = transform.position;      //Move to wrist anchor
                followingManagerOfTriggerObject.enabled = true;             //activate followingManager to adjust to view
                Logger.Log("WristTrigger", "Triggered");
                isTriggered = true;
                onWristTriggered(); //Tell the tutorial, that the user achieved that
            }
        } else {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down), Color.red);

            //gameObject.SetActive(false);

            if (isTriggered) {
                //Logger.Log("WristTrigger", "Untriggered");
                isTriggered = false;
            }
        }
    }
}
