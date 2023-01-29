using System.Collections;
using UnityEngine;

public enum NPC2_Actions { Start, SetIdle };

namespace Assets.Scripts
{
    public class NPC2_Manager : MonoBehaviour
    {
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

            currentDisable = FSM.OnDisableFunc;
            currentOTEnter = FSM.OnTriggerEnter2DFunc;
        }

        private void ActivateBT() {
            if (FSM.enabled) FSM.OnDisableFunc();
            if (!BT.enabled) BT.enabled = true;

            currentComp = BT;

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
        }

            // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown(KeyCode.X)) {
                ActivateFSM();
            }
            else if (Input.GetKeyDown(KeyCode.B)) {
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
    }
}