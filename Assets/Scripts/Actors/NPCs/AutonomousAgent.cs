using System.Collections.Generic;

public abstract class AutonomousAgent : ActorAgent
{
    private List<ActorAgent> neighbours;

    public abstract void SetTarget(Killable value);

    public abstract bool HasTarget();

    public virtual void AddNeighbour(ActorAgent neighbour)
    {
        if (neighbours==null)
            neighbours = new List<ActorAgent>();
        if (!neighbours.Contains(neighbour))
        neighbours.Add(neighbour);
    }

    public virtual void RemoveNeighbour(ActorAgent neighbour)
    {
        if (neighbours == null)
            neighbours = new List<ActorAgent>();
        neighbours.Remove(neighbour);
    }

    public virtual List<ActorAgent> GetNeighbours()
    {
        return neighbours;
    }
}
