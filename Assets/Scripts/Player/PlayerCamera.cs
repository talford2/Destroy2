using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private static PlayerCamera current;

    public static PlayerCamera Current
    {
        get { return current; }
    }

    private void Awake()
    {
        current = this;
    }

    private void LateUpdate()
    {

    }
}
