using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WedgeItem : MonoBehaviour 
{

    public Image Image;
    public Text Text;

    public Color EnabledColor;
    public Color DisabledColor;


    public void Initialize(string name, int index, int count)
    {
        Text.text = name;

        transform.localEulerAngles = new Vector3(0, 0, index * 360f / count);
        Image.transform.localEulerAngles = new Vector3(0, 0, 0.5f * 360f / count + 1f);

        Image.fillAmount = 1 / (float)count - 2 / 360f;
    }

    public void SetSelected(bool selected)
    {
        Image.color = selected ? EnabledColor : DisabledColor;
    }
}
