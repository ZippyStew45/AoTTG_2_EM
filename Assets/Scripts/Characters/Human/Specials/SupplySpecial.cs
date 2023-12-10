using ApplicationManagers;
using GameManagers;
using System.Collections;
using UnityEngine;
using Spawnables;

namespace Characters
{
    class SupplySpecial : BaseEmoteSpecial
    {
        protected override float ActiveTime => 0.5f;

        public SupplySpecial(BaseCharacter owner): base(owner)
        {
            UsesLeft = MaxUses = 3; //Changed by Momo Dec 7 2023 to use a seperate refill for supply special.
            Cooldown = 300;
        }

        protected override void Activate()
        {
            _human.EmoteAnimation(HumanAnimations.EmoteWave);
        }

        protected override void Deactivate()
        {
            _human.RefillSS(); //Added by Momo Dec 5 2023 to use stop supply can from spawning and skip straight to refill.
        }

        public override void Reset()
        {
            base.Reset();
            SetCooldownLeft(Cooldown);
        }
    }
}
