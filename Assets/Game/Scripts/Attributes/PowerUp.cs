using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public PowerUpType powerUpType;
    private GameObject powerUpObject;
    private PowerUpScriptableObject powerUpSO;

    private void OnDestroy()
    {
        if (powerUpObject != null)
            Destroy(powerUpObject);

        TriggerPowerUp(powerUpType, false);
    }

    public void AssignPowerUpType(PowerUpScriptableObject powerUp)
    {
        powerUpSO = powerUp;
        powerUpType = powerUp.powerUpType;

        if (powerUp.powerUpObject != null)
            powerUpObject = GetComponent<PlayerPowerUpManager>().AddPowerUpObject(powerUp.powerUpObject);

        TriggerPowerUp(powerUpType, true);
    }

    private void TriggerPowerUp(PowerUpType type, bool activate)
    {
        switch (type)
        {
            case PowerUpType.RunSpeedUp:
            {
                var pm = GetComponent<PlayerMovement>();
                pm.speed = activate
                    ? pm.speed * powerUpSO.modificationMultiplier
                    : pm.speed / powerUpSO.modificationMultiplier;

                break;
            }
            case PowerUpType.BiggerRange:
            {
                var pcg = GetComponentInChildren<PlayerChickGatherer>();
                pcg.transform.localScale = activate
                    ? pcg.transform.localScale * powerUpSO.modificationMultiplier
                    : pcg.transform.localScale / powerUpSO.modificationMultiplier;

                break;
            }
            case PowerUpType.HomingChicks:
            {
                var ct = GetComponent<ChickThrower>();
                ct.thrownChickMass = activate
                    ? ct.thrownChickMass * powerUpSO.modificationMultiplier
                    : ct.thrownChickMass / powerUpSO.modificationMultiplier;
                break;
            }
            case PowerUpType.ImpactUp:
            {
                var ct = GetComponent<ChickThrower>();

                ct.strikePower = activate
                    ? ct.strikePower * powerUpSO.modificationMultiplier
                    : ct.strikePower / powerUpSO.modificationMultiplier;

                ct.strikeRange = activate
                    ? ct.strikeRange * powerUpSO.modificationMultiplier
                    : ct.strikeRange / powerUpSO.modificationMultiplier;

                break;
            }
            case PowerUpType.SmallerChicks:
            case PowerUpType.BiggerChicks:
            {
                var ct = GetComponent<ChickThrower>();
                ct.thrownChickSize = activate
                    ? ct.thrownChickSize * powerUpSO.modificationMultiplier
                    : ct.thrownChickSize / powerUpSO.modificationMultiplier;
                break;
            }
            case PowerUpType.ThrowSpeedUp:
            {
                var ct = GetComponent<ChickThrower>();
                ct.throwingForce = activate
                    ? ct.throwingForce * powerUpSO.modificationMultiplier
                    : ct.throwingForce / powerUpSO.modificationMultiplier;

                break;
            }
        }

        StartCountdown();
    }

    private void StartCountdown()
    {
        Destroy(this, powerUpSO.powerUpDuration);
    }
}