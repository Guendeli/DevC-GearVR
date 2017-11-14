using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIButtonUtils : MonoBehaviour {


    public UnityEvent onInteraction;

	// Use this for initialization
	

    void OnInteraction()
    {
        onInteraction.Invoke();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Shiruken")
        {
            onInteraction.Invoke();
        }
    }
}
