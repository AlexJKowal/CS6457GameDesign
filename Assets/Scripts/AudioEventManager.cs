using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class AudioEventManager : MonoBehaviour
{
    public EventSound3D eventSound3DPrefab;

    public AudioClip ballBounceAudio;

    private UnityAction<Vector3> ballBounceEventListener;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        ballBounceEventListener = new UnityAction<Vector3>(ballBounceEventHandler);

    }

    private void OnEnable()
    {
        EventManager.StartListening<BallBounceEvent, Vector3>(ballBounceEventListener);
    }

    private void OnDisable()
    {
        EventManager.StopListening<BallBounceEvent, Vector3>(ballBounceEventListener);

    }

    void ballBounceEventHandler(Vector3 worldPos)
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
}