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
}
