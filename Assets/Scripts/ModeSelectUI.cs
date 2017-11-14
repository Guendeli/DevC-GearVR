using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelectUI : MonoBehaviour 
{
    public GameObject WedgePrefab;

    private WedgeItem[] Wedges = new WedgeItem[(int)ShurikenLauncher.ThrowMode.Count];

    public void Awake()
    {
        for (int i = 0; i < Wedges.Length; i++)
        {
            var inst = GameObject.Instantiate(WedgePrefab);
            inst.transform.SetParent(transform, false);
            var wedge = inst.GetComponent<WedgeItem>();

            wedge.Initialize(((ShurikenLauncher.ThrowMode)i).ToString(), i, Wedges.Length);
            wedge.SetSelected(false);
            Wedges[i] = wedge;
        }
    }

    public void SetSelectedMode(ShurikenLauncher.ThrowMode mode)
    {
        for (int i = 0; i < Wedges.Length; i++)
        {
            Wedges[i].SetSelected(i == (int)mode);
        }
    }



}
