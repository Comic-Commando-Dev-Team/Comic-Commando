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

        // Ustaw pierwszy utw�r i zacznij odtwarzanie
        if (musicTracks.Length > 0)
        {
            PlayTrack(currentTrackIndex);
        }
    }

    void Update()
    {
        // Sprawd�, czy utw�r si� sko�czy�
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
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length; // Przejd� do nast�pnego utworu
        PlayTrack(currentTrackIndex);
    }
}
