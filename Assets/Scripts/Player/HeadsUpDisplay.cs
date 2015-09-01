using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
    public Image Crosshair;

    private bool isTargetInSight;

    private static HeadsUpDisplay current;

    public static HeadsUpDisplay Current
    {
        get { return current; }
    }

    private void Awake()
    {
        isTargetInSight = false;
        current = this;
    }

    private void Update()
    {
        Crosshair.color = isTargetInSight ? Color.red : Color.white;
    }

    public void SetTargetInSight(bool value)
    {
        isTargetInSight = value;
    }
}