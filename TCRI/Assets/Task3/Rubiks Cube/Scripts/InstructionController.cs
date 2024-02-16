using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionController : MonoBehaviour
{
    public float distance = 0.04f;
    public bool active;
    public GameObject Arrow90;
    public GameObject Arrow180;
    public GameObject Arrow270;

    public void updateInstruction(Turn nextTurn) {
        if (!active) return;
        Arrow90.transform.localPosition = Vector3.zero;
        Arrow90.transform.localRotation = Quaternion.identity;
        Arrow90.GetComponent<MeshRenderer>().enabled = false;
        Arrow180.transform.localPosition = Vector3.zero;
        Arrow180.transform.localRotation = Quaternion.identity;
        Arrow180.GetComponent<MeshRenderer>().enabled = false;
        Arrow270.transform.localPosition = Vector3.zero;
        Arrow270.transform.localRotation = Quaternion.identity;
        Arrow270.GetComponent<MeshRenderer>().enabled = false;

        if (nextTurn != Turn.EmptyTurn) {
            //Get correct Arrow based on steps
            GameObject refArrow;
            if (nextTurn.steps == 1) {
                refArrow = Arrow90;
            } else if (nextTurn.steps == 2) {
                refArrow = Arrow180;
            } else {
                refArrow = Arrow270;
            }

            //Get position of refArrow
            Vector3 loc;
            Quaternion rot;

            switch (nextTurn.face) {
                case "F": loc = Vector3.back; rot = Quaternion.Euler(0, 90, -90); break;
                case "B": loc = Vector3.forward; rot = Quaternion.Euler(90, 0, 0); break;
                case "L": loc = Vector3.left; rot = Quaternion.Euler(0, 180, -90); break;
                case "R": loc = Vector3.right; rot = Quaternion.Euler(0, 0, -90); break;
                case "U": loc = Vector3.up; rot = Quaternion.Euler(0, 0, 0); break;
                default: loc = Vector3.down; rot = Quaternion.Euler(0, 0, -180); break; //D
            }

            refArrow.transform.localPosition = loc * distance;
            refArrow.transform.localRotation = rot;
            refArrow.GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
