using System.Collections;
using UnityEngine;

public enum NPC2_Actions { Start, SetIdle };

namespace Assets.Scripts
{
    public class NPC2_Manager : MonoBehaviour
    {
        public delegate void MessageManagerFunc(MsgContent MC);
        public delegate void OnTriggerEnter2DFunc(Collider2D MC);

        MessageManagerFunc currentMM;
        OnTriggerEnter2DFunc currentOT;

        NPC2_FSM FSM;
        NPC2_BT BT;

        private void Awake() {
            //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
            GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        }

        // Use this for initialization
        void Start() {
            FSM = GetComponent<NPC2_FSM>();
            BT = GetComponent<NPC2_BT>();

            currentMM = FSM.ReceiveMessage;
            currentOT = FSM.OnTriggerEnter2DFunc;
        }
        public void ReceiveMessage(MsgContent Msg) {
            currentMM(Msg);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            currentOT(other);
        }

            // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown(KeyCode.X)) {
                BT.enabled = false;
                FSM.enabled = true;
                currentMM = FSM.ReceiveMessage;
                currentOT = FSM.OnTriggerEnter2DFunc;
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                FSM.enabled = false;
                BT.enabled = true;

                currentMM = BT.ReceiveMessage;
                currentOT = BT.OnTriggerEnter2DFunc;
            }
        }
    }
}