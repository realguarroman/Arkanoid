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

public enum UIActions { BeatenBrick, LoseLife };

// UIReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class UIReceiveMessage : MonoBehaviour {
  
    MessageManager BolaManagerMailBox;
    MessageManager ScoreManagerMailBox;
    MessageManager LivesManagerMailBox;

    int lives = 5;
    int score = 0;

    RTDESKEngine Engine;
    HRT_Time halfSecond, fiveMillis;

    private void Awake() {
        //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    // Start is called before the first frame update
    void Start() {

        BolaManagerMailBox = RTDESKEntity.getMailBox("Bola");
        ScoreManagerMailBox = RTDESKEntity.getMailBox("Score");
        LivesManagerMailBox = RTDESKEntity.getMailBox("Lives");

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        fiveMillis = Engine.ms2Ticks(5);
        halfSecond = Engine.ms2Ticks(500);
    }

    void ReceiveMessage(MsgContent Msg) {

        switch (Msg.Type) {            
            case (int)UserMsgTypes.Action:
                Engine.PushMsg(Msg);

                Action a = (Action)Msg;
                if (a.action == (int)UIActions.LoseLife) {
                    lives--;

                    StringMsg strmsg = (StringMsg)Engine.PopMsg((int)UserMsgTypes.String);
                    strmsg.msg = "Lives: " + lives.ToString();

                    if (lives < 1) {
                        strmsg.msg = "Game Over";
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }

                    Engine.SendMsg(strmsg, gameObject, LivesManagerMailBox, fiveMillis);
                }

                if (a.action == (int)UIActions.BeatenBrick) {
                    score = score + 10;

                    StringMsg strmsg = (StringMsg)Engine.PopMsg((int)UserMsgTypes.String);
                    strmsg.msg = "Score: " + score.ToString();

                    if (score == 350) {
                        strmsg.msg = "You Win!";
                    }

                    Engine.SendMsg(strmsg, gameObject, ScoreManagerMailBox, fiveMillis);
                }

                 


                break;

            default:
                Engine.PushMsg(Msg);
                break;
        }
    }
}
