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
using System.Runtime.CompilerServices;
using GAIA;
using Debug = UnityEngine.Debug;

public class NPC2_FSM : MonoBehaviour
{
    // GameObject attributes
    private float range;
    private float speed;
    private float speedBola;

    private Vector3 target;

    private float WRight;
    private float WLeft;
    private float WTop;
    private float WBottom;

    bool possibility_of_persecution;
    BoxCollider2D mycollider;
    MessageManager BolaManagerMailBox;
    HRT_Time fiveMillis;

    // IA attributes
    private FSM_Machine FSM;                                      // Variable que contiene la FSM.
    private GAIA_Manager manager;                                 // Variable que contiene la referencia al manager.
    private List<int> FSMactions;                                 // Variable que contiene las acciones a realizar en cada update.
    private List<int> FSMevents = new List<int>();                // Variable que contiene los eventos.
    private List<int> FSMEnevtsQueue = new List<int>();

    RTDESKEngine Engine;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void addNoEvent() { FSMevents.Add((int)Tags.EventTags.NULL); }

    // Start is called before the first frame update
    void Start()
    {
        manager = GAIA_Controller.INSTANCE.m_manager;
        FSM = manager.createMachine(this, (int)FA_Classic.FAType.CLASSIC, "NPC2_FSMDeterministic");
        addNoEvent();

        range = 0.1f;
        speed = 2;
        speedBola = 6;

        var temp = GameObject.Find("Field" + tag);
        WRight = temp.transform.position.x + (temp.transform.localScale.x / 2) - 2f;
        WLeft = temp.transform.position.x - (temp.transform.localScale.x / 2) + 2f;
        WTop = temp.transform.position.y + (temp.transform.localScale.y / 2) - 4f;
        WBottom = temp.transform.position.y - (temp.transform.localScale.y / 4);

        mycollider = gameObject.GetComponent<BoxCollider2D>();
        BolaManagerMailBox = RTDESKEntity.getMailBox("Bola" + tag);
        possibility_of_persecution = true;

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        fiveMillis = Engine.ms2Ticks(5);

        GetComponent<Animation>().Play("EnemyRotation");
        set_target();
        visible(false);
        hide();
    }

    public void OnEnable() {
        FSMEnevtsQueue.Add((int)Tags.EventTags.PLAY_EVENT);
    }

    public void OnDisableFunc() {
        visible(false);
        FSMEnevtsQueue.Add((int)Tags.EventTags.IDLE_EVENT);
    }

    void set_target() {
        target = new Vector3(
            Random.Range(WLeft, WRight), 
            Random.Range(WBottom, WTop), 
            transform.position.z);
    }

    private void move() {
        transform.position = Vector3.MoveTowards(
            transform.position, 
            target, 
            speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < range)
            set_target();
    }

    private void attacking() {
        transform.position = Vector3.MoveTowards(transform.position,
            GameObject.Find("Bola" + tag).transform.position,
            speedBola * Time.deltaTime);
    }

    private void visible(bool visibility) {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(visibility);
        }
    }

    private void hide() {
        enabled = false;
    }

    private void show() {
        visible(true);
    }

    public void ExecuteAction(int actionTag) {
        switch (actionTag) {
            case (int)Tags.ActionTags.HIDE:
                hide();
                break;
            case (int)Tags.ActionTags.SHOW:
                show();
                break;
            case (int)Tags.ActionTags.MOVE:
                move();
                break;
            case (int)Tags.ActionTags.ATTACK:
                attacking();
                break;
        }
    }

    public List<int> EventsTrigger()
    {
        FSMevents.Clear();
        FSMevents.AddRange(FSMEnevtsQueue);
        FSMEnevtsQueue.Clear();

        if (Vector3.Distance(transform.position,
            GameObject.Find("Bola" + tag).transform.position) < 5) {
            if (possibility_of_persecution)
                FSMevents.Add((int)Tags.EventTags.ATTACK_EVENT);
        } else {
            FSMevents.Add((int)Tags.EventTags.MOVE_EVENT);
            possibility_of_persecution = true;
        }

        if (FSMevents.Count == 0) {
            FSMevents.Add((int)Tags.EventTags.NULL);
        }

        return FSMevents;
    }

    // Update is called once per frame
    void Update() {
        FSMactions = FSM.Update();
        for (int i = 0; i < FSMactions.Count; i++) {
            if (FSMactions[i] != (int)Tags.ActionTags.NULL)
                ExecuteAction(FSMactions[i]);
        }
    }

    public void OnTriggerEnter2DFunc(Collider2D other)
    {
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
            FSMEnevtsQueue.Add((int)Tags.EventTags.MOVE_EVENT);
        }
    }
}