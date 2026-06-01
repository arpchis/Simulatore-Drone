using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip musicClip;
    [Range(0f, 1f)] public float volume = 0.5f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f; // 2D
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        audioSource.Play();
        Debug.Log("Musica avviata: " + musicClip.name);
    }
}
