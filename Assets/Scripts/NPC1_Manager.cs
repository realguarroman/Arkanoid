using System.Collections;
using UnityEngine;

public enum NPC1_Actions { Start, SetIdle };

namespace Assets.Scripts
{
    public class NPC1_Manager : MonoBehaviour
    {
        public delegate void MessageManagerFunc(MsgContent MC);
        public delegate void OnTrigger2DFunc(Collider2D MC);
        public delegate void AnimFunc();

        MessageManagerFunc currentMM;
        OnTrigger2DFunc currentOTEnter;
        OnTrigger2DFunc currentOTExit;

        AnimFunc currentlbAnimFinished;
        AnimFunc currentexpAnimFinished;
        AnimFunc currentspawnAnimFinished;

        NPC1_FSM FSM;
        NPC1_BT BT;

        private void Awake() {
            //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
            GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        }

        private void ActivateFSM() {
            BT.enabled = false;
            FSM.enabled = true;

            currentMM = FSM.ReceiveMessage;
            currentOTEnter = FSM.OnTriggerEnter2DFunc;
            currentOTExit = FSM.OnTriggerExit2DFunc;

            currentexpAnimFinished = FSM.expAnimFinishedFunc;
            currentlbAnimFinished = FSM.lbAnimFinishedFunc;
            currentspawnAnimFinished = FSM.spawnAnimFinishedFunc;
        }

        private void ActivateBT() {
            BT.enabled = true;
            FSM.enabled = false;

            currentMM = BT.ReceiveMessage;
            currentOTEnter = BT.OnTriggerEnter2DFunc;
            currentOTExit = BT.OnTriggerExit2DFunc;

            currentexpAnimFinished = BT.expAnimFinishedFunc;
            currentlbAnimFinished = BT.lbAnimFinishedFunc;
            currentspawnAnimFinished = BT.spawnAnimFinishedFunc;
        }

        // Use this for initialization
        void Start() {
            FSM = GetComponent<NPC1_FSM>();
            BT = GetComponent<NPC1_BT>();

            ActivateFSM();
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
        public void ReceiveMessage(MsgContent Msg) {
            currentMM(Msg);
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