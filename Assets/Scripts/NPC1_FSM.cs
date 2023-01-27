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
using UnityEngine.AI;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GAIA;
using System.Diagnostics;

public enum NPC1_FSM_Actions { Start };

public class NPC1_FSM : MonoBehaviour
{
    // GameObject attributes
    private float range;
    private float speed;
    private int ladrillos_counter;

    private Vector3 target;

    private float WRight;
    private float WLeft;
    private float WTop;
    private float WBottom;

    private bool died;
    private int colliding_with_bricks;

    private Stopwatch stopwatch;

    public GameObject brick_prefab;
    private GameObject ladrillos_set;

    // IA attributes
    private FSM_Machine FSM;                                      // Variable que contiene la FSM.
    private GAIA_Manager manager;                                 // Variable que contiene la referencia al manager.
    private List<int> FSMactions;                                 // Variable que contiene las acciones a realizar en cada update.
    private List<int> FSMevents = new List<int>();                // Variable que contiene los eventos.
    private List<int> FSMEnevtsQueue = new List<int>();

    RTDESKEngine Engine;
    HRT_Time fiveMillis;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void addNoEvent() { FSMevents.Add((int)Tags.EventTags.NULL); }

    void Awake() {
        //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    // Start is called before the first frame update
    void Start()
    {
        manager = GAIA_Controller.INSTANCE.m_manager;
        FSM = manager.createMachine(this, (int)FA_Classic.FAType.CLASSIC, "NPC1_FSMDeterministic");
        addNoEvent();

        range = 0.1f;
        speed = 2;
        ladrillos_counter = 36;

        var temp = GameObject.Find("Field" + tag);
        WRight = temp.transform.position.x + (temp.transform.localScale.x / 2);
        WLeft = temp.transform.position.x - (temp.transform.localScale.x / 2);
        WTop = temp.transform.position.y + (temp.transform.localScale.y / 2);
        WBottom = temp.transform.position.y - (temp.transform.localScale.y / 2);

        died = false;
        colliding_with_bricks = 0;
        ladrillos_set = GameObject.Find("Ladrillos" + tag);

        stopwatch = new Stopwatch();

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();
    }

    public void ReceiveMessage(MsgContent Msg) {
        Engine.PushMsg(Msg);
        if (Msg.Type == (int)UserMsgTypes.Action
            && ((Action)Msg).action == (int)NPC1_FSM_Actions.Start) {
            FSMEnevtsQueue.Add((int)Tags.EventTags.PLAY);
        }
    }

    private void start_moving() {
        stopwatch.Start();
        set_target();
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

    private void lay_brick() {
        var pos = new Vector3(
            transform.position.x, 
            transform.position.y, 
            -1.5f);
        var rot = new Quaternion();

        GameObject new_brick = Instantiate(brick_prefab, pos, rot);

        new_brick.name = $"Ladrillo{tag}_{ladrillos_counter++}";
        new_brick.tag = tag;
        new_brick.transform.parent = ladrillos_set.transform;

        FSMEnevtsQueue.Add((int)Tags.EventTags.MOVE_E);
    }

    private void kill() {
        gameObject.SetActive(false);
    }

    public void ExecuteAction(int actionTag)
    {
        switch (actionTag)
        {
            case (int)Tags.ActionTags.START_MOVING:
                start_moving();
                break;

            case (int)Tags.ActionTags.MOVE:
                move();
                break;

            case (int)Tags.ActionTags.LAY_BRICK:
                lay_brick();
                break;
            case (int)Tags.ActionTags.KILL:
                kill();
                break;
        }
    }

    public List<int> EventsTrigger()
    {
        FSMevents.Clear();
        FSMevents.AddRange(FSMEnevtsQueue);
        FSMEnevtsQueue.Clear();

        if (stopwatch.ElapsedMilliseconds > 5000) {
            if (colliding_with_bricks > 0) stopwatch.Restart();
            else {
                FSMevents.Add((int)Tags.EventTags.LAY_BRICK_E);
                stopwatch.Reset();
            }
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

    private void OnTriggerEnter2D(Collider2D other) {
        //UnityEngine.Debug.Log("OnCollisionEnter2D");

        if (other.name.StartsWith("Ladrillo"))
        {
            colliding_with_bricks++;
        }
        else if (
            other.name.StartsWith("Bola") ||
            other.name.StartsWith("Raqueta")) {
            FSMEnevtsQueue.Add((int)Tags.EventTags.KILL_E);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.name.StartsWith("Ladrillo")) {
            colliding_with_bricks--;
        }
    }
}