using UnityEngine;
using System.Collections;

public class GameState
{
	private static GameState current { get; set; }

	public static GameState Current
	{
		get
		{
			if (current == null)
			{
				current = new GameState();
			}
			return current;
		}
	}
}
