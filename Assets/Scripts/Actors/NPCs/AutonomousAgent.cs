using System.Collections.Generic;

public abstract class AutonomousAgent : ActorAgent
{
    private List<ActorAgent> neighbours;
    private List<Cover> detectedCover;

    public abstract void SetTarget(Killable value);

    public abstract bool HasTarget();

    public virtual void AddNeighbour(ActorAgent neighbour)
    {
        if (neighbours == null)
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
        if (neighbours == null)
            neighbours = new List<ActorAgent>();
        return neighbours;
    }

    public virtual void AddDetectedCover(Cover cover)
    {
        if (detectedCover == null)
            detectedCover = new List<Cover>();
        if (!detectedCover.Contains(cover))
            detectedCover.Add(cover);
    }

    public virtual void RemoveDetectedCover(Cover cover)
    {
        if (detectedCover == null)
            detectedCover = new List<Cover>();
        detectedCover.Remove(cover);
    }

    public virtual List<Cover> GetDetectedCover()
    {
        if (detectedCover == null)
            detectedCover = new List<Cover>();
        return detectedCover;
    }
}
