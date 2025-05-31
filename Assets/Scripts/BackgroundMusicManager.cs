using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] musicTracks;
    [SerializeField] private bool shuffleMode = false;

    private AudioSource audioSource;
    private int currentTrackIndex = 0;
    private System.Random random = new System.Random();
    private List<int> availableTracks = new List<int>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (musicTracks.Length > 0)
        {
            if (shuffleMode)
            {
                FillAvailableTracks();
                PlayNextTrack();
            }
            else
            {
                PlayTrack(currentTrackIndex);
            }
        }
    }

    private void FillAvailableTracks()
    {
        availableTracks = Enumerable.Range(0, musicTracks.Length).ToList();
    }

    private void PlayTrack(int trackIndex)
    {
        audioSource.clip = musicTracks[trackIndex];
        audioSource.Play();
    }

    private void PlayNextTrack()
    {
        if (musicTracks.Length == 0) return;

        if (shuffleMode)
        {
            if (availableTracks.Count == 0)
            {
                FillAvailableTracks();
            }

            if (availableTracks.Count == 0) return;

            int randomIndexInAvailableList = random.Next(availableTracks.Count);
            currentTrackIndex = availableTracks[randomIndexInAvailableList];
            availableTracks.RemoveAt(randomIndexInAvailableList);
        }
        else
        {
            currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
        }

        PlayTrack(currentTrackIndex);
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayNextTrack();
        }
    }
}
