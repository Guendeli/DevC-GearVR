using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlType { HMD, Controller}
public class PlatformManager : MonoBehaviour {


    /*          Platform Manager
     * This script handle both normal gearVR AND motion controller support
     * */
    public static PlatformManager Instance;
    // public members are done here
    public ControlType controlType { get; private set; }

	// Use this for initialization
	void Awake () {
        Instance = this;
	}

    IEnumerator Start()
    {
        // delay platform check
        yield return new WaitForSeconds(1.0f);
        CheckPlatform();
    }

    // helper methods are done here

    void CheckPlatform()
    {
        if (OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote) || OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote))
        {
            controlType = ControlType.Controller;
        }
        else
        {
            controlType = ControlType.HMD;
        }

        Debug.Log("Control Type: " + controlType);
    }
}
