using System.Collections.Generic;
using System.Linq;
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

    public static Killable FindNearest(Team team, Vector3 fromPosition, float maxDistance)
    {
        Killable target = null;
        if (targetables.ContainsKey(team))
        {
            var targetCandidates = targetables[team];
            if (targetCandidates.Any())
            {
                var smallestDistanceSquared = maxDistance * maxDistance;
                foreach (var candidate in targetCandidates)
                {
                    var toTarget = candidate.transform.position - fromPosition;
                    if (toTarget.sqrMagnitude < smallestDistanceSquared)
                    {
                        smallestDistanceSquared = toTarget.sqrMagnitude;
                        target = candidate;
                    }
                }
            }
        }
        return target;
    }

    public static Team GetOpposingTeam(Team team)
    {
        return team == Team.Good
            ? Team.Bad
            : Team.Good;
    }
}
