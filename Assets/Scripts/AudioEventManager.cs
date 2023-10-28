using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class AudioEventManager : MonoBehaviour
{
    public EventSound3D eventSound3DPrefab;

    public AudioClip ballBounceAudio;
    public AudioClip cheeringAudio;

    private UnityAction<Vector3, SquareLocation> ballBounceEventListener;
    private UnityAction<Vector3> cheeringEventListener;

    public AudioClip[] backgroundMusicArr; 
    private AudioSource backgroundMusicSource;

    public bool musicEnabled;


    void Start()
    {
        if (musicEnabled && backgroundMusicArr != null && backgroundMusicArr.Length > 0)
        {
            PlayBackgroundMusic();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!backgroundMusicSource.isPlaying)
        {
            PlayBackgroundMusic();
        }
    }

    private void Awake()
    {
        ballBounceEventListener = new UnityAction<Vector3, SquareLocation>(ballBounceEventHandler);
        cheeringEventListener = new UnityAction<Vector3>(cheeringEventHandler);

        backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        backgroundMusicSource.loop = true;
        backgroundMusicSource.spatialBlend = 0;
        backgroundMusicSource.volume = 0.1f;

    }

    private void OnEnable()
    {
        EventManager.StartListening<BallBounceEvent, Vector3, SquareLocation>(ballBounceEventListener);
        EventManager.StartListening<CheeringEvent, Vector3>(cheeringEventListener);
    }

    private void OnDisable()
    {
        EventManager.StopListening<BallBounceEvent, Vector3, SquareLocation>(ballBounceEventListener);
        EventManager.StopListening<CheeringEvent, Vector3>(cheeringEventListener);

    }

    void ballBounceEventHandler(Vector3 worldPos, SquareLocation square)
    {
        if (eventSound3DPrefab)
        {

            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.clip = this.ballBounceAudio;

            snd.audioSrc.minDistance = 50f;
            snd.audioSrc.maxDistance = 500f;

            snd.audioSrc.Play();
        }
    }

    void cheeringEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.clip = this.cheeringAudio;

            snd.audioSrc.minDistance = 50f;
            snd.audioSrc.maxDistance = 500f;

            snd.audioSrc.Play();
        }
    }

    private void PlayBackgroundMusic()
    {
        int backgroundMusicIndex = Random.Range(0, backgroundMusicArr.Length);

        backgroundMusicSource.clip = backgroundMusicArr[backgroundMusicIndex];
        backgroundMusicSource.Play();
    }
}
