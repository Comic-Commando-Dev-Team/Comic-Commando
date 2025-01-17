using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public AudioClip[] musicTracks; // Tablica z utworami
    private AudioSource audioSource;
    private int currentTrackIndex = 0;

    void Start()
    {
        // Pobierz AudioSource
        audioSource = GetComponent<AudioSource>();

        // Ustaw pierwszy utwór i zacznij odtwarzanie
        if (musicTracks.Length > 0)
        {
            PlayTrack(currentTrackIndex);
        }
    }

    void Update()
    {
        // SprawdŸ, czy utwór siê skoñczy³
        if (!audioSource.isPlaying)
        {
            NextTrack();
        }
    }

    private void PlayTrack(int trackIndex)
    {
        audioSource.clip = musicTracks[trackIndex];
        audioSource.Play();
    }

    private void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length; // PrzejdŸ do nastêpnego utworu
        PlayTrack(currentTrackIndex);
    }
}
