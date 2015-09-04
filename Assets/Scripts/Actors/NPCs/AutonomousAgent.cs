using System.Collections.Generic;
using UnityEngine;

public abstract class AutonomousAgent : MonoBehaviour
{
    public Vector3 Heading;
    private List<AutonomousAgent> neighbours;

    public abstract Vehicle GetVehicle();

    public virtual void AddNeighbour(AutonomousAgent neighbour)
    {
        if (neighbours==null)
            neighbours = new List<AutonomousAgent>();
        if (!neighbours.Contains(neighbour))
        neighbours.Add(neighbour);
    }

    public virtual void RemoveNeighbour(AutonomousAgent neighbour)
    {
        if (neighbours == null)
            neighbours = new List<AutonomousAgent>();
        neighbours.Remove(neighbour);
    }

    public virtual List<AutonomousAgent> GetNeighbours()
    {
        return neighbours;
    }
}
