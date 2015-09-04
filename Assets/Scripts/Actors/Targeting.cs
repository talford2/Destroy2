using System.Collections.Generic;
using UnityEngine;

public class Targeting {
    private static Dictionary<Team, List<Killable>> targetables;

    public static void AddTargetable(Team team, Killable killable)
    {
        if (targetables == null)
            targetables = new Dictionary<Team, List<Killable>>();

        if (!targetables.ContainsKey(team))
            targetables.Add(team, new List<Killable>());
        targetables[team].Add(killable);

        //Debug.Log("+ TEAM: " + team +" COUNT: " + targetables[team].Count);
    }

    public static void RemoveTargetable(Team team, Killable killable)
    {
        if (targetables.ContainsKey(team))
            targetables[team].Remove(killable);

        //Debug.Log("- TEAM: " + team + " COUNT: " + targetables[team].Count);
    }
}
