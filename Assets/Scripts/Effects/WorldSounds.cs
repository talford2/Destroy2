using UnityEngine;

public static class WorldSounds
{
    private static GameObject worldSoundPrefab;

    public static void PlayClipAt(Vector3 position, AudioClip clip)
    {
        if (worldSoundPrefab == null)
            worldSoundPrefab = Resources.Load<GameObject>("Effects/WorldSound");
        var worldSound = ((GameObject)Object.Instantiate(worldSoundPrefab, position, Quaternion.identity)).GetComponent<WorldSound>();
        worldSound.PlayClip(clip);
    }
}
