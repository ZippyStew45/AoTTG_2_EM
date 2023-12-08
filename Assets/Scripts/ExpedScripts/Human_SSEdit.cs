using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    internal partial class Human : BaseCharacter
    {
        public string targetTag = "SupplySpawnable";
        public float targetRadius = 10f;
        public bool RefillSS()
        {
            State = HumanState.RefillSS;
            ToggleSparks(false);
            CrossFade(HumanAnimations.Refill, 0.1f);
            PlaySound(HumanSounds.Refill);
            _stateTimeLeft = Cache.Animation[HumanAnimations.Refill].length;
            return true;
        }
        public bool NeedRefillSS()
        {
            if (CurrentGas < MaxGas)
                return true;
            return false;
        }
        public void FinishRefillSS()
        {
            CurrentGas += 15f;
            if (CurrentGas > MaxGas)
            {
                CurrentGas = MaxGas;
            }
            GameObject target = GameObject.FindGameObjectWithTag(targetTag);
            Vector3 currentPosition = transform.position;

            float distance = Vector3.Distance(target.transform.position, currentPosition);

            if (distance <= targetRadius)
            {
                Destroy(target);
            }
        }
    }
}
