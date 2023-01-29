#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

#define PANDA

//----constantes y tipos-----
#if OS_MSWINDOWS
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

using UnityEngine;
using UnityEngine.AI;
using Panda;
using GAIA;

public class NPC2_BT : MonoBehaviour {

    // GameObject attributes
    float range;
    float speedWandering;
    float speedBola;

    bool possibility_of_persecution;
    bool can_play;

    Vector3 target;

    float WRight;
    float WLeft;
    float WTop;
    float WBottom;

    private BoxCollider2D mycollider;

    RTDESKEngine Engine;
    HRT_Time fiveMillis;

    MessageManager BolaManagerMailBox;

    private GAIA_Manager manager;               // Instatiates the manager.
    public string NameFile;

    private void visible(bool visibility) {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(visibility);
        }
    }

    public void OnEnable()
    {
        if (mycollider != null) {
            can_play = true;
            visible(true);
        }
    }

    public void OnDisableFunc()
    {
        can_play = false;
        visible(false);
        enabled = false;
    }

    // Start is called before the first frame update
    void Start() {
        manager = GAIA_Controller.INSTANCE.m_manager;

#if (PANDA)
        manager.createBT(gameObject, NameFile);
#endif

        range = 0.1f;
        speedWandering = 2;
        speedBola = 6;

        possibility_of_persecution = true;

        var temp = GameObject.Find("Field" + tag);
        WRight = temp.transform.position.x + (temp.transform.localScale.x / 2) - 2f;
        WLeft = temp.transform.position.x - (temp.transform.localScale.x / 2) + 2f;
        WTop = temp.transform.position.y + (temp.transform.localScale.y / 2) - 4f;
        WBottom = temp.transform.position.y - (temp.transform.localScale.y / 4);

        mycollider = gameObject.GetComponent<BoxCollider2D>();

        set_target();

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        BolaManagerMailBox = RTDESKEntity.getMailBox("Bola" + tag);

        fiveMillis = Engine.ms2Ticks(5);
        visible(false);

        GetComponent<Animation>().Play();
    }

    void set_target() {
        target = new Vector3(
        Random.Range(WLeft, WRight),
        Random.Range(WBottom, WTop),
        transform.position.z);
    }

    float distancia() {
        return Vector3.Distance(transform.position,
            GameObject.Find("Bola" + tag).transform.position) - 5;
    }

    [Task]
    bool CanPlay() {
        return can_play;
    }

    [Task]
    bool BolaCerca() {
        if (distancia() < 0)
            return true;

        return false;
    }

    [Task]
    bool BolaLejos()
    {
        if (distancia() > 0)
            return true;

        return false;
    }

    [Task]
    bool PerseguirBola() {
        transform.position = Vector3.MoveTowards(transform.position, 
            GameObject.Find("Bola" + tag).transform.position, 
            speedBola * Time.deltaTime);

        return true;
    }

    [Task]
    bool IsPossPersecution() {
        return possibility_of_persecution;
    }

    [Task]
    bool ActivatePossOfPersecution() {
        possibility_of_persecution = true;
        return true;
    }

    [Task]
    bool Wandering() {
        transform.position = Vector3.MoveTowards(transform.position, 
            target, speedWandering * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < range)
            set_target();
 
        return true;
    }

    public void OnTriggerEnter2DFunc(Collider2D other) {
        if (other.name.StartsWith("Bola")) {
            possibility_of_persecution = false;

            float RectWidth = mycollider.size.y;
            float RectHeight = mycollider.size.x;
            float RectX = transform.position.x - (RectWidth / 2f);
            float RectY = transform.position.y - (RectHeight / 2f);

            float CircleRadius = 1f / 2f;
            float CircleX = other.gameObject.transform.position.x;
            float CircleY = other.gameObject.transform.position.y;

            float NearestX = Mathf.Max(RectX, Mathf.Min(CircleX, RectX + RectWidth));
            float NearestY = Mathf.Max(RectY, Mathf.Min(CircleY, RectY + RectHeight));

            float DeltaX = CircleX - NearestX;
            float DeltaY = CircleY - NearestY;
            bool intersection = (DeltaX * DeltaX + DeltaY * DeltaY) < (CircleRadius * CircleRadius);

            Transform TMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Rotation);
            if (NearestX == CircleX)
                TMsg.V3 = new Vector3(1f, -1f, 1f);
            else if (NearestY == CircleY)
                TMsg.V3 = new Vector3(-1f, 1f, 1f);
            else
                TMsg.V3 = new Vector3(-1f, -1f, 1f);

            Engine.SendMsg(TMsg, gameObject, BolaManagerMailBox, fiveMillis);
        }
    }
}
