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
		INIT,
		MOVING,
		LAYING_BRICK
	}

	//Transition tags
	public enum TransitionTags
	{
		NULL,
		INIT_TO_MOVING,
		MOVING_TO_LAYING_BRICK,
		LAYING_BRICK_TO_MOVING
	}

	//EVENT TAGS
	public enum EventTags
	{
		NULL,
		PLAY,
		LAY_BRICK_E,
		MOVE_E
	}

	//ACTION TAGS
	public enum ActionTags
	{
		NULL,
		RESET_NPC,
		FINISH_INIT,
		START_MOVING,
		MOVE,
		LAY_BRICK,
		FINISH_LAYING_BRICK
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
