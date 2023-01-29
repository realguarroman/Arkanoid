using System.Collections;
using UnityEngine;

public enum NPC2_Actions { Start, SetIdle };

namespace Assets.Scripts
{
    public class NPC2_Manager : MonoBehaviour
    {
        public delegate void MessageManagerFunc(MsgContent MC);
        public delegate void OnTrigger2DFunc(Collider2D MC);

        MessageManagerFunc currentMM;
        OnTrigger2DFunc currentOTEnter;

        NPC2_FSM FSM;
        NPC2_BT BT;

        private void Awake() {
            //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
            GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        }

        private void ActivateFSM() {
            BT.enabled = false;
            FSM.enabled = true;

            currentMM = FSM.ReceiveMessage;
            currentOTEnter = FSM.OnTriggerEnter2DFunc;
        }

        private void ActivateBT() {
            BT.enabled = true;
            FSM.enabled = false;

            currentMM = BT.ReceiveMessage;
            currentOTEnter = BT.OnTriggerEnter2DFunc;
        }

        // Use this for initialization
        void Start() {
            FSM = GetComponent<NPC2_FSM>();
            BT = GetComponent<NPC2_BT>();

            ActivateFSM();
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

        public void ReceiveMessage(MsgContent Msg) {
            currentMM(Msg);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            currentOTEnter(other);
        }
    }
}