using UnityEngine;

public class PlayerPowerUpManager : MonoBehaviour
{
    [SerializeField] private Transform powerUpParent;
    private FlockController flockController;

    private void Awake()
    {
        flockController = GetComponent<FlockController>();
    }

    public GameObject AddPowerUpObject(GameObject go)
    {
        var powerUpObject = Instantiate(go, powerUpParent);
        powerUpObject.transform.localPosition = Vector3.zero;
        powerUpObject.transform.localRotation = Quaternion.identity;

        if (powerUpObject.TryGetComponent(out MeshRenderer rend)) rend.material.color = flockController.FlockColor;

        foreach (var r in powerUpObject.GetComponentsInChildren<MeshRenderer>())
            r.material.color = flockController.FlockColor;

        return powerUpObject;
    }
}