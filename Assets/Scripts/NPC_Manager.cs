using System.Collections.Generic;
using UnityEngine;

public enum NPC_Actions { Start, SetIdle };
public enum EnemyType { Type1, Type2 };

namespace Assets.Scripts
{
    public class NPC_Manager : MonoBehaviour
    {
        public EnemyType Enemy_Type;

        List<List<Color>> colors1 = new List<List<Color>> {
            new List<Color>{ Color.blue, Color.magenta, Color.yellow },
            new List<Color>{ Color.green, Color.yellow, Color.white },
            new List<Color>{ Color.red, Color.gray, Color.black }
        };

        List<List<Color>> colors2 = new List<List<Color>> {
            new List<Color>{ Color.red, Color.cyan, Color.green },
            new List<Color>{ Color.black, Color.green, Color.blue },
            new List<Color>{ Color.yellow, Color.gray, Color.magenta }
        };

        int current_color_index;
        List<List<Color>> colors;

        private void AssignColor()
        {
            gameObject.transform.Find("Sphere1").GetComponent<Renderer>().material.color
                = colors[current_color_index][0];
            gameObject.transform.Find("Sphere2").GetComponent<Renderer>().material.color
                = colors[current_color_index][1];
            gameObject.transform.Find("Sphere3").GetComponent<Renderer>().material.color
                = colors[current_color_index][2];
        }

        public delegate void OnTrigger2DFunc(Collider2D MC);
        public delegate void VoidFunc();

        private void emptyFuncColl(Collider2D other) { }
        private void emptyFunc() { }


        VoidFunc currentDisable;
        OnTrigger2DFunc currentOTEnter;
        OnTrigger2DFunc currentOTExit;

        VoidFunc currentlbAnimFinished;
        VoidFunc currentexpAnimFinished;
        VoidFunc currentspawnAnimFinished;

        Behaviour currentComp;

        NPC1_FSM FSM1;
        NPC1_BT BT1;
        NPC1_Script Script1;

        NPC2_FSM FSM2;
        NPC2_BT BT2;
        NPC2_Script Script2;

        RTDESKEngine Engine;

        private void Awake()
        {
            //Asignar el "listener" al componente normalizado que contienen todos los objetos que pueden recibir mensajes
            GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
        }

        private void ActivateFSM1()
        {
            if (Script1.enabled) Script1.OnDisableFunc();
            if (BT1.enabled) BT1.OnDisableFunc();
            if (!FSM1.enabled) FSM1.enabled = true;

            currentComp = FSM1;
            current_color_index = 0;
            colors = colors1;
            AssignColor();

            currentDisable = FSM1.OnDisableFunc;
            currentOTEnter = FSM1.OnTriggerEnter2DFunc;
            currentOTExit = FSM1.OnTriggerExit2DFunc;

            currentexpAnimFinished = FSM1.expAnimFinishedFunc;
            currentlbAnimFinished = FSM1.lbAnimFinishedFunc;
            currentspawnAnimFinished = FSM1.spawnAnimFinishedFunc;
        }

        private void ActivateFSM2()
        {
            if (Script2.enabled) Script2.OnDisableFunc();
            if (BT2.enabled) BT2.OnDisableFunc();
            if (!FSM2.enabled) FSM2.enabled = true;

            currentComp = FSM2;
            current_color_index = 0;
            colors = colors2;
            AssignColor();

            currentDisable = FSM2.OnDisableFunc;
            currentOTEnter = FSM2.OnTriggerEnter2DFunc;
            currentOTExit = emptyFuncColl;

            currentexpAnimFinished = emptyFunc;
            currentlbAnimFinished = emptyFunc;
            currentspawnAnimFinished = emptyFunc;
        }

        private void ActivateBT1()
        {
            if (Script1.enabled) Script1.OnDisableFunc();
            if (FSM1.enabled) FSM1.OnDisableFunc();
            if (!BT1.enabled) BT1.enabled = true;

            currentComp = BT1;
            current_color_index = 1;
            colors = colors1;
            AssignColor();

            currentDisable = BT1.OnDisableFunc;
            currentOTEnter = BT1.OnTriggerEnter2DFunc;
            currentOTExit = BT1.OnTriggerExit2DFunc;

            currentexpAnimFinished = BT1.expAnimFinishedFunc;
            currentlbAnimFinished = BT1.lbAnimFinishedFunc;
            currentspawnAnimFinished = BT1.spawnAnimFinishedFunc;
        }

        private void ActivateBT2()
        {
            if (Script2.enabled) Script2.OnDisableFunc();
            if (FSM2.enabled) FSM2.OnDisableFunc();
            if (!BT2.enabled) BT2.enabled = true;

            currentComp = BT2;
            current_color_index = 1;
            colors = colors2;
            AssignColor();

            currentDisable = BT2.OnDisableFunc;
            currentOTEnter = BT2.OnTriggerEnter2DFunc;
            currentOTExit = emptyFuncColl;

            currentexpAnimFinished = emptyFunc;
            currentlbAnimFinished = emptyFunc;
            currentspawnAnimFinished = emptyFunc;
        }

        private void ActivateScript1()
        {
            if (FSM1.enabled) FSM1.OnDisableFunc();
            if (BT1.enabled) BT1.OnDisableFunc();
            if (!Script1.enabled) Script1.enabled = true;

            currentComp = Script1;
            current_color_index = 2;
            colors = colors1;
            AssignColor();

            currentDisable = Script1.OnDisableFunc;
            currentOTEnter = Script1.OnTriggerEnter2DFunc;
            currentOTExit = Script1.OnTriggerExit2DFunc;

            currentexpAnimFinished = Script1.expAnimFinishedFunc;
            currentlbAnimFinished = Script1.lbAnimFinishedFunc;
            currentspawnAnimFinished = Script1.spawnAnimFinishedFunc;
        }

        private void ActivateScript2()
        {
            if (FSM2.enabled) FSM2.OnDisableFunc();
            if (BT2.enabled) BT2.OnDisableFunc();
            if (!Script2.enabled) Script2.enabled = true;

            currentComp = Script2;
            current_color_index = 2;
            colors = colors2;
            AssignColor();

            currentDisable = Script2.OnDisableFunc;
            currentOTEnter = Script2.OnTriggerEnter2DFunc;
            currentOTExit = emptyFuncColl;

            currentexpAnimFinished = emptyFunc;
            currentlbAnimFinished = emptyFunc;
            currentspawnAnimFinished = emptyFunc;
        }

        private void InitialFSM() {
            if (Enemy_Type == EnemyType.Type1) {
                FSM1 = GetComponent<NPC1_FSM>();
                BT1 = GetComponent<NPC1_BT>();
                Script1 = GetComponent<NPC1_Script>();

                currentComp = FSM1;

                currentDisable = FSM1.OnDisableFunc;
                currentOTEnter = FSM1.OnTriggerEnter2DFunc;
                currentOTExit = FSM1.OnTriggerExit2DFunc;

                currentexpAnimFinished = FSM1.expAnimFinishedFunc;
                currentlbAnimFinished = FSM1.lbAnimFinishedFunc;
                currentspawnAnimFinished = FSM1.spawnAnimFinishedFunc;

                if (Script1.enabled) Script1.OnDisableFunc();
                if (BT1.enabled) BT1.OnDisableFunc();
                colors = colors1;
            }
            else {
                FSM2 = GetComponent<NPC2_FSM>();
                BT2 = GetComponent<NPC2_BT>();
                Script2 = GetComponent<NPC2_Script>();

                currentComp = FSM2;

                currentDisable = FSM2.OnDisableFunc;
                currentOTEnter = FSM2.OnTriggerEnter2DFunc;
                currentOTExit = emptyFuncColl;

                currentexpAnimFinished = emptyFunc;
                currentlbAnimFinished = emptyFunc;
                currentspawnAnimFinished = emptyFunc;

                if (Script2.enabled) Script2.OnDisableFunc();
                if (BT2.enabled) BT2.OnDisableFunc();
                colors = colors2;
            }

            current_color_index = 0;
            AssignColor();
        }

        // Use this for initialization
        void Start()
        {
            GameObject engine = GameObject.Find(RTDESKEngine.Name);
            Engine = engine.GetComponent<RTDESKEngine>();

            InitialFSM();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.X)) {
                if (Enemy_Type == EnemyType.Type1) {
                    if (!FSM1.enabled) ActivateFSM1();
                }
                else if (Enemy_Type == EnemyType.Type2) {
                    if (!FSM2.enabled) ActivateFSM2();
                }
            }

            else if (Input.GetKeyDown(KeyCode.B)) {
                if (Enemy_Type == EnemyType.Type1) {
                    if (!BT1.enabled) ActivateBT1();
                }
                else if (Enemy_Type == EnemyType.Type2) {
                    if (!BT2.enabled) ActivateBT2();
                }
            }

            else if (Input.GetKeyDown(KeyCode.S)) {
                if (Enemy_Type == EnemyType.Type1) {
                    if (!Script1.enabled) ActivateScript1();
                }
                else if (Enemy_Type == EnemyType.Type2) {
                    if (!Script2.enabled) ActivateScript2();
                }
            }

            //else if (Input.GetKeyDown(KeyCode.C)) {
            //    if (Enemy_Type == EnemyType.Type1) {
            //        Enemy_Type = EnemyType.Type2;
            //    }
            //    else {
            //        Enemy_Type = EnemyType.Type1;
            //    }
            //    InitialFSM();
            //}
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