using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class SafeSensor : MonoBehaviour
{
    private Team opposingTeam;

    private List<ActorAgent> enemyAgents;
    private bool isSafe;
    
    public void SetTeam(Team team)
    {
        opposingTeam = Targeting.GetOpposingTeam(team);
        enemyAgents = new List<ActorAgent>();
    }

    public bool GetIsSafe()
    {
        return isSafe;
    }

    public int GetEnemyAgentCount()
    {
        if (enemyAgents == null)
            return 0;
        return enemyAgents.Count;
    }

    private void OnTriggerEnter(Collider other)
    {
        var agent = other.GetComponentInParent<ActorAgent>();
        if (agent != null)
        {
            if (agent.Team == opposingTeam)
            {
                if (!enemyAgents.Contains(agent))
                    enemyAgents.Add(agent);
                isSafe = false;
                //Debug.Log("AGENTS: " + enemyAgents.Count);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var agent = other.GetComponentInParent<ActorAgent>();
        if (agent != null)
        {
            if (agent.Team == opposingTeam)
            {
                enemyAgents.Remove(agent);
                //Debug.Log("AGENTS: " + enemyAgents.Count);
                if (enemyAgents.Count == 0)
                    isSafe = true;
            }
        }
    }
}
