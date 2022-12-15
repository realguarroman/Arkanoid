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

// LadrilloReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class LadrilloReceiveMessage : MonoBehaviour
{
    enum LadrilloActions { CheckCollision, ChangeColor, Destroy };
    enum LadrilloStates { Active, Destroyed };

    string myName;
    LadrilloStates state;
    MessageManager BolaManagerMailBox;

    RTDESKEngine Engine;
    HRT_Time halfSecond, fiveMillis;

    private void Awake()
    {
        //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    // Start is called before the first frame update
    void Start()
    {
        myName = gameObject.name;
        state = LadrilloStates.Active;
        BolaManagerMailBox = RTDESKEntity.getMailBox("Bola");

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        fiveMillis = Engine.ms2Ticks(5);
        halfSecond = Engine.ms2Ticks(500);

        ObjectMsg ObjMsg = (ObjectMsg)Engine.PopMsg((int)UserMsgTypes.Object);
        ObjMsg.o = gameObject;
        Engine.SendMsg(ObjMsg, gameObject, BolaManagerMailBox, fiveMillis);
    }

    void ReceiveMessage(MsgContent Msg) {
        switch (Msg.Type)
        {
            case (int)UserMsgTypes.Position:
                Engine.PushMsg(Msg);

                if (state == LadrilloStates.Active) {
                    Transform p = (Transform)Msg;
                    Vector3 pos = p.V3;

                    //Vemos si la bola ha colisionado con este ladrillo
                    //en caso afirmativo lo destruimos

                    float RectWidth = transform.localScale.x;
                    float RectHeight = transform.localScale.y;
                    float RectX = transform.position.x - (RectWidth / 2f);
                    float RectY = transform.position.y - (RectHeight / 2f);

                    float CircleRadius = 1f / 2f;
                    float CircleX = pos.x;
                    float CircleY = pos.y;

                    float NearestX = Mathf.Max(RectX, Mathf.Min(CircleX, RectX + RectWidth));
                    float NearestY = Mathf.Max(RectY, Mathf.Min(CircleY, RectY + RectHeight));

                    float DeltaX = CircleX - NearestX;
                    float DeltaY = CircleY - NearestY;
                    bool intersection = (DeltaX * DeltaX + DeltaY * DeltaY) < (CircleRadius * CircleRadius);

                    if (intersection) {
                        Transform TMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Rotation);

                        if (NearestX == CircleX)
                            TMsg.V3 = new Vector3(1f, -1f, 1f);
                        else if (NearestY == CircleY)
                            TMsg.V3 = new Vector3(-1f, 1f, 1f);
                        else
                            TMsg.V3 = new Vector3(-1f, -1f, 1f);

                        Engine.SendMsg(TMsg, gameObject, BolaManagerMailBox, fiveMillis);

                        state = LadrilloStates.Destroyed;
                        Action ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                        ActMsg.action = (int)LadrilloActions.Destroy;
                        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, fiveMillis);
                    }
                }

                break;

            case (int)UserMsgTypes.Action:
                Engine.PushMsg(Msg);

                Action a = (Action)Msg;
                if (myName == Msg.Sender.name && a.action == (int)LadrilloActions.Destroy)
                    Destroy(gameObject);               
                break;

            default:
                Engine.PushMsg(Msg);
                break;
        }
    }
}
