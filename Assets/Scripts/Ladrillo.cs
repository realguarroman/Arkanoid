

#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

#if OS_MSWINDOWS
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(RTDESKEntity))]
public class Ladrillo : MonoBehaviour
{
    HRT_Time tenMillis;
    RTDESKEngine Engine;

    private void Awake()
    {
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    void Start()
    {
        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Debug.Log(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();
        tenMillis = Engine.ms2Ticks(100);


    }

    void ReceiveMessage(MsgContent Msg)
    {
        Debug.Log("Mensaje recibido");
        if (Msg.Type==(int)UserMsgTypes.Action) {
            Action a = (Action)Msg;
            if (a.action==(int)UserActions.Destroy) {
                Destroy(gameObject);  
            }
        }
        Engine.PushMsg(Msg);
    }

    private void OnCollisionEnter(Collision colision)
    {
        //Destroy(gameObject);
        Debug.Log("Colision detectada");
        Action msg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        msg.action = (int)UserActions.Destroy;
        Engine.SendMsg(msg, tenMillis);
        Debug.Log("Mensaje enviado");
    }
}
