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

using UnityEngine;
using System.Collections.Generic;

public enum BolaActions { Start, ChangeColor }

// BolaReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class BolaReceiveMessage : MonoBehaviour {

    enum BolaStates { Idle, Moving }

    float WRight = 10f;
    float WLeft = -10f;
    float WTop = 30f;

    string myName;
    BolaStates state;
    Renderer renderComponent;
    MessageManager MMRaqueta;
    List<MessageManager> MMLadrillos;

    [SerializeField]
    Vector3 direction;
    float speed = 0.05f;

    RTDESKEngine Engine;
    HRT_Time oneSecond, halfSecond, fiveMillis;

    private void Awake() {
        //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    // Start is called before the first frame update
    void Start() {
        myName = gameObject.name;
        state = BolaStates.Idle;
        renderComponent = GetComponent<Renderer>();
        MMRaqueta = RTDESKEntity.getMailBox("Raqueta");
        MMLadrillos = new List<MessageManager>();

        direction = new Vector3(1f, 1f, 0);

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        fiveMillis = Engine.ms2Ticks(5);
        halfSecond = Engine.ms2Ticks(500);
        oneSecond = Engine.ms2Ticks(1000);    
    }

    void ReceiveMessage(MsgContent Msg) {
        Transform p;

        if (Msg.Type == (int)UserMsgTypes.Object) {
            Engine.PushMsg(Msg);
            MessageManager mm = RTDESKEntity.getMailBox(((ObjectMsg)Msg).o);
            MMLadrillos.Add(mm);
        }

        else if (state == BolaStates.Idle) {
            Engine.PushMsg(Msg);

            switch (Msg.Type) {
                case (int)UserMsgTypes.Position:
                    p = (Transform)Msg;
                    transform.position = new Vector3(p.V3.x, 
                        p.V3.y + transform.localScale.y / 2f, 0);
                    break;
                case (int)UserMsgTypes.Action:
                    if (((Action)Msg).action == (int)BolaActions.Start) {
                            state = BolaStates.Moving;

                            //Get a new message to change position
                            Transform PosMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                            //Update the content of the message
                            PosMsg.V3 = direction;

                            //Get a new message to activate a new action in the object
                            Action ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                            //Update the content of the message sending and activation 
                            ActMsg.action = (int)BolaActions.ChangeColor;

                            Engine.SendMsg(PosMsg, gameObject, ReceiveMessage, fiveMillis);
                            Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, halfSecond);
                    }
                    break;
            }
        }

        else
            switch (Msg.Type) {
                case (int)UserMsgTypes.Position:
                    if (myName == Msg.Sender.name) {
                        if (transform.position.y - transform.localScale.y / 2f <= 0) {
                            Engine.PushMsg(Msg);
                            state = BolaStates.Idle;
                            direction = new Vector3(1f, 1f, 0);

                            Action ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                            //Update the content of the message sending and activation 
                            ActMsg.action = (int)RaquetaActions.SetIdle;
                            Engine.SendMsg(ActMsg, gameObject, MMRaqueta, fiveMillis);
                            break;
                        }

                        if (transform.position.x + transform.localScale.x / 2f >= WRight ||
                            transform.position.x - transform.localScale.x / 2f <= WLeft)
                            direction.x = -direction.x;
                        if (transform.position.y + transform.localScale.y / 2f >= WTop)
                            direction.y = -direction.y;

                        transform.Translate(direction * speed);
                    
                        foreach (MessageManager MM in MMLadrillos) {
                            p = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                            p.V3 = transform.position;
                            Engine.SendMsg(p, gameObject, MM, fiveMillis);
                        }

                        p = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                        p.V3 = transform.position;
                        Engine.SendMsg(p, gameObject, MMRaqueta, fiveMillis);

                        Engine.SendMsg(Msg, fiveMillis);
                    }
                    else Engine.PushMsg(Msg);
                    break;

                case (int)UserMsgTypes.Rotation:
                    if (Msg.Sender.name.StartsWith("Ladrillo")) {
                        MessageManager MMLadrillo = RTDESKEntity.getMailBox(Msg.Sender);
                        MMLadrillos.Remove(MMLadrillo);
                    }

                    p = (Transform)Msg;
                    direction.x *= p.V3.x;
                    direction.y *= p.V3.y;

                    Engine.PushMsg(Msg);
                    break;

                case (int)UserMsgTypes.Action:
                    Action a = (Action)Msg;
                    if (myName == Msg.Sender.name)
                        switch ((int)a.action) {
                            case (int)BolaActions.ChangeColor:
                                Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
                                renderComponent.material.SetColor("_Color", randomColor);
                                Engine.SendMsg(Msg, (HRT_Time)(((double)oneSecond) * (1 - Random.value * 0.5d)));
                                break;

                            default:
                                Engine.PushMsg(Msg);
                                break;
                        }
                    else Engine.PushMsg(Msg);
                    break;

                default:
                    Engine.PushMsg(Msg);
                    break;
            }
    }
}
