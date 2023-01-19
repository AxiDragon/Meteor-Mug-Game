using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;

public class FlockController : MonoBehaviour
{
    [SerializeField] private Renderer[] colorRenderers;
    public ObservableCollection<ChickController> flock = new();
    private Color flockColor;

    public Color FlockColor
    {
        get => flockColor;
        set
        {
            flockColor = value;
            for (var i = 0; i < colorRenderers.Length; i++) colorRenderers[i].material.color = flockColor;
        }
    }

    private void Awake()
    {
        flock.CollectionChanged += FlockOnCollectionChanged;
    }

    public event Action<int> onFlockChanged;

    private void FlockOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateFlockTargetOrder();
        onFlockChanged?.Invoke(flock.Count);
    }

    private void UpdateFlockTargetOrder()
    {
        for (var index = 0; index < flock.Count; index++)
            if (flock[index] != null && (index == 0 || flock[index - 1] != null))
            {
                flock[index].StartFollowing(index == 0 ? transform : flock[index - 1].transform);
                flock[index].agentMover.enabled = true;
            }
    }

    public ChickController RemoveFlockMember(int index)
    {
        var removedFlockMember = flock[index];
        removedFlockMember.SetChickColor();

        flock.RemoveAt(index);

        return removedFlockMember;
    }

    public bool AddFlockMember(ChickController newFlockMember, int index = -1)
    {
        if (flock.Contains(newFlockMember) || newFlockMember.flockTimeout > 0f)
            return false;

        if (newFlockMember.owner != null) newFlockMember.owner.RemoveFlockMember(newFlockMember);

        if (index == -1)
            flock.Add(newFlockMember);
        else
            flock.Insert(index, newFlockMember);

        newFlockMember.trail.enabled = false;
        newFlockMember.TogglePhysics(false);
        newFlockMember.SetTimeout();
        newFlockMember.SetChickColor(flockColor);
        newFlockMember.owner = this;

        return true;
    }

    public void RemoveFlockMember(ChickController removedFlockMember)
    {
        if (removedFlockMember == null)
            return;

        removedFlockMember.SetChickColor();
        flock.Remove(removedFlockMember);
    }

    public int GetChickCount()
    {
        return flock.Count;
    }

    public void ScatterFlock(float force = 0f, float radius = 5f)
    {
        for (var i = 0; i < flock.Count; i++) flock[i].rb.AddExplosionForce(force, transform.position, 5f);

        ReleaseFlock();
    }

    public void ReleaseFlock()
    {
        for (var i = flock.Count - 1; i >= 0; i--) RemoveFlockMember(flock[i]);
    }
}