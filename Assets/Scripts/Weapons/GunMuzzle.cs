using UnityEngine;

public class GunMuzzle : MonoBehaviour
{
    public GameObject BaseFlash;

    public float FlashTime = 0.1f;

    private float coolDown = 0;

    private Renderer[] renderables;
    private bool show;

    private void Awake()
    {
        renderables = GetComponentsInChildren<Renderer>();
        DisplayRenderables(false);
    }

    private void Update()
    {
        if (show)
        {
            coolDown -= Time.deltaTime;
            if (coolDown <= 0)
            {
                DisplayRenderables(false);
                show = false;
            }
        }
    }

    private void DisplayRenderables(bool show)
    {
        foreach (var renderable in renderables)
        {
            renderable.enabled = show;
        }
    }

    public void Flash()
    {
        coolDown = FlashTime;
        DisplayRenderables(true);
        if (BaseFlash != null)
        {
            //BaseFlash.transform.localRotation = Quaternion.Euler(BaseFlash.transform.localRotation.x, BaseFlash.transform.localRotation.y, Random.Range(0, 360));
            BaseFlash.transform.Rotate(0, 0, Random.Range(0, 360));
        }
        show = true;
    }
}

