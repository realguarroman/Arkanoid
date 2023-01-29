using System.Collections.Generic;
using UnityEngine;

public enum NPC1_Actions { Start, SetIdle };

namespace Assets.Scripts
{
    public class NPC1_Manager : MonoBehaviour
    {
        List<List<Color>> colors = new List<List<Color>> {
            new List<Color>{ Color.blue, Color.magenta, Color.yellow },
            new List<Color>{ Color.green, Color.yellow, Color.white },
            new List<Color>{ Color.red, Color.gray, Color.black }
        };

        int current_color_index;

        private void AssignColor() {
            gameObject.transform.Find("Sphere1").GetComponent<Renderer>().material.color
                = colors[current_color_index][0];
            gameObject.transform.Find("Sphere2").GetComponent<Renderer>().material.color
                = colors[current_color_index][1];
            gameObject.transform.Find("Sphere3").GetComponent<Renderer>().material.color
                = colors[current_color_index][2];
        }

        public delegate void OnTrigger2DFunc(Collider2D MC);
        public delegate void VoidFunc();

        VoidFunc currentDisable;
        OnTrigger2DFunc currentOTEnter;
        OnTrigger2DFunc currentOTExit;

        VoidFunc currentlbAnimFinished;
        VoidFunc currentexpAnimFinished;
        VoidFunc currentspawnAnimFinished;

        Behaviour currentComp;

        NPC1_FSM FSM;
        NPC1_BT BT;

        RTDESKEngine Engine;

        private void Awake() {
            //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
            GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        }

        private void ActivateFSM() {
            if (BT.enabled) BT.OnDisableFunc();
            if (!FSM.enabled) FSM.enabled = true;

            currentComp = FSM;
            current_color_index = 0;
            AssignColor();

            currentDisable = FSM.OnDisableFunc;
            currentOTEnter = FSM.OnTriggerEnter2DFunc;
            currentOTExit = FSM.OnTriggerExit2DFunc;

            currentexpAnimFinished = FSM.expAnimFinishedFunc;
            currentlbAnimFinished = FSM.lbAnimFinishedFunc;
            currentspawnAnimFinished = FSM.spawnAnimFinishedFunc;
        }

        private void ActivateBT() {
            if (FSM.enabled) FSM.OnDisableFunc();
            if (!BT.enabled) BT.enabled = true;

            currentComp = BT;
            current_color_index = 1;
            AssignColor();

            currentDisable = BT.OnDisableFunc;
            currentOTEnter = BT.OnTriggerEnter2DFunc;
            currentOTExit = BT.OnTriggerExit2DFunc;

            currentexpAnimFinished = BT.expAnimFinishedFunc;
            currentlbAnimFinished = BT.lbAnimFinishedFunc;
            currentspawnAnimFinished = BT.spawnAnimFinishedFunc;
        }

        // Use this for initialization
        void Start() {
            GameObject engine = GameObject.Find(RTDESKEngine.Name);
            Engine = engine.GetComponent<RTDESKEngine>();

            FSM = GetComponent<NPC1_FSM>();
            BT = GetComponent<NPC1_BT>();

            currentComp = FSM;

            currentDisable = FSM.OnDisableFunc;
            currentOTEnter = FSM.OnTriggerEnter2DFunc;
            currentOTExit = FSM.OnTriggerExit2DFunc;

            currentexpAnimFinished = FSM.expAnimFinishedFunc;
            currentlbAnimFinished = FSM.lbAnimFinishedFunc;
            currentspawnAnimFinished = FSM.spawnAnimFinishedFunc;

            if (BT.enabled) BT.OnDisableFunc();
            current_color_index = 0;
            AssignColor();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                ActivateFSM();
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                ActivateBT();
            }
        }

        public void ReceiveMessage(MsgContent Msg)
        {
            Engine.PushMsg(Msg);
            if (Msg.Type == (int)UserMsgTypes.Action)
            {

                switch (((Action)Msg).action)
                {
                    case (int)NPC1_Actions.Start:
                        currentComp.enabled = true;
                        break;
                    case (int)NPC1_Actions.SetIdle:
                        currentDisable();
                        break;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            currentOTEnter(other);
        }

        private void OnTriggerExit2D(Collider2D other) {
            currentOTExit(other);
        }

        public void lbAnimFinished() {
            currentlbAnimFinished();
        }

        public void expAnimFinished() {
            currentexpAnimFinished();
        }

        public void spawnAnimFinished() {
            currentspawnAnimFinished();
        }
    }
}