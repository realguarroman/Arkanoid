/**
 * RTDESKInputManager: The prefab in charge of managing the input from internet or preipherals and inject events into the RTDESK system
 *
 * Copyright(C) 2022
 *
 * Prefix: RTDESKIM_

 * @Author:	Dr. Ramón Mollá Vayá
 * @Date:	12/2022
 * @Version: 2.0
 *
 * Update:
 * Date:	
 * Version: 
 * Changes:
 *
 */

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//The component that creates an internal RTDESK Engine
public class RTDESKInputManager : MonoBehaviour
{
	SortedList<KeyCode, List<MessageManager>> ActiveInputs = new SortedList<KeyCode, List<MessageManager>>();

	RTDESKEngine Engine;

	/**
		* @fn StartUp()
		* Insert comment
	*/
	void Awake()
	{
		Reset();
	}

    void Start()
    {
		Engine = GetComponent<RTDESKEngine>();
		Debug.Log("Engine name " + Engine.name);
	}

	// Update is called once per frame
	void Update()
	{
		for (int i = 0; i < ActiveInputs.Count; i++)
			if (Input.GetKeyDown(ActiveInputs.Keys[i]))
				foreach (MessageManager RMM in ActiveInputs.Values[i])
				{
					RTDESKInputMsg IM	= (RTDESKInputMsg)Engine.PopMsg((int)RTDESKMsgTypes.Input);
					IM.c				= ActiveInputs.Keys[i];
					IM.Sender			= gameObject;
					IM.Receiver			= RMM;
					IM.AbsoluteTime		= HRTimer.HRT_INMEDIATELY;
					Engine.SendMsg (IM,	  HRTimer.HRT_INMEDIATELY);
				}
	}

	/**
		* @fn RegisterKeyCode()
		* Introduces a new listener into the list of receiving methods sensitive to the given code
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void RegisterKeyCode (MessageManager Rmethod, KeyCode Kc)
    {
		if (ActiveInputs.ContainsKey(Kc))
			ActiveInputs.Values[ActiveInputs.IndexOfKey(Kc)].Add(Rmethod);
		else
        {
			List<MessageManager> LRM = new List<MessageManager>();
			LRM.Add(Rmethod);
			ActiveInputs.Add(Kc, new List<MessageManager>());
		}
		Debug.Log("Registrando otro código");
	}

	/**
		* @fn RegisterKeyCode()
		* Introduces a new listener into the list of receiving methods sensitive to the given code
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void UnRegisterKeyCode(MessageManager Rmethod, KeyCode Kc)
	{
		if (ActiveInputs.ContainsKey(Kc))
			ActiveInputs.Values[ActiveInputs.IndexOfKey(Kc)].Remove(Rmethod);
	}

	/**
		* @fn Reset()
		* Insert comment
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void Reset()
	{
		ActiveInputs.Clear();
	}

}

