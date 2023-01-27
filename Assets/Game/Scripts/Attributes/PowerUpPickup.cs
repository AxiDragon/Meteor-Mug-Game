using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    [SerializeField] private PowerUpScriptableObject[] powerUps;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private ParticleSystem pickupGrabParticles;
    [SerializeField] private TextMeshPro pickupGrabText;
    private bool pickedUp;

    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        transform.DOScale(Vector3.one, .5f).SetEase(Ease.OutBounce);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !pickedUp)
        {
            pickedUp = true;

            var powerUpScriptableObject = powerUps[Random.Range(0, powerUps.Length)];

            var newPowerUp = other.AddComponent<PowerUp>();
            newPowerUp.AssignPowerUpType(powerUpScriptableObject);

            Instantiate(pickupGrabParticles, transform.position, Quaternion.identity);

            var tmPro = Instantiate(pickupGrabText, transform.position + Vector3.one * 2f, Quaternion.identity);
            tmPro.text = powerUpScriptableObject.powerUpName;
            GetComponent<MMF_Player>().PlayFeedbacks();

            transform.DOScale(Vector3.zero, .5f).OnComplete(() => Destroy(gameObject));
        }
    }
}