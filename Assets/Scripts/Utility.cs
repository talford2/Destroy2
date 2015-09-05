using UnityEngine;

public class Utility {

    public static void SetLayerRecursively(Transform parentTransform, int layer)
    {
        parentTransform.gameObject.layer = layer;
        foreach (Transform trans in parentTransform)
        {
            trans.gameObject.layer = layer;
            SetLayerRecursively(trans, layer);
        }
    }
}
