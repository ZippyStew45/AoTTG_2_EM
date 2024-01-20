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
        public void DieToTS() //Added by Momo Dec 6 2023 to kill people too close to the explosion. and print 100 damage.
        {
            GetHit("Thunderspear", 100, "Thunderspear", "");
            Die();
        }
        //SUPPLY SPECIAL EDIT //Added by Momo Dec 5 2023 to use a seperate refill for supply special.
        public bool RefillSS()
        {
            State = HumanState.RefillSS;
            ToggleSparks(false);
            CrossFade(HumanAnimations.Refill, 0.1f);
            PlaySound(HumanSounds.Refill);
            _stateTimeLeft = Cache.Animation[HumanAnimations.Refill].length;
            return true;
        }
        public void FinishRefillSS() //Added by Momo Dec 5 2023 to use a seperate refill for supply special.
        {
            CurrentGas += 15f;
            if (CurrentGas > MaxGas)
            {
                CurrentGas = MaxGas;
            }
        }

        public void FixedUpdateStandStill(Vector3 gravity) //Added by Sysyfus Jan 19 2024 so characters can stand still and not slide down slopes they can stand on
        {
            if(CanJump() && Cache.Animation.IsPlaying(StandAnimation) && Cache.Rigidbody.velocity.magnitude < 1f)
            {
                Vector3 inverseGravity = -gravity * Mathf.Clamp(((Cache.Rigidbody.velocity.magnitude - 0.8f) / -0.55f), 0f, 1f);
                Cache.Rigidbody.AddForce(inverseGravity);
            }
        }
    }
}
