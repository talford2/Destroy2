using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SteeringBehaviour
{
    private readonly AutonomousAgent agent;

    public SteeringBehaviour(AutonomousAgent value)
    {
        agent = value;
    }

    public Vector3 SeekForce(Vector3 position)
    {
        return (position - agent.GetVehicle().transform.position).normalized;
    }

    public Vector3 FleeForce(Vector3 position)
    {
        return (agent.GetVehicle().transform.position - position).normalized;
    }

    public GroupForces CalculateGroupForces(List<AutonomousAgent> neighbours)
    {
        if (neighbours != null && neighbours.Any())
        {
            var avoidSum = Vector3.zero;
            var cohesiveSum = Vector3.zero;
            var headingSum = Vector3.zero;
            foreach (var neighbour in neighbours)
            {
                if (neighbour != null && neighbour != agent)
                {
                    var fromNeighbour = agent.transform.position - neighbour.transform.position;
                    //if (fromNeighbour.sqrMagnitude < avoidDistance * avoidDistance)
                    avoidSum += fromNeighbour.normalized / fromNeighbour.magnitude;

                    cohesiveSum += neighbour.transform.position;

                    headingSum += neighbour.Heading;
                }
            }
            return new GroupForces
            {
                SeparationForce = avoidSum,
                CohesiveForce = SeekForce(cohesiveSum / neighbours.Count()),
                AlignmentForce = (headingSum / neighbours.Count()).normalized
            };
        }
        return new GroupForces();
    }

    public class GroupForces
    {
        public Vector3 SeparationForce { get; set; }
        public Vector3 CohesiveForce { get; set; }
        public Vector3 AlignmentForce { get; set; }
    }
}
