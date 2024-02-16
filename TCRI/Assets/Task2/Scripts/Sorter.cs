using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Sorter : MonoBehaviour
{

    public int keyToSort;
    public string TextLabel;
    public Material normalMaterial;
    public Material highlightedMaterial;
    public PassthroughManager passthroughManager;

    public TextMeshPro label;

    // Start is called before the first frame update
    void Start()
    {
        PassthroughManager.onLongtermTransition += TransitionOccured;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        Sortable s = other.gameObject.GetComponent<Sortable>();
        if (s != null && s.IsSelected == false) {   //Only trigger this event, when user does not hold cubeshape in hand
            //Component is sortable
            AudioSource[] audios = GetComponents<AudioSource>();
            if (s.sortKey == keyToSort) {
                //Sorted correctly
                Logger.Log("Sorter Bucket " + keyToSort, "Correctly sorted");
                audios[0].Play();
                s.Sorted();
            } else {
                //sorted incorrectly
                Logger.Log("Sorter Bucket " + keyToSort, "Player sorted a Sortable Shape with " + s.sortKey + " cubes incorrectly.");
                audios[1].Play();
                s.Reset();
            }
        }
    }

    private void TransitionOccured(PassthroughManager.Environments from, PassthroughManager.Environments to) {
        if(to == PassthroughManager.Environments.VR) {
            label.text = keyToSort.ToString();
        } else if(to == PassthroughManager.Environments.R) {
            label.text = "";
        } else {
            label.text = TextLabel;
        }
    }

    public void highlight() {
        if(passthroughManager.GetCurrentEnvironment() <= PassthroughManager.Environments.FAR) {
            GetComponent<MeshRenderer>().material = highlightedMaterial;
        }
    }

    public void normalize() {
        GetComponent<MeshRenderer>().material = normalMaterial;
    }
}
