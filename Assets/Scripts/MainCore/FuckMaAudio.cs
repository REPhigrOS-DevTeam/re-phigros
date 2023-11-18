using System.Collections;
using System.Collections.Generic;
using MaTech.Audio;
using UnityEngine;

public class FuckMaAudio : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;

    private AudioSample audioSample;

    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        MaAudio.LoadForUnity();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.clip = audioClip;
        Init();
    }

    async void Init()
    {
        audioSample = await AudioSample.LoadFromAudioClip(audioClip);
        audioSample.Volume = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && audioSample != null)
        {
            audioSample.Channel = MaAudio.ChannelAutoAssign.index;
            audioSample.PlayImmediate();
        }

        if (Input.GetKeyDown(KeyCode.S) && audioSource)
        {
            audioSource.Play();
        }
    }
}
