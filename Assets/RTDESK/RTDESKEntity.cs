/**
 * RTDeskMsg: Basic Entity class
 *
 * Copyright(C) 2022
 *
 * Prefix: RTDM_

 * @Author: Dr. Ram�n Moll� Vay�
 * @Date:	11/2022
 * @Version: 2.0
 *
 * Update:
 * Date:	
 * Version: 
 * Changes:
 *
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

public class RTDESKEntity : MonoBehaviour
{
    public MessageManager MailBox;
    public RTDESKEngine RTDESKEngineScript;

    void Start()
    {
        RTDESKEngineScript = GameObject.Find(RTDESKEngine.Name).GetComponent<RTDESKEngine>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MessageManager getMailBox(string objectName)
    {
        return GameObject.Find(objectName).GetComponent<RTDESKEntity>().MailBox;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MessageManager getMailBox(GameObject obj)
    {
        return obj.GetComponent<RTDESKEntity>().MailBox;
    }

}