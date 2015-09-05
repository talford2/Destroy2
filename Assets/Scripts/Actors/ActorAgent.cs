using UnityEngine;

public abstract class ActorAgent : MonoBehaviour {
    public Team Team;

    public Vector3 Heading;

    public abstract Vehicle GetVehicle();
}
