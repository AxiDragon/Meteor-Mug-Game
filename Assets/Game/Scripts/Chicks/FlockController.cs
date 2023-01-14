using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;

public class FlockController : MonoBehaviour
{
    public ObservableCollection<ChickController> flock = new();
    public event Action<int> onFlockChanged;
    [SerializeField] private Renderer colorRenderer;
    protected Color flockColor;

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
        onFlockChanged?.Invoke(flock.Count);
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

            AddFlockMember(chickController);
        }
    }

    public ChickController RemoveFlockMember(int index)
    {
        ChickController removedFlockMember = flock[index];
        removedFlockMember.SetChickColor();

        flock.RemoveAt(index);

        return removedFlockMember;
    }

    public void AddFlockMember(ChickController newFlockMember, int index = -1)
    {
        if (newFlockMember.owner != null) newFlockMember.owner.RemoveFlockMember(newFlockMember);

        if (index == -1)
        {
            flock.Add(newFlockMember);
        }
        else
        {
            flock.Insert(index, newFlockMember);
        }

        newFlockMember.SetTimeout();
        newFlockMember.SetChickColor(flockColor);
        newFlockMember.owner = this;
    }

    public void RemoveFlockMember(ChickController removedFlockMember)
    {
        removedFlockMember.SetChickColor();
        flock.Remove(removedFlockMember);
    }

    public ChickController GetChick(int index)
    {
        return flock[index];
    }

    public int GetChickCount()
    {
        return flock.Count;
    }
}