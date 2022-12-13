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

[RequireComponent(typeof(RTDESKEntity))]
public class RaquetaReceiveMessage : MonoBehaviour {
    enum RaquetaStates { Active, InActive }
    enum RaquetaActions { ChangeDir }

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
        state = RaquetaStates.Active;
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
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.UpArrow);

        //Get a new message to activate a new action in the object
        Action ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Update the content of the message sending and activation 
        ActMsg.action = (int)RaquetaActions.ChangeDir;

        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, halfSecond);
    }

    void ReceiveMessage(MsgContent Msg) {

        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
                switch (((RTDESKInputMsg)Msg).c) {
                    case KeyCode.LeftArrow:
                        direction.y = 1f;
                        break;
                    case KeyCode.RightArrow:
                        direction.y = -1f;
                        break;
                    case KeyCode.UpArrow:
                        direction.y = 0f;
                        break;
                }
                Engine.PushMsg(Msg);
                break;

            case (int)UserMsgTypes.Action:
                switch (((Action)Msg).action) {
                    case (int)RaquetaActions.ChangeDir:
                        if (transform.position.x + transform.localScale.y/2 >= 10)
                            direction.y = 1f;
                        else if (transform.position.x - transform.localScale.y/2 <= -10)
                            direction.y = -1f;


                        transform.Translate(direction*speed);
                        Engine.SendMsg(Msg, fiveMillis);
                        break;

                    default:
                        Engine.PushMsg(Msg);
                        break;
                }
                break;

            case (int)UserMsgTypes.Rotation:
                Engine.PushMsg(Msg);
                Debug.Log("Rotar");

                Transform r = (Transform)Msg;
                //direction.x *= r.V3.x;
                direction.y *= r.V3.x;
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
