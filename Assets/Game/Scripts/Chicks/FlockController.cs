using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;

public class FlockController : MonoBehaviour
{
    public ObservableCollection<ChickController> flock = new();
    [SerializeField] private Renderer colorRenderer;
    private Color flockColor;

    public Color FlockColor
    {
        get => flockColor;
        set
        {
            flockColor = value;
            colorRenderer.material.color = flockColor;
        }
    }

    private void Awake()
    {
        flock.CollectionChanged += FlockOnCollectionChanged;
    }

    private void FlockOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateFlockTargetOrder();
        UpdateFlockColor();
    }

    private void UpdateFlockColor()
    {
        for (int index = 0; index < flock.Count; index++)
        {
            flock[index].colorRenderer.material.color = flockColor;
        }
    }

    private void UpdateFlockTargetOrder()
    {
        for (int index = 0; index < flock.Count; index++)
        {
            flock[index].StartFollowing(index == 0 ? transform : flock[index - 1].transform);
            flock[index].agentMover.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ChickController chickController))
        {
            if (flock.Contains(chickController) || chickController.flockTimeout > 0f)
                return;

            flock.Add(chickController);
        }
    }

    public ChickController RemoveFlockMember(int index)
    {
        ChickController removedFlockMember = flock[index]; 
        flock.RemoveAt(index);

        return removedFlockMember;
    }
}