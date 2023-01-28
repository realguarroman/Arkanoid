using System;
using System.Collections;
using System.Runtime.CompilerServices;

//Tags that identify data of XML file
public static class Tags
{
	//State tags
	public enum StateTags
	{
		NULL,
		START,
		MOVING,
		LAYING_BRICK,
		DEAD
	}

	//Transition tags
	public enum TransitionTags
	{
		NULL,
		START_TO_MOVING,
		MOVING_TO_LAYING_BRICK,
		MOVING_TO_DEAD,
		LAYING_BRICK_TO_MOVING,
		LAYING_BRICK_TO_DEAD,
		MOVING_TO_START,
		LAYING_BRICK_TO_START
	}

	//EVENT TAGS
	public enum EventTags
	{
		NULL,
		PLAY_EVENT,
		LAY_BRICK_EVENT,
		MOVE_EVENT,
		KILL_EVENT,
		RESPAWN_EVENT,
		IDLE_EVENT
	}

	//ACTION TAGS
	public enum ActionTags
	{
		NULL,
		HIDE,
		SHOW,
		START_MOVING,
		MOVE,
		LAY_BRICK,
		KILL
	}

	// <summary>
	// Get a string that has the name of a given enumeration and returns the type of enumerated value associated
	// </summary>
	// <returns>Generic enumerated value</returns>
	// <remarks> Generic lexical analyzer. Converts a lexeme into a tag with meaning </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TEnum name2Tag<TEnum>(string s)
	where TEnum : struct
	{
		TEnum resultInputType;

		Enum.TryParse(s, true, out resultInputType);
		return resultInputType;
	}
}
