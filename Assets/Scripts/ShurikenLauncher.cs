using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShurikenLauncher : MonoBehaviour {

    [UnityEngine.Serialization.FormerlySerializedAs("Prefab")]
    public GameObject ShurikenPrefab;

    public GameObject BambooPrefab;

    public ModeSelectUI SelectUI;

    public float ShurikenLifetime;

    public Vector3 LaunchDirection = Vector3.forward;
    
    public float Velocity;

    public int BambooCount = 15;
    private List<Bamboo> StartingBamboo = new List<Bamboo>();
    
    private bool TouchPadDown = false;
    private Vector2 FirstTouchPosition;
    private float FirstTouchTime;

    private float HandShurikenLerp = 0f;
    
    private float TouchpadDownTime;
    private Quaternion TouchpadDownRotation;

    public int MaxHandShurikens = 3;
    private List<GameObject> HandShurikens = new List<GameObject>();


    public int MaxThrownShurikens = 20;
    private List<GameObject> ThrownShurikens = new List<GameObject>();


    private List<GameObject> ShurikenPool = new List<GameObject>();

    public enum ThrowMode
    {
        Swipe,
        Trigger,
        Throw,

        Count
    }

    private ThrowMode Mode = ThrowMode.Trigger;

    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
        for(int i = 0; i < BambooCount; i++)
        {
            float angle = (Mathf.PI * 2) * i / BambooCount;
            var cosw = Mathf.Cos(angle);
            var sinw = Mathf.Sin(angle);

            Vector3 position = 5 * new Vector3(cosw, 0, sinw);

            GameObject bamboo = (GameObject)Instantiate(BambooPrefab);

            StartingBamboo.Add(bamboo.GetComponent<Bamboo>());
            bamboo.transform.position = position;
        }
    }

    private void AddShurikenToHand()
    {
        GameObject shuriken;
        if(ShurikenPool.Count > 0)
        {
            shuriken = ShurikenPool[ShurikenPool.Count - 1];
            ShurikenPool.RemoveAt(ShurikenPool.Count - 1);
        }
        else
        {
            shuriken = (GameObject)GameObject.Instantiate(ShurikenPrefab);
        }
        shuriken.gameObject.SetActive(true);
        shuriken.transform.SetParent(transform);

        var rb = shuriken.GetComponent<Rigidbody>();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        LerpHandPosition(shuriken.transform, HandShurikens.Count, 0);
        HandShurikens.Add(shuriken);
    }


    private void LerpHandPosition(Transform tform, int pos, float lerp)
    {

        float angleA = pos * Mathf.PI / 6f;
        float angleB = (pos + 1) * Mathf.PI / 6f;

        float angle = Mathf.Lerp(angleA, angleB, lerp);

        float heightA = -0.2f * pos;
        float heightB = -0.2f * (pos + 1);

        float height = Mathf.Lerp(heightA, heightB, lerp);

        tform.localPosition = new Vector3(Mathf.Sin(angle), height, 1 - Mathf.Cos(angle));
        tform.localRotation = Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
    }



    // Update is called once per frame
    void LateUpdate ()
    {
        if (HandShurikens.Count < MaxHandShurikens)
        {
            AddShurikenToHand();
        }

        if (HandShurikenLerp > 0)
        {
            HandShurikenLerp -= Mathf.Min(HandShurikenLerp, Time.deltaTime * 2);

            for(int i = 0; i < HandShurikens.Count; i++)
            {
                LerpHandPosition(HandShurikens[i].transform, i, HandShurikenLerp);
            }
        }

        // Debug.Log(OVRInput.GetConnectedControllers().ToString() + " " + OVRInput.GetActiveController().ToString());

        // first check if we have a right handed remote (we may have both left and right).
        var handedRemote = OVRInput.GetConnectedControllers() & OVRInput.Controller.RTrackedRemote;
        // if not, check if we have a left handed remote
        if(handedRemote == 0)
        {
            handedRemote = OVRInput.GetConnectedControllers() & OVRInput.Controller.LTrackedRemote;
        }

        #if UNITY_EDITOR

        // Use Touch controller in editor
        handedRemote = OVRInput.GetConnectedControllers() & OVRInput.Controller.RTouch;

        #endif

        if (PlatformManager.Instance.controlType == ControlType.Controller)
        {
            #region Controller Gameplay Mechanic
            if (handedRemote != 0)
            {
                var rotation = OVRInput.GetLocalControllerRotation(handedRemote);

                var camTransform = _mainCam.transform;
                transform.rotation = rotation;
                transform.position = OVRInput.GetLocalControllerPosition(handedRemote);
                //transform.position = 1.2f * Vector3.up + (handedRemote == OVRInput.Controller.LTrackedRemote ? -0.4f : 0.4f) * camTransform.right + 0.4f * camTransform.forward + rotation * Vector3.forward * 0.4f;

                switch (Mode)
                {
                    case ThrowMode.Swipe:
                        if (OVRInput.Get(OVRInput.Touch.PrimaryTouchpad, handedRemote))
                        {
                            if (!TouchPadDown)
                            {
                                TouchPadDown = true;
                                FirstTouchPosition = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, handedRemote);
                                FirstTouchTime = Time.time;
                            }
                        }
                        else
                        {
                            if (TouchPadDown)
                            {
                                var delta = (OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, handedRemote) - FirstTouchPosition) / (Time.time - FirstTouchTime);

                                if (delta.magnitude > 1f)
                                {
                                    LaunchDirection = new Vector3(delta.x, 0, delta.y).normalized;

                                    Velocity = delta.magnitude * 2f;

                                    Throw();
                                }

                                TouchPadDown = false;
                            }
                        }
                        break;
                    case ThrowMode.Trigger:
                        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, handedRemote))
                        {
                            LaunchDirection = Vector3.forward;
                            Velocity = 10f;
                            Throw();
                        }
                        break;
                    case ThrowMode.Throw:
                        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, handedRemote))
                        {
                            TouchpadDownTime = Time.time;
                            TouchpadDownRotation = rotation;
                        }
                        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, handedRemote))
                        {
                            float angle = Quaternion.Angle(rotation, TouchpadDownRotation);
                            LaunchDirection = Vector3.forward;
                            Velocity = angle * 0.05f / (Time.time - TouchpadDownTime);

                            Throw();
                        }

                        break;
                }

                if (OVRInput.Get(OVRInput.Button.PrimaryTouchpad, handedRemote))
                {
                    SelectUI.gameObject.SetActive(true);

                    Vector2 pos = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, handedRemote);

                    Mode = (ThrowMode)((int)((int)ThrowMode.Count + 0.5f + (Mathf.Atan2(-pos.x, pos.y) * (int)ThrowMode.Count) / (2 * Mathf.PI)) % (int)ThrowMode.Count);
                    SelectUI.SetSelectedMode(Mode);
                }
                else
                {
                    SelectUI.gameObject.SetActive(false);
                }

            }
            #endregion
        }
        else
        {
#region HMD Gameplay Mechanics
            
                Quaternion rotation = _mainCam.transform.rotation;

                var camTransform = _mainCam.transform;
                transform.rotation = rotation;
                transform.position = _mainCam.transform.position + _mainCam.transform.forward;
                //transform.position = 1.2f * Vector3.up + (handedRemote == OVRInput.Controller.LTrackedRemote ? -0.4f : 0.4f) * camTransform.right + 0.4f * camTransform.forward + rotation * Vector3.forward * 0.4f;

                        
                                if (OVRInput.GetUp(OVRInput.Button.PrimaryTouchpad) || Input.GetMouseButtonUp(0))
                                {
                                    LaunchDirection = _mainCam.transform.forward;

                                    Velocity = 10f;

                                    Throw();
                                }


                if (OVRInput.Get(OVRInput.Button.Back))
                {
                    SelectUI.gameObject.SetActive(true);

                    Vector2 pos = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, handedRemote);

                    Mode = (ThrowMode)((int)((int)ThrowMode.Count + 0.5f + (Mathf.Atan2(-pos.x, pos.y) * (int)ThrowMode.Count) / (2 * Mathf.PI)) % (int)ThrowMode.Count);
                    SelectUI.SetSelectedMode(Mode);
                }
                else
                {
                    SelectUI.gameObject.SetActive(false);
                }
                
            
#endregion
        }
        
    }


    void Throw()
    {
        if (HandShurikens.Count == 0 || HandShurikenLerp > 0)
            return;

        //ScreenDebugLogger.Log("Throwing " + LaunchDirection + " Vel " + Velocity);

        var obj = HandShurikens[0];
        HandShurikens.RemoveAt(0);

        HandShurikenLerp = 1f;

        var rb = obj.GetComponent<Rigidbody>();

        rb.maxAngularVelocity = 45f;
        rb.angularVelocity = _mainCam.transform.forward + new Vector3(0, 45f, 0);
        rb.isKinematic = false;

        rb.velocity = transform.rotation * LaunchDirection * Velocity;
        obj.transform.SetParent(null, true);

        ThrownShurikens.Add(obj);

        if(ThrownShurikens.Count > MaxThrownShurikens)
        {
            var first = ThrownShurikens[0];
            ThrownShurikens.RemoveAt(0);
            first.SetActive(false);
            ShurikenPool.Add(first);
        }
    }
}
