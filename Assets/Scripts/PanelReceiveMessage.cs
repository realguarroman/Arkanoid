using UnityEngine.UI;
using UnityEngine;

public enum PanelActions { Hide, Show };

namespace Assets.Scripts
{
    public class PanelReceiveMessage : MonoBehaviour
    {

        RTDESKEngine Engine;

        private void Awake()
        {
            //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
            GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        }

        void Start()
        {
            GameObject engine = GameObject.Find(RTDESKEngine.Name);
            Engine = engine.GetComponent<RTDESKEngine>();
        }

        // Use this for initialization
        void ReceiveMessage(MsgContent Msg)
        {
            switch (Msg.Type)
            {
                case (int)UserMsgTypes.Action:
                    Engine.PushMsg(Msg);
                    Action a = (Action)Msg;

                    switch (a.action) {
                        case (int)PanelActions.Hide:
                            gameObject.SetActive(false);
                            break;
                        case (int)PanelActions.Show:
                            gameObject.SetActive(true);
                            break;
                    }
                    break;

                default:
                    Engine.PushMsg(Msg);
                    break;
            }
        }
    }
}