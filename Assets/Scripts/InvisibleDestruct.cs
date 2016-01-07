using UnityEngine;

public class InvisibleDestruct : MonoBehaviour
{
    public float MinLifeTime = 15f;
    private float lifeCooldown = 0;

    private Renderer visibleRenderer;

    private void Awake()
    {
        lifeCooldown = MinLifeTime;
        visibleRenderer = gameObject.GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        if (lifeCooldown > 0f)
            lifeCooldown -= Time.deltaTime;
        if (lifeCooldown < 0 && !visibleRenderer.isVisible)
            Destroy(gameObject);
    }
}
