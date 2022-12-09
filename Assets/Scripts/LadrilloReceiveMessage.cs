/**
 * LadrilloReceiveMessage: Basic Message Management for the Ladrillo game object
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

enum LadrilloActions {CheckCollision, ChangeColor, Destroy }



// LadrilloReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class LadrilloReceiveMessage : MonoBehaviour
{
    public enum LadrilloStates{
        Destroyed,
        Steady
    }

    //Initial Ladrillo state
    LadrilloStates state = LadrilloStates.Steady;

    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis, oneMilli;

    [SerializeField]
     RTDESKEngine       Engine;   //Shortcut

   

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
      
        Action          ActMsg;
        GameObject      engine = GameObject.Find(RTDESKEngine.Name);

        Engine = engine.GetComponent<RTDESKEngine>();
        RTDESKInputManager IM = engine.GetComponent<RTDESKInputManager>();

        //Register keys that we want to be signaled in case the user press them
        // IM.RegisterKeyCode(ReceiveMessage, KeyCode.UpArrow);
        // IM.RegisterKeyCode(ReceiveMessage, KeyCode.DownArrow);
        // IM.RegisterKeyCode(ReceiveMessage, KeyCode.LeftArrow);
        // IM.RegisterKeyCode(ReceiveMessage, KeyCode.RightArrow);

      
        //Debug.Log("Solicitud de Acci�n por cubo");
        //Get a new message to activate a new action in the object
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Update the content of the message sending and activation 
        ActMsg.action = (int)LadrilloActions.ChangeColor;

        myName = gameObject.name;

        renderComponent = GetComponent<Renderer>();

        halfSecond = Engine.ms2Ticks(500);
        tenMillis  = Engine.ms2Ticks(10);
        oneMilli = Engine.ms2Ticks(1);
        oneSecond  = Engine.ms2Ticks(1000);

        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, halfSecond);


        //Get a new message to activate a new action in the object
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Update the content of the message sending and activation 
        ActMsg.action = (int)LadrilloActions.CheckCollision;
        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, tenMillis);

    }

    void ReceiveMessage(MsgContent Msg)
    {
        //Transform p;

        switch (Msg.Type)
        {
           
          
           
            case (int)UserMsgTypes.Action:
                Action a;
                a = (Action)Msg;
                //Sending automessage
                if (myName == Msg.Sender.name)
                    switch ((int)a.action)
                    {
 
                        case (int)LadrilloActions.CheckCollision:

                            //Vemos si la bola ha colisionado con este ladrillo
                            //en caso afirmativo lo destruimos
                         
           
                            float lsy = transform.localScale.y / 2;
                            float bsy = GameObject.Find("Bola").transform.localScale.y / 2;
                            float lcy = transform.position.y;
                            float bcy = GameObject.Find("Bola").transform.position.y;

                            float lsx = transform.localScale.x / 2;
                            float bsx = GameObject.Find("Bola").transform.localScale.x / 2;
                            float lcx = transform.position.x;
                            float bcx = GameObject.Find("Bola").transform.position.x;
                            bool vert = (Mathf.Abs(lcy - bcy) < (lsy + bsy));
                            bool hor = (Mathf.Abs(lcx - bcx) < (lsx + bsx));
                            if (vert && hor)
                            {
                                Engine.PushMsg(Msg); //descartamos el mensaje

                                if ((bcx<(lcx+lsx)) && (bcx>(lcx-lsx)))
                                {
                                    GameObject b = GameObject.Find("Bola");
                                   // Action msg_to_bola = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                                   // msg_to_bola.action = (int)BolaActions.ChangeDirectionY; 
                                   // Engine.SendMsg(msg_to_bola, gameObject, b, tenMillis);



                                    Transform TMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Rotation);
                                    TMsg.V3 = new Vector3(1f,-1f, 1f);
                                    Engine.SendMsg(TMsg, gameObject, b, tenMillis);


                                    Debug.Log("mensaje enviado Y");
                                }
                                    else
                                {
                                    if ((bcy < (lcy + lsy)) && (bcy > (lcy - lsy)))
                                    {
                                        GameObject b = GameObject.Find("Bola");
                                       // Action msg_to_bola = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                                       // msg_to_bola.action = (int)BolaActions.ChangeDirectionX;
                                       // Engine.SendMsg(msg_to_bola, gameObject, b, tenMillis);
                                        Debug.Log("mensaje enviado X");

                                        Transform TMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Rotation);
                                        TMsg.V3 = new Vector3(-1f, 1f, 1f);
                                        Engine.SendMsg(TMsg, gameObject, b, tenMillis);
                                    } else
                                    {
                                        GameObject b = GameObject.Find("Bola");
                                       // Action msg_to_bola = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                                       // msg_to_bola.action = (int)BolaActions.ChangeDirectionXY;
                                       // Engine.SendMsg(msg_to_bola, gameObject, b, tenMillis);
                                        Debug.Log("mensaje enviado XY");

                                        Transform TMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Rotation);
                                        TMsg.V3 = new Vector3(-1f, -1f, 1f);
                                        Engine.SendMsg(TMsg, gameObject, b, tenMillis);
                                    }
                                }
                                Action ActMsg;
                                //Get a new message to activate a new action in the object
                                ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                                //Update the content of the message sending and activation 
                                ActMsg.action = (int)LadrilloActions.Destroy;
                                Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, tenMillis);
                                
                            } else
                            {
                                Engine.SendMsg(Msg, tenMillis);
                            }
     
                            break;

                            case (int)LadrilloActions.Destroy:
                                Engine.PushMsg(Msg); //descartamos el mensaje
                                Destroy(gameObject);
                            break;

                        //   case (int)LadrilloActions.ChangeColor:
                        //       Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
                        //       renderComponent.material.SetColor("_Color", randomColor);
                        //       Engine.SendMsg(Msg, (HRT_Time)(((double)oneSecond)*(1-Random.value*0.5d)));
                        //       //Este mensaje se reutiliza para volver a mandarse a s� mismo. No hace falta devolver al pool empleando PutMsg(Msg);
                        //      break;

                        default:
                            break;
                    }
                else
                {
                    switch ((int)a.action)
                    {
                        case (int)UserActions.GetSteady: //Stop the movement of the object
                            state = LadrilloStates.Steady;
                            break;
                      
                    }
                    Engine.PushMsg(Msg);
                }
        break;
        }
    }
}
