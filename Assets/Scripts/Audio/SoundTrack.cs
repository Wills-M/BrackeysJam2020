using UnityEngine;
using System.Collections;

[System.Serializable]
[CreateAssetMenu(fileName = "SoundTrack")]
public class SoundTrack : ScriptableObject
{   

    public SoundType type;
    public AudioClip clip;

    [Range(0, 1)]
    public float volume = 1;
    public bool loop = false;
}

public enum SoundType
{
    NONE = 0,

    // Looping music
    BG_MUSIC = 001,

    // Player sound effects
    PLAYER_MOVE = 101,
    GHOST_MOVE = 102,

    // UI
    MOVE_CURSOR = 201,
    SELECT = 202,
    EXIT = 203,

}
