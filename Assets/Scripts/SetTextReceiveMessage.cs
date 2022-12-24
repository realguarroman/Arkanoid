using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class SetTextReceiveMessage : MonoBehaviour
    {
        RTDESKEngine Engine;
        UnityEngine.UI.Text textcomponent;

        private void Awake()
        {
            //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
            GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        }

        void Start()
        {
            GameObject engine = GameObject.Find(RTDESKEngine.Name);
            Engine = engine.GetComponent<RTDESKEngine>();

            textcomponent = GetComponent<UnityEngine.UI.Text>();
        }

        // Use this for initialization
        void ReceiveMessage(MsgContent Msg)
        {
            switch (Msg.Type)
            {
                case (int)UserMsgTypes.String:
                    Engine.PushMsg(Msg);
                    textcomponent.text = "<color=##e28743>" + ((StringMsg)Msg).msg + "</color>";
                    break;

                default:
                    Engine.PushMsg(Msg);
                    break;
            }
        }
    }
}