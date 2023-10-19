using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShotScript : MonoBehaviour
{
    private bool smashEnabled = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ToggleSmash()
    {
        smashEnabled = !smashEnabled;
        ShotType currentShotState = smashEnabled ? ShotType.smash_shot : ShotType.lob_shot;
        EventManager.TriggerEvent<ShotTypeEvent, ShotType>(currentShotState);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
