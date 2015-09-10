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

    public static bool IsInFrontOfCamera(Camera camera, Vector3 position)
    {
        if (camera == null)
            return false;
        var dotProduct = Vector3.Dot(position - camera.transform.position, camera.transform.forward);
        return dotProduct > 0f;
    }
}
