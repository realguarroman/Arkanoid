using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class TextReceiveMessage : MonoBehaviour
    {
        RTDESKEngine Engine;
        Text textcomponent;

        private void Awake()
        {
            //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
            GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        }

        void Start()
        {
            GameObject engine = GameObject.Find(RTDESKEngine.Name);
            Engine = engine.GetComponent<RTDESKEngine>();

            textcomponent = GetComponent<Text>();
        }

        // Use this for initialization
        void ReceiveMessage(MsgContent Msg) {
            switch (Msg.Type) {
                case (int)UserMsgTypes.String:
                    Engine.PushMsg(Msg);
                    gameObject.SetActive(true);
                    textcomponent.text = ((StringMsg)Msg).msg;
                    break;

                default:
                    Engine.PushMsg(Msg);
                    break;
            }
        }
    }
}