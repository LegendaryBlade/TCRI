using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityController : MonoBehaviour
{

    public GameObject VRRubiksCube;
    public GameObject ARRubiksCube;

    private Vector3 scale;

    // Start is called before the first frame update
    void Start()
    {
        PassthroughManager.onShorttermTransition += ShorttermTransitionOccured;
        scale = transform.localScale;

        //Put VR cube away at first
        //VRRubiksCube.transform.position = Vector3.down;
        //ARRubiksCube.transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ShorttermTransitionOccured(PassthroughManager.Environments from, PassthroughManager.Environments to) {


        //Rubiks Cube activeness:
        if (to == PassthroughManager.Environments.FAR && from == PassthroughManager.Environments.AR) {
            Vector3 pos = ARRubiksCube.transform.position;
            Quaternion rot = ARRubiksCube.transform.rotation;
            //ARRubiksCube.transform.position = Vector3.down;
            ARRubiksCube.transform.localScale = Vector3.zero;
            //ARRubiksCube.transform.position = VRRubiksCube.transform.position;


            VRRubiksCube.transform.position = pos;
            VRRubiksCube.transform.rotation = rot;
            VRRubiksCube.transform.localScale = Vector3.one * 2;
        }
        if (to == PassthroughManager.Environments.AR && from == PassthroughManager.Environments.FAR) {
            //Somehow that triggers the next button on the AR CubeController
            Vector3 pos = VRRubiksCube.transform.position;
            Quaternion rot = VRRubiksCube.transform.rotation;
            //VRRubiksCube.transform.position = Vector3.down;
            VRRubiksCube.transform.localScale = Vector3.zero;
            //VRRubiksCube.transform.position = ARRubiksCube.transform.position;

            ARRubiksCube.transform.position = pos;
            ARRubiksCube.transform.rotation = rot;
            ARRubiksCube.transform.localScale = Vector3.one * 2;
        }
    }
}
