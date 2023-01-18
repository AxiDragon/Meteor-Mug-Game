using DG.Tweening;
using UnityEngine;

public class PowerUpText : MonoBehaviour
{
    private void Start()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 1.5f).SetEase(Ease.OutCubic).OnComplete(() => transform.DOScale(Vector3.zero, .2f)
            .SetEase(Ease.InCubic).OnComplete(() => Destroy(gameObject)));
    }
}
