using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GAIA;
using System.Diagnostics;

namespace Assets.Scripts
{
    public class NPC1_FSM : MonoBehaviour
    {
        // GameObject attributes
        float range;
        float speed;
        int ladrillos_counter = 36;

        Vector3 target;

        float WRight;
        float WLeft;
        float WTop;
        float WBottom;

        bool finished_laying_brick;
        bool start;
        bool colliding_with_brick;
        Stopwatch stopwatch;

        public GameObject brick_prefab;
        public GameObject ladrillos_set;

        // IA attributes
        private FSM_Machine FSM;                                      // Variable que contiene la FSM.
        private GAIA_Manager manager;                                 // Variable que contiene la referencia al manager.
        private List<int> FSMactions;                                 // Variable que contiene las acciones a realizar en cada update.
        private List<int> FSMevents = new List<int>();                // Variable que contiene los eventos.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void addNoEvent() { FSMevents.Add((int)Tags.EventTags.NULL); }

        // Start is called before the first frame update
        void Start()
        {
            manager = GAIA_Controller.INSTANCE.m_manager;
            FSM = manager.createMachine(this, (int)FA_Classic.FAType.CLASSIC, "NPC1_FSMDeterministic");

            addNoEvent();

            range = 0.1f;
            speed = 2;

            var temp = GameObject.Find("Field" + tag);
            WRight = temp.transform.position.x + (temp.transform.localScale.x / 2);
            WLeft = temp.transform.position.x - (temp.transform.localScale.x / 2);
            WTop = temp.transform.position.y + (temp.transform.localScale.y / 2);
            WBottom = temp.transform.position.y - (temp.transform.localScale.y / 2);

            finished_laying_brick = false;
            start = true;
            stopwatch = new Stopwatch();

            ladrillos_set = GameObject.Find("Ladrillos" + tag);
        }

        private void reset_npc() {
            start = true;
        }

        private void finish_init() {
            start = false;
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

            finished_laying_brick = true;
        }

        public void ExecuteAction(int actionTag)
        {
            switch (actionTag)
            {
                case (int)Tags.ActionTags.RESET_NPC:
                    reset_npc();
                    break;

                case (int)Tags.ActionTags.FINISH_INIT:
                    finish_init();
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
            }
        }

        public List<int> EventsTrigger()
        {
            FSMevents.Clear();

            if (stopwatch.ElapsedMilliseconds > 5000) {
                FSMevents.Add((int)Tags.EventTags.LAY_BRICK_E);
                stopwatch.Reset();
            }
            if (start) {
                FSMevents.Add((int)Tags.EventTags.PLAY);
            }
            if (finished_laying_brick) {
                FSMevents.Add((int)Tags.EventTags.MOVE_E);
                finished_laying_brick = false;
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
        }
    }
}