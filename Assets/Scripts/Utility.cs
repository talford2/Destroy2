using System.Collections;
using UnityEngine;
using UnityEngine.Networking.Match;

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

    public static void PlaySoundAt(AudioClip clip, Vector3 position)
    {
        var obj = new GameObject("TempSound");
        obj.transform.position = position;
        var audioSource = obj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.spatialBlend = 1f;
        audioSource.volume = 1f;
        audioSource.maxDistance = 500f;
        audioSource.minDistance = 5f;
        audioSource.Play();

        var selfDestructor = obj.AddComponent<SelfDestructor>();
        selfDestructor.Cooldown = audioSource.clip.length;
    }
}
