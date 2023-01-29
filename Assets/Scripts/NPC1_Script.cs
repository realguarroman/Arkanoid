using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GAIA;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts
{
    public class NPC1_Script : MonoBehaviour
    {
        enum StateNPC1 { InStart, Start, OutStart, InMove, Move, InLayBrick, LayBrick, inDead, Dead };

        private StateNPC1 current_state = StateNPC1.InStart;

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

        // Use this for initialization
        void Start()
        {
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

            animation_comp = GetComponent<Animation>();
            StatesMachine();
        }

        void set_target()
        {
            target = new Vector3(
            Random.Range(WLeft, WRight),
            Random.Range(WBottom, WTop),
            transform.position.z);
        }

        public void visible(bool visibility)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(visibility);
            }
        }

        public void OnEnable() {
            if (current_state == StateNPC1.Dead)
                stopwatch.Start();
            else if (current_state == StateNPC1.Start)
                current_state = StateNPC1.OutStart;
        }

        public void OnDisableFunc() {
            visible(false);
            if (current_state == StateNPC1.Dead)
                stopwatch.Stop();
            else
                current_state = StateNPC1.InStart;
        }

        void StatesMachine() {
            switch (current_state) {
                case StateNPC1.InStart:
                    visible(false);
                    current_state = StateNPC1.Start;
                    enabled = false;
                    break;
                case StateNPC1.OutStart:
                    visible(true);
                    animation_comp.Stop();
                    animation_comp.Play("EnemyRotation");
                    current_state = StateNPC1.InMove;
                    break;
                case StateNPC1.InMove:
                    stopwatch.Start();
                    set_target();
                    current_state = StateNPC1.Move;
                    break;
                case StateNPC1.Move:
                    transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, target) < range) set_target();
                    break;
                case StateNPC1.InLayBrick:
                    Debug.Log("LayBrick");
                    animation_comp.Stop();
                    animation_comp.Play("LayingBrick");
                    current_state = StateNPC1.LayBrick;
                    break;
                case StateNPC1.inDead:
                    animation_comp.Stop();
                    animation_comp.Play("Exploding");
                    current_state = StateNPC1.Dead;
                    break;
            }
        }

        public void lbAnimFinishedFunc()
        {
            var pos = new Vector3(
                transform.position.x,
                transform.position.y,
                -1.5f);
            var rot = new Quaternion();

            GameObject new_brick = Instantiate(brick_prefab, pos, rot);

            new_brick.name = $"Ladrillo{tag}_{ladrillos_counter++}";
            new_brick.tag = tag;
            new_brick.transform.parent = ladrillos_set.transform;
            current_state = StateNPC1.InMove;
        }

        public void expAnimFinishedFunc() {
            visible(false);
            stopwatch2.Start();
        }

        // Update is called once per frame
        void Update() {
            StatesMachine();

            if (stopwatch.ElapsedMilliseconds > 3000)
            {
                if (colliding_with_bricks > 0) stopwatch.Restart();
                else
                {
                    stopwatch.Reset();
                    current_state = StateNPC1.InLayBrick;
                }
            }
            if (stopwatch2.ElapsedMilliseconds > 5000)
            {
                stopwatch2.Reset();
                visible(true);
                animation_comp.Stop();
                animation_comp.Play("Spawning");
            }
        }

        public void spawnAnimFinishedFunc() {
            current_state = StateNPC1.InMove;
        }

        public void OnTriggerEnter2DFunc(Collider2D other)
        {
            if (other.name.StartsWith("Ladrillo"))
            {
                colliding_with_bricks++;
            }
            else if (
                other.name.StartsWith("Bola") ||
                other.name.StartsWith("Raqueta"))
            {
                current_state = StateNPC1.inDead;
            }
        }

        public void OnTriggerExit2DFunc(Collider2D other)
        {
            if (other.name.StartsWith("Ladrillo"))
            {
                colliding_with_bricks--;
            }
        }
    }
}