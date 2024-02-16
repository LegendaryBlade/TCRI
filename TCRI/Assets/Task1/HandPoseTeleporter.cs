using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPoseTeleporter : MonoBehaviour
{
    //Local data with hand palm as parent
    public GameObject HandPalmAnchor;
    public Vector3 HandPosePosition;
    public Vector3 HandPoseRotation;

    public Vector3 TablePosePosition;
    public Vector3 TablePoseRotation;

    private Vector3 previousPos;
    private Quaternion previousRot;
    private Vector3 targetPos;
    private Quaternion targetRot;

    private Transform preParent;

    public float animationSpeed = 2f;
    private float timeCount = 0.0f;
    private bool animatingHand = false;
    private bool animatingTable = false;

    void Start()
    {
        preParent = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        if(animatingHand) {
            transform.rotation = Quaternion.Lerp(previousRot, targetRot, timeCount);
            transform.position = Vector3.Lerp(previousPos, targetPos, timeCount);

            timeCount += animationSpeed * Time.deltaTime;

            if (timeCount >= 1) {
                //reached hand
                animatingHand = false;
                transform.SetParent(HandPalmAnchor.transform);
                transform.localPosition = HandPosePosition;
                transform.localRotation = Quaternion.Euler(HandPoseRotation);
            }
        } else if (animatingTable) {
            transform.rotation = Quaternion.Lerp(previousRot, targetRot, timeCount);
            transform.position = Vector3.Lerp(previousPos, targetPos, timeCount);

            timeCount += animationSpeed * Time.deltaTime;

            if (timeCount >= 1) {
                //reached table
                animatingTable = false;
                transform.localPosition = TablePosePosition;
                transform.localRotation = Quaternion.Euler(TablePoseRotation);
            }
        }
    }

    public void TeleportToHand() {
        previousPos = transform.position;
        previousRot = transform.rotation;

        targetPos = HandPalmAnchor.transform.TransformPoint(HandPosePosition);
        targetRot = HandPalmAnchor.transform.rotation * Quaternion.Euler(HandPoseRotation);
        timeCount = 0.0f;
        animatingHand = true;
    }

    public void TeleportToTable() {
        transform.SetParent(preParent);
        previousPos = transform.position;
        previousRot = transform.rotation;

        targetPos = preParent.transform.TransformPoint(TablePosePosition);
        targetRot = preParent.transform.rotation * Quaternion.Euler(TablePoseRotation);
        timeCount = 0.0f;
        animatingTable = true;
    }
}
