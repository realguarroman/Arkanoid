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

// BolaReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class BolaReceiveMessage : MonoBehaviour {
    enum BolaActions { Idle, Start, Sleep, Watch, Run, Walk, Turn, ChangeColor }

    string myName;
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
        renderComponent = GetComponent<Renderer>();
        MMRaqueta = RTDESKEntity.getMailBox("Raqueta");
        MMLadrillos = new List<MessageManager>();

        direction = new Vector3(1f, 1f, 0);

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        fiveMillis = Engine.ms2Ticks(5);
        halfSecond = Engine.ms2Ticks(500);
        oneSecond = Engine.ms2Ticks(1000);

        //Get a new message to change position
        Transform PosMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
        //Update the content of the message
        PosMsg.V3 = direction; // new Vector3(0.0005f, 0.0002f, 0.001f);

        //Get a new message to activate a new action in the object
        Action ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Update the content of the message sending and activation 
        ActMsg.action = (int)BolaActions.ChangeColor;

        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, halfSecond);
        Engine.SendMsg(PosMsg, gameObject, ReceiveMessage, fiveMillis);     
    }

    void ReceiveMessage(MsgContent Msg) {
        Transform p;

        switch (Msg.Type) {
            case (int)UserMsgTypes.Object:
                Engine.PushMsg(Msg);

                MessageManager mm = RTDESKEntity.getMailBox(((ObjectMsg)Msg).o);
                MMLadrillos.Add(mm);
                break;

            case (int)UserMsgTypes.Position:
                //Avoids mesages sent by error. Only recognize self messages
                //Debug.Log("Sender name " + Msg.Sender.name + " My name " + myName);
                if (myName == Msg.Sender.name) {
                    if (transform.position.x + transform.localScale.x/2 >= 10 || 
                        transform.position.x - transform.localScale.x/2 <= -10)
                        direction.x = -direction.x;
                    if (transform.position.y + transform.localScale.y / 2 >= 30 || 
                        transform.position.y - +transform.localScale.y / 2 <= 0)
                        direction.y = -direction.y;

                    transform.Translate(direction*speed);

                    Transform pos;
                    foreach (MessageManager MM in MMLadrillos) {
                        pos = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                        pos.V3 = transform.position;
                        Engine.SendMsg(pos, gameObject, MM, fiveMillis);
                    }

                    pos = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                    pos.V3 = transform.position;
                    Engine.SendMsg(pos, gameObject, MMRaqueta, fiveMillis);

                    //Este mensaje se reutiliza para volver a mandarse a s� mismo. No hace falta devolver al pool empleando PutMsg(Msg);
                    Engine.SendMsg(Msg, fiveMillis);
                }
                else Engine.PushMsg(Msg);
                break;

            case (int)UserMsgTypes.Rotation:
                if (Msg.Sender.name.StartsWith("Ladrillo")) {
                    MessageManager mmm = RTDESKEntity.getMailBox(Msg.Sender);
                    MMLadrillos.Remove(mmm);
                }

                p = (Transform)Msg;

                direction.x *= p.V3.x;
                direction.y *= p.V3.y;

                Engine.PushMsg(Msg);
                break;

            case (int)UserMsgTypes.Action:
                Action a = (Action)Msg;
                //Sending automessage
                if (myName == Msg.Sender.name)
                    switch ((int)a.action) {
                        case (int)BolaActions.ChangeColor:
                            Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
                            renderComponent.material.SetColor("_Color", randomColor);
                            Engine.SendMsg(Msg, (HRT_Time)(((double)oneSecond) * (1 - Random.value * 0.5d)));
                            //Este mensaje se reutiliza para volver a mandarse a s� mismo. No hace falta devolver al pool empleando PutMsg(Msg);
                            break;

                        default:
                            Engine.PushMsg(Msg);
                            break;
                    }
                break;

            default:
                Engine.PushMsg(Msg);
                break;
        }
    }
}
