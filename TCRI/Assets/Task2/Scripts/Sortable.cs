using Oculus.Interaction.HandGrab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sortable : MonoBehaviour
{

    public int sortKey;
    public Sorter[] buckets;

    public bool IsSelected = false;

    private Vector3 startingLocation;
    
    // Start is called before the first frame update
    void Start()
    {
        startingLocation = transform.position;
        PassthroughManager.onLongtermTransition += LongtermTransitionOccured;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < 0.1) {
            //Shape fell on ground. Teleport to starting location again
            transform.position = startingLocation;
        }
    }

    public void OnSelect() {
        Logger.Log("Sortable", "Player selected a SortableShape with " + sortKey + " cubes.");
        IsSelected = true;
        foreach(Sorter s in buckets) {
            if(s.keyToSort == sortKey) {
                //s is the bucket this objects belongs into
                s.highlight();
                return;
            }
        }
    }

    public void OnDeselect() {
        Logger.Log("Sortable", "Player dropped a SortableShape with " + sortKey + " cubes.");
        IsSelected = false;
        //Reset all buckets
        foreach (Sorter s in buckets) {
            s.normalize();
        }

        //activate gravity for a short amount of time to let shape fall again
        GetComponent<Rigidbody>().isKinematic = false;
        Invoke("StopGravity", 2.0f);
    }

    private void StopGravity() {
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void LongtermTransitionOccured(PassthroughManager.Environments from, PassthroughManager.Environments to) {
        if(to == PassthroughManager.Environments.R) {
            //deactivate handgrabbing while in R to prevent unintended interactions
            GetComponent<HandGrabInteractable>().enabled = false;
        } else {
            GetComponent<HandGrabInteractable>().enabled = true;
        }
    }

    public void Sorted() {
        //Stuff that happens when this item is sorted
    }

    public void Reset() {
        //Stuff that happens when this item is sorted incorrectly (e.g. reset)
        transform.position = startingLocation;
    }
}
