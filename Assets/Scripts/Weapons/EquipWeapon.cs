using UnityEngine;

public class EquipWeapon : MonoBehaviour
{
    public Transform[] ShootPoints;

    private Collectible collectible;

    private void Awake()
    {
        collectible = GetComponentInChildren<Collectible>();
        if (collectible != null)
            collectible.OnCollect += OnCollect;
    }

    public void EnableCollect()
    {
        if (collectible != null)
            collectible.Enabled = true;
    }

    public void OnCollect()
    {
        Destroy(gameObject);
    }
}
