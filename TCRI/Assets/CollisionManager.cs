using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        Sortable s = other.gameObject.GetComponent<Sortable>();
        if (s != null) {
            GetComponent<AudioSource>().Play();
            s.Sorted();
        }
    }
}
