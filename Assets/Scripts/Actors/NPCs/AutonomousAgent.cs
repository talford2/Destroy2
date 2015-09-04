using UnityEngine;

public abstract class AutonomousAgent : MonoBehaviour
{
    public Vector3 Heading;

    public abstract Vehicle GetVehicle();
}
