using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScreenDebugLogger : MonoBehaviour {

    public static ScreenDebugLogger Inst;

    private Text Text;

    public float MessageLifetime = 15f;

    private struct Message
    {
        public readonly float Time;
        public readonly string Body;

        public Message(float time, string body)
        {
            Time = time;
            Body = body;
        }
    }

    private List<Message> Messages = new List<Message>();

    private void Awake()
    {
        Inst = this;
        Text = GetComponent<Text>();
    }


    private void Update()
    {
        StringBuilder sb = new StringBuilder();

        for(int i = 0; i < Messages.Count; i++)
        {
            if(Messages[i].Time + MessageLifetime < Time.time)
            {
                Messages.RemoveAt(i);
                i--;
            }
            else
            {
                sb.AppendLine(Messages[i].Body);
            }
        }
        Text.text = sb.ToString();
    }

    public static void Log(string message)
    {
        if(Inst)
        {
            Inst.Messages.Add(new Message(Time.time, message));
            if(Inst.Messages.Count > 15)
            {
                Inst.Messages.RemoveAt(0);
            }
        }
    }



}
