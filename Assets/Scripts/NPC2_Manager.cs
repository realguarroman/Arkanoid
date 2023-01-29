using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPC2_Actions { Start, SetIdle };

namespace Assets.Scripts
{
    public class NPC2_Manager : MonoBehaviour
    {
        List<List<Color>> colors = new List<List<Color>> {
            new List<Color>{ Color.red, Color.cyan, Color.green },
            new List<Color>{ Color.black, Color.green, Color.blue },
            new List<Color>{ Color.yellow, Color.gray, Color.magenta }
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

        Behaviour currentComp;

        NPC2_FSM FSM;
        NPC2_BT BT;

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
        }

        private void ActivateBT() {
            if (FSM.enabled) FSM.OnDisableFunc();
            if (!BT.enabled) BT.enabled = true;

            currentComp = BT;
            current_color_index = 1;
            AssignColor();

            currentDisable = BT.OnDisableFunc;
            currentOTEnter = BT.OnTriggerEnter2DFunc;
        }

        // Use this for initialization
        void Start() {
            GameObject engine = GameObject.Find(RTDESKEngine.Name);
            Engine = engine.GetComponent<RTDESKEngine>();

            FSM = GetComponent<NPC2_FSM>();
            BT = GetComponent<NPC2_BT>();

            currentComp = FSM;

            currentDisable = FSM.OnDisableFunc;
            currentOTEnter = FSM.OnTriggerEnter2DFunc;

            if (BT.enabled) BT.OnDisableFunc();
            current_color_index = 0;
            AssignColor();
        }

            // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown(KeyCode.X) && !FSM.enabled)
                ActivateFSM();
            else if (Input.GetKeyDown(KeyCode.B) && !BT.enabled)
                ActivateBT();
        }

        public void ReceiveMessage(MsgContent Msg) {
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
    }
}