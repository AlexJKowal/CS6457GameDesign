using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ShotType
{
    lob_shot,
    smash_shot
}
public class BallHitEvent : UnityEvent<SquareLocation, ShotType> { }