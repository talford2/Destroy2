using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(SphereCollider))]
public class SafeSensor : MonoBehaviour
{
    public float CheckFrequency = 1f;

    private SphereCollider triggerCollider;
    private List<Collider> lastColliders;

    private Team opposingTeam;
    private List<ActorAgent> enemyAgents;
    private bool isSafe;
    
    public void SetTeam(Team team)
    {
        opposingTeam = Targeting.GetOpposingTeam(team);
        enemyAgents = new List<ActorAgent>();
        triggerCollider = GetComponent<SphereCollider>();
        lastColliders = new List<Collider>();

        StartCoroutine(CheckInside(Random.Range(0.5f, 1f)));
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

    private IEnumerator CheckInside(float delay)
    {
        yield return new WaitForSeconds(delay);
        var recentColliders = Physics.OverlapSphere(transform.position, triggerCollider.radius, LayerMask.GetMask("Detectable")).ToList();

        var enteredColliders = recentColliders.Except(lastColliders).ToList();
        var exitedColliders = lastColliders.Except(recentColliders).ToList();

        foreach (var enteredCollider in enteredColliders)
        {
            var detectable = enteredCollider.GetComponent<Detectable>();
            var agent = detectable.GetOwner();

            if (agent != null)
            {
                if (agent.Team == opposingTeam)
                {
                    if (!enemyAgents.Contains(agent))
                        enemyAgents.Add(agent);
                    isSafe = false;
                }
            }
        }

        foreach (var exitedCollider in exitedColliders)
        {
            if (exitedCollider != null)
            {
                var detectable = exitedCollider.GetComponent<Detectable>();
                var agent = detectable.GetOwner();
                if (agent != null)
                {
                    if (agent.Team == opposingTeam)
                    {
                        enemyAgents.Remove(agent);
                        if (enemyAgents.Count == 0)
                            isSafe = true;
                    }
                }
            }
        }

        //Debug.Log("ENEMIES: " + enemyAgents.Count);

        lastColliders = recentColliders;
        StartCoroutine(CheckInside(CheckFrequency));
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        var detectable = other.GetComponent<Detectable>();
        if (detectable != null)
        {
            var agent = detectable.GetOwner();
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
    }

    private void OnTriggerExit(Collider other)
    {
        var detectable = other.GetComponent<Detectable>();
        if (detectable != null)
        {
            var agent = detectable.GetOwner();
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
    */

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (enemyAgents != null)
        {
            foreach (var agent in enemyAgents)
            {
                Gizmos.DrawLine(transform.position, agent.GetVehicle().transform.position);
            }
        }
    }
}
