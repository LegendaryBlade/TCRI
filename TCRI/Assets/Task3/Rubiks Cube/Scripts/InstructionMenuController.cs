using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionMenuController : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject CubeController;
    public GameObject CenterEyeAnchor;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = CubeController.transform.position + new Vector3(0, 0.15f, 0);
        transform.position = position;

        Vector3 dir = transform.position - CenterEyeAnchor.transform.position;
        Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = rotation;
    }
}
