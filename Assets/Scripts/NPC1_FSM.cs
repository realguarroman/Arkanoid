﻿#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GAIA;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public enum NPC1_FSM_Actions { Start, SetIdle };

public class NPC1_FSM : MonoBehaviour
{
    // GameObject attributes
    private float range;
    private float speed;
    private int ladrillos_counter;
    private int colliding_with_bricks;

    private Vector3 target;

    private float WRight;
    private float WLeft;
    private float WTop;
    private float WBottom;

    private Stopwatch stopwatch;
    private Stopwatch stopwatch2;

    public GameObject brick_prefab;
    private GameObject ladrillos_set;
    private Animation animation_comp;

    // IA attributes
    private FSM_Machine FSM;                                      // Variable que contiene la FSM.
    private GAIA_Manager manager;                                 // Variable que contiene la referencia al manager.
    private List<int> FSMactions;                                 // Variable que contiene las acciones a realizar en cada update.
    private List<int> FSMevents = new List<int>();                // Variable que contiene los eventos.
    private List<int> FSMEnevtsQueue = new List<int>();

    RTDESKEngine Engine;

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
        colliding_with_bricks = 0;

        var temp = GameObject.Find("Field" + tag);
        WRight = temp.transform.position.x + (temp.transform.localScale.x / 2) - 2f;
        WLeft = temp.transform.position.x - (temp.transform.localScale.x / 2) + 2f;
        WTop = temp.transform.position.y + (temp.transform.localScale.y / 2) - 4f;
        WBottom = temp.transform.position.y - (temp.transform.localScale.y / 5);

        ladrillos_set = GameObject.Find("Ladrillos" + tag);

        stopwatch = new Stopwatch();
        stopwatch2 = new Stopwatch();

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        animation_comp = GetComponent<Animation>();
    }

    public void ReceiveMessage(MsgContent Msg) {
        Engine.PushMsg(Msg);
        if (Msg.Type == (int)UserMsgTypes.Action) {

            switch (((Action)Msg).action) {
                case (int)NPC1_FSM_Actions.Start:
                    FSMEnevtsQueue.Add((int)Tags.EventTags.PLAY_EVENT);
                    break;
                case (int)NPC1_FSM_Actions.SetIdle:
                    Debug.Log("IDLE");
                    FSMEnevtsQueue.Add((int)Tags.EventTags.IDLE_EVENT);
                    break;
            }            
        }
    }
    void set_target() {
        target = new Vector3(
            Random.Range(WLeft, WRight), 
            Random.Range(WBottom, WTop), 
            transform.position.z);
    }

    private void start_moving() {
        animation_comp.Stop();
        animation_comp.Play("EnemyRotation");
        stopwatch.Start();
        set_target();
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
        animation_comp.Stop();
        animation_comp.Play("LayingBrick");
    }

    public void lbAnimFinished() {
        var pos = new Vector3(
            transform.position.x,
            transform.position.y,
            -1.5f);
        var rot = new Quaternion();

        GameObject new_brick = Instantiate(brick_prefab, pos, rot);

        new_brick.name = $"Ladrillo{tag}_{ladrillos_counter++}";
        new_brick.tag = tag;
        new_brick.transform.parent = ladrillos_set.transform;

        FSMEnevtsQueue.Add((int)Tags.EventTags.MOVE_EVENT);
    }

    private void visible(bool visibility) {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(visibility);
        }
    }

    private void kill() {
        animation_comp.Stop();
        animation_comp.Play("Exploding");
    }

    public void expAnimFinished() {
        visible(false);
        stopwatch2.Start();
    }

    private void respaw() {
        visible(true);
        animation_comp.Stop();
        animation_comp.Play("Spawning");
    }

    public void spawnAnimFinished() {
        FSMEnevtsQueue.Add((int)Tags.EventTags.RESPAWN_EVENT);
    }

    private void hide() {
        visible(false);
    }

    private void show() {
        visible(true);
    }

    public void ExecuteAction(int actionTag)
    {
        switch (actionTag) {
            case (int)Tags.ActionTags.HIDE:
                hide();
                break;
            case (int)Tags.ActionTags.SHOW:
                show();
                break;
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
                stopwatch.Reset();
                FSMevents.Add((int)Tags.EventTags.LAY_BRICK_EVENT);
            }
        }
        if (stopwatch2.ElapsedMilliseconds > 5000) {
            stopwatch2.Reset();
            respaw();
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
        if (other.name.StartsWith("Ladrillo")) {
            colliding_with_bricks++;
        }
        else if (
            other.name.StartsWith("Bola") ||
            other.name.StartsWith("Raqueta")) {
            FSMEnevtsQueue.Add((int)Tags.EventTags.KILL_EVENT);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.name.StartsWith("Ladrillo")) {
            colliding_with_bricks--;
        }
    }
}