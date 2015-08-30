using UnityEngine;

public class SceneManager
{
    private static Transform missileContainer;

    public static Transform MissileContainer 
    {
        get
        {
            if (missileContainer == null)
                missileContainer = new GameObject("Missiles").transform;
            return missileContainer;
        }
    }
}
