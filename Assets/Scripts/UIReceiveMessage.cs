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
public class UIReceiveMessage : MonoBehaviour
{
  
    public enum UIStates {  };

    string myName;
    UIStates state;
    MessageManager BolaManagerMailBox;

    int lives = 5;
    int score = 0;


    //Textos
    public GameObject CurrentScore;
    public GameObject CurrentLives;

    //Components
    TextMeshProUGUI textmeshpro_CurrentScore_text;
    TextMeshProUGUI textmeshpro_CurrentLives_text;


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
       // state = UIStates.Active;
        BolaManagerMailBox = RTDESKEntity.getMailBox("Bola");


        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        fiveMillis = Engine.ms2Ticks(5);
        halfSecond = Engine.ms2Ticks(500);


        textmeshpro_CurrentScore_text = CurrentScore.GetComponent<TextMeshProUGUI>();
        textmeshpro_CurrentLives_text = CurrentLives.GetComponent<TextMeshProUGUI>();
        textmeshpro_CurrentScore_text.text = "Score: 0";
        textmeshpro_CurrentLives_text.text = "Lives: " + lives.ToString();



    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            
            case (int)UserMsgTypes.Action:
                Engine.PushMsg(Msg);

                Action a = (Action)Msg;
                if (a.action == (int)UIActions.LoseLife)
                {
                    lives--;
                    textmeshpro_CurrentLives_text.text = "Lives: " + lives.ToString();

                    if (lives < 0)
                    {
                        textmeshpro_CurrentLives_text.text = "Game Over";
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }

                }
                    


                if (a.action == (int)UIActions.BeatenBrick)
                {
                    score = score + 10;
                    textmeshpro_CurrentScore_text.text = "Score: " + score.ToString();
                    if (score == 350)
                    {
                        textmeshpro_CurrentLives_text.text = "You Win!";
                    }
                }

                 


                break;

            default:
                Engine.PushMsg(Msg);
                break;
        }
    }
}
