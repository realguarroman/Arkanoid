#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

//----constantes y tipos-----
#if OS_MSWINDOWS
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RTDESKEntity))]
public class AreaJuegoRecieveMessage : MonoBehaviour
{
    enum AreaJuegoStates { Active, InActive };
    string myName;
    AreaJuegoStates state;

    RTDESKEngine Engine;
    HRT_Time fiveMillis;

    private void Awake()
    {
        //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    // Start is called before the first frame update
    void Start()
    {
        state = AreaJuegoStates.Active;

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        fiveMillis = Engine.ms2Ticks(5);
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)UserMsgTypes.Position:
                Engine.PushMsg(Msg);

                Vector3 pos = ((Transform)Msg).V3;
                //no dejamos que salga de los límites del terreno de juego
                //ver la manera de asignar estos límites de manera global al proyecto
                //o sacarlos de otro gameobject que haga de fondo
                Vector3 speed = new Vector3(1f, 1f, 1f);
                if (pos.x + 1/2 >= transform.localScale.x / 2 || pos.x - 1/2 <= -transform.localScale.x / 2)
                    speed.x = -speed.x;
                if (pos.y + 1/2 >= transform.localScale.y || pos.y - 1/2 <= 0)
                    speed.y = -speed.y;

                if (state == AreaJuegoStates.Active && (speed.x != 1f || speed.y != 1f))
                {
                    MessageManager MM = RTDESKEntity.getMailBox(Msg.Sender);
                    Transform TMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Rotation);
                    TMsg.V3 = speed;
                    Engine.SendMsg(TMsg, gameObject, MM, fiveMillis);
                    state = AreaJuegoStates.InActive;
                }
                else if (state == AreaJuegoStates.InActive && speed.x == 1f && speed.y == 1f) {
                    state = AreaJuegoStates.Active;
                }

                break;
            default:
                Engine.PushMsg(Msg);
                break;
        }
    }
}