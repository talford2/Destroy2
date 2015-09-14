using System.Linq;
using UnityEngine;

public class SceneManager
{
    private static Transform missileContainer;
    private static Transform corpseContainer;

    public static Transform MissileContainer
    {
        get
        {
            if (missileContainer == null)
                missileContainer = new GameObject("Missiles").transform;
            return missileContainer;
        }
    }

    public static Transform CorpseContainer
    {
        get
        {
            if (corpseContainer == null)
                corpseContainer = new GameObject("Corpses").transform;
            return corpseContainer;
        }
    }

    public static PlayerSpawner FindNearestSpawner(Vector3 position)
    {
        PlayerSpawner closestSpawner = null;
        var closestDistanceSquared = Mathf.Infinity;
        var spawners = GameObject.FindObjectsOfType<PlayerSpawner>().Where(s => s.IsSafe);
        foreach (var spawner in spawners)
        {
            var distSquared = (spawner.transform.position - position).sqrMagnitude;
            if (distSquared < closestDistanceSquared)
            {
                closestSpawner = spawner;
                closestDistanceSquared = distSquared;
            }
        }
        return closestSpawner;
    }
}
