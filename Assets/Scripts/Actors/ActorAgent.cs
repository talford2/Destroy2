using UnityEngine;

public abstract class ActorAgent : MonoBehaviour {

    public Team Team;

    public abstract Vehicle GetVehicle();
}
