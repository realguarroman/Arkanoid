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
using Debug = UnityEngine.Debug;

namespace Assets.Scripts
{
    public class NPC2_Script : MonoBehaviour
    {
        enum StateNPC2 { InStart, Start, OutStart, InMove, Move, Attack };

        private StateNPC2 current_state = StateNPC2.InStart;

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

        RTDESKEngine Engine;

        // Use this for initialization
        void Start()
        {
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
            StatesMachine();
            set_target();
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

        public void OnEnable()
        {
            if (current_state == StateNPC2.Start)
                current_state = StateNPC2.OutStart;
        }

        public void OnDisableFunc()
        {
            current_state = StateNPC2.InStart;
        }

        void StatesMachine()
        {
            switch (current_state)
            {
                case StateNPC2.InStart:
                    visible(false);
                    current_state = StateNPC2.Start;
                    enabled = false;
                    break;
                case StateNPC2.OutStart:
                    visible(true);
                    current_state = StateNPC2.Move;
                    break;
                case StateNPC2.Move:
                    transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, target) < range) set_target();
                    break;
                case StateNPC2.Attack:
                    transform.position = Vector3.MoveTowards(transform.position,
                    GameObject.Find("Bola" + tag).transform.position,
                    speedBola * Time.deltaTime);
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            StatesMachine();

            if (Vector3.Distance(transform.position,
            GameObject.Find("Bola" + tag).transform.position) < 5) {
                if (possibility_of_persecution) 
                    current_state = StateNPC2.Attack;
            }
            else {
                if (current_state == StateNPC2.Attack)
                    current_state = StateNPC2.Move;
                possibility_of_persecution = true;
            }
        }

        public void OnTriggerEnter2DFunc(Collider2D other)
        {
            if (other.name.StartsWith("Bola"))
            {
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
                current_state = StateNPC2.Move;
            }
        }
    }
}