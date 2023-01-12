using System.Collections.Generic;
using UnityEngine;

namespace Prototyping
{
    public class FlockController : MonoBehaviour
    {
        [HideInInspector] public List<ChickController> flock = new();

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ChickController chickController))
            {
                if (flock.Contains(chickController) || chickController.incompatibleFlockControllers.Contains(this))
                    return;
            
                chickController.StartFollowing(flock.Count == 0 ? transform : flock[^1].transform);
                flock.Add(chickController);
            }
        }

        public void RemoveFlockMember(int id)
        {
            flock.RemoveAt(id);

            if (flock.Count == 0)
                return;
        
            flock[id].StartFollowing(id == 0 ? transform : flock[id - 1].transform);
        }
    }
}
