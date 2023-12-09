using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    internal partial class Human : BaseCharacter
    {
        public string targetTag = "SupplySpawnable";
        public float targetRadius = 10f;

        //THUNDERSPEAR EDIT
        public void DieToTS()
        {
            GetHit(this, 100, "Thunderspear", "");
            Die();
        }
        //SUPPLY SPECIAL EDIT
        public bool RefillSS()
        {
            State = HumanState.RefillSS;
            ToggleSparks(false);
            CrossFade(HumanAnimations.Refill, 0.1f);
            PlaySound(HumanSounds.Refill);
            _stateTimeLeft = Cache.Animation[HumanAnimations.Refill].length;
            return true;
        }
        public void FinishRefillSS()
        {
            CurrentGas += 15f;
            if (CurrentGas > MaxGas)
            {
                CurrentGas = MaxGas;
            }
        }
    }
}
