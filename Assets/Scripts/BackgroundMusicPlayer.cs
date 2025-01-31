using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script manages background music by looping through a playlist of AudioClips.
/// It starts with a random track and then continues sequentially from there.
/// Attach this script to a GameObject in your scene, assign the desired AudioClips in the inspector,
/// and the music will start playing on awake and continue looping through the playlist.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicPlayer : MonoBehaviour
{
    /// <summary>
    /// List of background music tracks to play.
    /// Assign these in the Inspector.
    /// </summary>
    [Tooltip("List of background music tracks to play in order.")]
    public List<AudioClip> playlist = new List<AudioClip>();

    /// <summary>
    /// The AudioSource component used to play the music.
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// Index of the currently playing track in the playlist.
    /// </summary>
    private int currentTrackIndex = 0;

    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake()
    {
        // Get or add the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource settings for background music
        audioSource.playOnAwake = false; // We'll handle playing manually
        audioSource.loop = false; // We'll handle looping through the playlist

        // Start playing a random track if the playlist is not empty
        if (playlist.Count > 0)
        {
            // Select a random starting index
            currentTrackIndex = Random.Range(0, playlist.Count);
            PlayCurrentTrack();
        }
        else
        {
            Debug.LogWarning("BackgroundMusicPlayer: Playlist is empty. Please assign AudioClips in the Inspector.");
        }
    }

    /// <summary>
    /// Plays the current track and starts the coroutine to wait for it to finish.
    /// </summary>
    private void PlayCurrentTrack()
    {
        if (playlist.Count == 0)
            return;

        AudioClip clipToPlay = playlist[currentTrackIndex];
        if (clipToPlay == null)
        {
            Debug.LogWarning($"BackgroundMusicPlayer: AudioClip at index {currentTrackIndex} is null. Skipping to next track.");
            PlayNextTrack();
            return;
        }

        audioSource.clip = clipToPlay;
        audioSource.Play();
        StartCoroutine(WaitForTrackEnd(clipToPlay.length));
    }

    /// <summary>
    /// Coroutine that waits for the current track to finish before playing the next one.
    /// </summary>
    /// <param name="clipLength">Length of the current AudioClip in seconds.</param>
    /// <returns></returns>
    private IEnumerator WaitForTrackEnd(float clipLength)
    {
        // Wait for the clip to finish playing
        yield return new WaitForSeconds(clipLength);

        // Play the next track
        PlayNextTrack();
    }

    /// <summary>
    /// Advances to the next track in the playlist and plays it.
    /// Loops back to the first track if at the end of the playlist.
    /// </summary>
    private void PlayNextTrack()
    {
        if (playlist.Count == 0)
            return;

        // Advance the track index
        currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
        PlayCurrentTrack();
    }

    /// <summary>
    /// Optional: Allows adding tracks to the playlist at runtime.
    /// </summary>
    /// <param name="newClip">The AudioClip to add to the playlist.</param>
    public void AddTrack(AudioClip newClip)
    {
        if (newClip != null)
        {
            playlist.Add(newClip);
        }
        else
        {
            Debug.LogWarning("BackgroundMusicPlayer: Attempted to add a null AudioClip to the playlist.");
        }
    }

    /// <summary>
    /// Optional: Allows removing tracks from the playlist at runtime.
    /// </summary>
    /// <param name="clipToRemove">The AudioClip to remove from the playlist.</param>
    public void RemoveTrack(AudioClip clipToRemove)
    {
        if (playlist.Contains(clipToRemove))
        {
            playlist.Remove(clipToRemove);
        }
        else
        {
            Debug.LogWarning("BackgroundMusicPlayer: Attempted to remove an AudioClip that is not in the playlist.");
        }
    }
}
