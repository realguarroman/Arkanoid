/**
 * BolaReceiveMessage: Basic Message Management for the Bola game object
 *
 * Copyright(C) 2022
 *
 * Prefix: CRM_

 * @Author: Dr. Ram�n Moll� Vay�
 * @Date:	11/2022
 * @Version: 2.0
 *
 * Update:
 * Date:	
 * Version: 
 * Changes:
 *
 */

#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

using UnityEngine;

//----constantes y tipos-----
#if OS_MSWINDOWS
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

enum BolaActions { Idle, Start, Sleep, Watch, Run, Walk, Turn, ChangeColor, ChangeDirectionX, ChangeDirectionY, ChangeDirectionXY }



public class BolaLiveState
{
    public float life,  //<Amount of live in the interval [0,1]
                 gas;   //<Amount of live in the interval [0,1]
    public uint ammo;  //<Amount of bullets remaining [0,100)
}

// BolaReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class BolaReceiveMessage : MonoBehaviour
{
    public enum BolaStates
    {
        Moving,
        Steady
    }
    public int sp;
    //Initial Bola state
    BolaStates state = BolaStates.Moving;

    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis, Millis33;

    [SerializeField]
    Vector3 speed;
    RTDESKEngine Engine;   //Shortcut

    BolaLiveState lifeState;

    Renderer renderComponent;

    string myName;

    private void Awake()
    {
        //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    // Start is called before the first frame update
    void Start()
    {
        Transform PosMsg;
        Action ActMsg;
        GameObject engine = GameObject.Find(RTDESKEngine.Name);

        Engine = engine.GetComponent<RTDESKEngine>();
        RTDESKInputManager IM = engine.GetComponent<RTDESKInputManager>();

        //Register keys that we want to be signaled in case the user press them
        // IM.RegisterKeyCode(ReceiveMessage, KeyCode.UpArrow);
        // IM.RegisterKeyCode(ReceiveMessage, KeyCode.DownArrow);
        // IM.RegisterKeyCode(ReceiveMessage, KeyCode.LeftArrow);
        // IM.RegisterKeyCode(ReceiveMessage, KeyCode.RightArrow);

        //Debug.Log("Solicitud de Posici�n");
        //Get a new message to change position
        PosMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
        //Update the content of the message
        PosMsg.V3 = speed; // new Vector3(0.0005f, 0.0002f, 0.001f);

     

        //Debug.Log("Solicitud de Acci�n por cubo");
        //Get a new message to activate a new action in the object
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Update the content of the message sending and activation 
        ActMsg.action = (int)BolaActions.ChangeColor;

        myName = gameObject.name;

        renderComponent = GetComponent<Renderer>();

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        Millis33 = Engine.ms2Ticks(33);

        oneSecond = Engine.ms2Ticks(1000);

        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, halfSecond);
        Engine.SendMsg(PosMsg, gameObject, ReceiveMessage, tenMillis);
       
    }

    void ReceiveMessage(MsgContent Msg)
    {
        Transform p;

        switch (Msg.Type)
        {
            
            case (int)UserMsgTypes.Position:
                //Avoids mesages sent by error. Only recognize self messages
                //Debug.Log("Sender name " + Msg.Sender.name + " My name " + myName);
                if (myName == Msg.Sender.name)
                {
                    //Update the content of the message
                    p = (Transform)Msg;
                    if (BolaStates.Moving == state)
                    //no dejamos que salga de los límites del terreno de juego
                    //ver la manera de asignar estos límites de manera global al proyecto
                    //o sacarlos de otro gameobject que haga de fondo
                    {
                        if (transform.position.x > GameObject.Find("AreaJuego").transform.localScale.x/2)
                        {
                            //p.V3.x = -p.V3.x;
                            speed.x = -speed.x;
                        }
                        if (transform.position.x < -GameObject.Find("AreaJuego").transform.localScale.x/2)
                        {
                            //p.V3.x = -p.V3.x;
                            speed.x = -speed.x;
                        }
                        if (transform.position.y > GameObject.Find("AreaJuego").transform.localScale.y)
                        {
                            //p.V3.y = -p.V3.y;
                            speed.y = -speed.y;
                        }
                        if (transform.position.y < 0)
                        {
                           // p.V3.y = -p.V3.y;
                            speed.y = -speed.y;
                        }
                        transform.Translate(speed);
                        Engine.SendMsg(Msg, tenMillis);
                        //Este mensaje se reutiliza para volver a mandarse a s� mismo. No hace falta devolver al pool empleando PutMsg(Msg);
                    }
                    else Engine.PushMsg(Msg);
                }
                break;
            case (int)UserMsgTypes.Rotation:
                break;
            case (int)UserMsgTypes.Scale:
                break;
            case (int)UserMsgTypes.TRE:
                break;
            case (int)UserMsgTypes.Action:
                Action a;
                a = (Action)Msg;
                //Sending automessage
                if (myName == Msg.Sender.name)
                    switch ((int)a.action)
                    {
                        case (int)BolaActions.Idle:
                            if (Engine.GetRealTime() - userTime > halfSecond)
                            {
                                a.action = (int)BolaActions.Walk;
                                //Reuse the received message to resend it again to itself
                                Engine.SendMsg(Msg, halfSecond);
                            }
                            break;
                        case (int)BolaActions.Sleep:
                            break;
                        case (int)BolaActions.Watch:
                            break;
                        case (int)BolaActions.Run:
                            break;
                        case (int)BolaActions.Walk:
                            //Reactive behaviour. No responsive, no proactive
                            //Ya no se va a volver a gastar este tipo de mensaje. Devolver al pool
                            Engine.PushMsg(Msg);
                            break;
                        case (int)BolaActions.Turn:
                            break;
                        case (int)BolaActions.ChangeColor:
                            Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
                            renderComponent.material.SetColor("_Color", randomColor);
                            Engine.SendMsg(Msg, (HRT_Time)(((double)oneSecond) * (1 - Random.value * 0.5d)));
                            //Este mensaje se reutiliza para volver a mandarse a s� mismo. No hace falta devolver al pool empleando PutMsg(Msg);
                            break;

                       

                        case (int)BolaActions.Start:
                            //We have to start the Bola behaviour

                            break;
                        default:
                            break;
                    }
                else
                {
                    switch ((int)a.action)
                    {
                        case (int)UserActions.GetSteady: //Stop the movement of the object
                            state = BolaStates.Steady;
                            break;
                        case (int)UserActions.Move:
                            state = BolaStates.Moving;
                            Transform TMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                            TMsg.V3 = new Vector3(0.0005f, 0.0002f, 0.001f);//speed;
                            Engine.SendMsg(TMsg, gameObject, ReceiveMessage, tenMillis);
                            break;
                        case (int)BolaActions.ChangeDirectionY:
                            //  Debug.Log("Mensaje recibido");
                            speed.y = -speed.y;
                            //speed = Quaternion.Euler(0, 0, 20) * speed;
                            break;
                        case (int)BolaActions.ChangeDirectionX:
                            //  Debug.Log("Mensaje recibido");
                            speed.x = -speed.x;
                            break;
                        case (int)BolaActions.ChangeDirectionXY:
                            //  Debug.Log("Mensaje recibido");
                            speed.x = -speed.x;
                            speed.y = -speed.y;
                            break;
                    }
                    Engine.PushMsg(Msg);
                }
                break;
        }
    }
}
