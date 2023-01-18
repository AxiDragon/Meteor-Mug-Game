using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Power Up", menuName = "Power Up")]
public class PowerUpScriptableObject : ScriptableObject
{
    public PowerUpType powerUpType;
    public float powerUpDuration;
    public float modificationMultiplier;
    public string powerUpName;
    public GameObject powerUpObject;
}
