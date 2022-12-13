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

public enum RaquetaActions { Move, SetIdle }

[RequireComponent(typeof(RTDESKEntity))]
public class RaquetaReceiveMessage : MonoBehaviour {

    float WRight = 10f;
    float WLeft = -10f;

    enum RaquetaStates { Idle, Active, InActive }

    RaquetaStates state;
    MessageManager BolaManagerMailBox;

    [SerializeField]
    Vector3 direction;
    float speed = 0.05f;

    RTDESKEngine Engine;
    HRT_Time fiveMillis, halfSecond;

    private void Awake() {
        //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    // Start is called before the first frame update
    void Start() {
        state = RaquetaStates.Idle;
        BolaManagerMailBox = RTDESKEntity.getMailBox("Bola");

        direction = new Vector3(0f, 0f, 0f);

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        fiveMillis = Engine.ms2Ticks(5);
        halfSecond = Engine.ms2Ticks(500);

        RTDESKInputManager IM = engine.GetComponent<RTDESKInputManager>();
        // Register keys that we want to be signaled in case the user press them

        IM.RegisterKeyCode(ReceiveMessage, KeyCode.LeftArrow);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.RightArrow);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.DownArrow);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.Space);

        transform.position = new Vector3(
            Random.Range(WLeft + transform.localScale.y/2f, 
            WRight - transform.localScale.x/2f ), 
            transform.position.y, 0);

        //Get a new message to activate a new action in the object
        Transform TransMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
        //Update the content of the message sending and activation 
        TransMsg.V3 = new Vector3(transform.position.x,
            transform.position.y + transform.localScale.x / 2, 0);
        Engine.SendMsg(TransMsg, gameObject, BolaManagerMailBox, fiveMillis);
    }

    void ReceiveMessage(MsgContent Msg) {
        if (state == RaquetaStates.Idle) {
            Engine.PushMsg(Msg);

            if (Msg.Type == (int)RTDESKMsgTypes.Input &&
                ((RTDESKInputMsg)Msg).c == KeyCode.Space) {
                Action ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                //Update the content of the message sending and activation 
                ActMsg.action = (int)BolaActions.Start;
                Engine.SendMsg(ActMsg, gameObject, BolaManagerMailBox, fiveMillis);

                state = RaquetaStates.InActive;
                //Get a new message to activate a new action in the object
                ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                //Update the content of the message sending and activation 
                ActMsg.action = (int)RaquetaActions.Move;
                Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, fiveMillis);
            }
        }

        else
            switch (Msg.Type)
            {
                case (int)RTDESKMsgTypes.Input:
                    Engine.PushMsg(Msg);

                    switch (((RTDESKInputMsg)Msg).c) {
                        case KeyCode.LeftArrow:
                            direction.y = 1f;
                            break;
                        case KeyCode.RightArrow:
                            direction.y = -1f;
                            break;
                        case KeyCode.DownArrow:
                            direction.y = 0f;
                            break;
                    }
                    break;

                case (int)UserMsgTypes.Action:
                    switch (((Action)Msg).action) {
                        case (int)RaquetaActions.Move:
                            if (transform.position.x + transform.localScale.y/2f >= WRight)
                                direction.y = 1f;
                            else if (transform.position.x - transform.localScale.y/2f <= WLeft)
                                direction.y = -1f;

                            transform.Translate(direction*speed);
                            Engine.SendMsg(Msg, fiveMillis);
                            break;

                        case (int)RaquetaActions.SetIdle:
                            Engine.PushMsg(Msg);
                            state = RaquetaStates.Idle;
                            direction.y = 0f;

                            Transform TransMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);
                            //Update the content of the message sending and activation 
                            TransMsg.V3 = new Vector3(transform.position.x,
                                transform.position.y + transform.localScale.x / 2, 0);
                            Engine.SendMsg(TransMsg, gameObject, BolaManagerMailBox, fiveMillis);
                            break;

                        default:
                            Engine.PushMsg(Msg);
                            break;
                    }
                    break;

                case (int)UserMsgTypes.Position:
                    Engine.PushMsg(Msg);

                    Transform p = (Transform)Msg;
                    Vector3 pos = p.V3;

                    //Vemos si la bola ha colisionado con este ladrillo
                    //en caso afirmativo lo destruimos

                    float RectWidth = transform.localScale.y;
                    float RectHeight = transform.localScale.x;
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

                    if (state == RaquetaStates.Active && intersection) {
                        Transform TMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Rotation);
                        if (NearestX == CircleX)
                            TMsg.V3 = new Vector3(1f, -1f, 1f);
                        else if (NearestY == CircleY)
                            TMsg.V3 = new Vector3(-1f, 1f, 1f);
                        else
                            TMsg.V3 = new Vector3(-1f, -1f, 1f);

                        Engine.SendMsg(TMsg, gameObject, BolaManagerMailBox, fiveMillis);
                        state = RaquetaStates.InActive;

                    } else if (state == RaquetaStates.InActive && !intersection) {
                        state = RaquetaStates.Active;
                    }

                    break;

                default:
                    Engine.PushMsg(Msg);
                    break;
            }
    }
}
