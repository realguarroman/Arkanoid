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
using UnityEngine.SceneManagement;
using TMPro;

public enum UIActions { BeatenBrick, LoseLife, SetIdle };

// UIReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class UIReceiveMessage : MonoBehaviour {

    public enum UIState { Start, Playing, Finished };

    KeyCode KSpace;

    int lives;
    int score;
    UIState state;

    MessageManager BolaManagerMailBox;
    MessageManager RaquetaManagerMailBox;
    MessageManager ScoreManagerMailBox;
    MessageManager LivesManagerMailBox;
    MessageManager PanelManagerMailBox;
    MessageManager TextManagerMailBox;
    MessageManager EngineManagerMailBox;
    MessageManager NPC1ManagerMailBox;
    MessageManager NPC2ManagerMailBox;

    RTDESKEngine Engine;
    HRT_Time fiveMillis;

    private void Awake() {
        //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    // Start is called before the first frame update
    void Start() {
        lives = 5;
        score = 0;
        state = UIState.Start;

        BolaManagerMailBox = RTDESKEntity.getMailBox("Bola" + tag);
        RaquetaManagerMailBox = RTDESKEntity.getMailBox("Raqueta" + tag);
        ScoreManagerMailBox = RTDESKEntity.getMailBox("Score" + tag);
        LivesManagerMailBox = RTDESKEntity.getMailBox("Lives" + tag);
        PanelManagerMailBox = RTDESKEntity.getMailBox("Panel" + tag);
        TextManagerMailBox = RTDESKEntity.getMailBox("Text" + tag);
        NPC1ManagerMailBox = RTDESKEntity.getMailBox("Enemy" + tag + "_1");
        NPC2ManagerMailBox = RTDESKEntity.getMailBox("Enemy" + tag + "_2");

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();
        EngineManagerMailBox = RTDESKEntity.getMailBox(RTDESKEngine.Name);

        fiveMillis = Engine.ms2Ticks(5);

        if (tag == "1") KSpace = KeyCode.Space;
        else KSpace = KeyCode.W;

        RTDESKInputManager IM = engine.GetComponent<RTDESKInputManager>();
        // Register keys that we want to be signaled in case the user press them
        IM.RegisterKeyCode(ReceiveMessage, KSpace);
    }

    void ReceiveMessage(MsgContent Msg) {

        if (Msg.Type == (int)RTDESKMsgTypes.Input && 
            ((RTDESKInputMsg)Msg).c == KSpace) {
            Engine.PushMsg(Msg);

            if (state == UIState.Start) {
                state = UIState.Playing;

                Action ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                ActMsg.action = (int)PanelActions.Hide;
                //Update the content of the message sending and activation 
                Engine.SendMsg(ActMsg, gameObject, PanelManagerMailBox, fiveMillis);

                MsgContent SyncMsg = Engine.PopMsg((int)RTDESKEngine.Actions.SynchSim2RealTime); ;
                //Update the content of the message sending and activation 
                Engine.SendMsg(SyncMsg, gameObject, EngineManagerMailBox, fiveMillis);

                ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                //Update the content of the message sending and activation 
                ActMsg.action = (int)BolaActions.Start;
                Engine.SendMsg(ActMsg, gameObject, BolaManagerMailBox, fiveMillis);

                //Get a new message to activate a new action in the object
                ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                //Update the content of the message sending and activation 
                ActMsg.action = (int)RaquetaActions.Start;
                Engine.SendMsg(ActMsg, gameObject, RaquetaManagerMailBox, fiveMillis);

                //Get a new message to activate a new action in the object
                ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                //Update the content of the message sending and activation 
                ActMsg.action = (int)NPC_Actions.Start;
                Engine.SendMsg(ActMsg, gameObject, NPC1ManagerMailBox, fiveMillis);

                //Get a new message to activate a new action in the object
                ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                //Update the content of the message sending and activation 
                ActMsg.action = (int)NPC_Actions.Start;
                Engine.SendMsg(ActMsg, gameObject, NPC2ManagerMailBox, fiveMillis);
            }

            else if (state == UIState.Finished)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }

        else if (state == UIState.Playing) {
            switch (Msg.Type)
            {
                case (int)UserMsgTypes.Action:
                    Engine.PushMsg(Msg);

                    Action a = (Action)Msg;
                    if (a.action == (int)UIActions.LoseLife)
                    {
                        lives--;

                        StringMsg strmsg = (StringMsg)Engine.PopMsg((int)UserMsgTypes.String);
                        strmsg.msg = $"<color=#feae34>Lives: {lives}</color>";
                        Engine.SendMsg(strmsg, gameObject, LivesManagerMailBox, fiveMillis);

                        strmsg = (StringMsg)Engine.PopMsg((int)UserMsgTypes.String);
                        strmsg.msg = $"Para continuar jugando presione <color=#feae34>{KSpace}</color>.";

                        if (lives < 1) {
                            strmsg.msg = $"Ha perdido la partida. Para volver a jugar presione <color=#feae34>{KSpace}</color>.";
                            state = UIState.Finished;
                        }

                        Engine.SendMsg(strmsg, gameObject, TextManagerMailBox, fiveMillis);

                        Action ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                        ActMsg.action = (int)PanelActions.Show;
                        //Update the content of the message sending and activation 
                        Engine.SendMsg(ActMsg, gameObject, PanelManagerMailBox, fiveMillis);
                    }

                    if (a.action == (int)UIActions.BeatenBrick)
                    {
                        score = score + 10;

                        StringMsg strmsg = (StringMsg)Engine.PopMsg((int)UserMsgTypes.String);
                        strmsg.msg = $"<color=#feae34>Score: {score}</color>";
                        Engine.SendMsg(strmsg, gameObject, ScoreManagerMailBox, fiveMillis);

                        if (score == 350) {
                            strmsg = (StringMsg)Engine.PopMsg((int)UserMsgTypes.String);
                            strmsg.msg = $"Ha ganado la partida. Para volver a jugar presione <color=#feae34>{KSpace}</color>.";
                            Engine.SendMsg(strmsg, gameObject, TextManagerMailBox, fiveMillis);

                            Action ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                            ActMsg.action = (int)PanelActions.Show;
                            //Update the content of the message sending and activation 
                            Engine.SendMsg(ActMsg, gameObject, PanelManagerMailBox, fiveMillis);

                            state = UIState.Finished;
                        }
                    }

                    if (a.action == (int)UIActions.SetIdle) {
                        state = UIState.Start;
                    }

                    break;

                default:
                    Engine.PushMsg(Msg);
                    break;
            }

        }
    }
}
