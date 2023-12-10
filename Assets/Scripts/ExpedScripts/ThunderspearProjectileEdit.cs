using Characters;
using Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Projectiles
{
    internal partial class ThunderspearProjectile : BaseProjectile
    {
        void KillMyHuman()
        {
            if (_owner == null || !(_owner is Human) || !_owner.IsMainCharacter())
                return;
            if (SettingsManager.InGameCurrent.Misc.ThunderspearPVP.Value)
                return;
            float radius = CharacterData.HumanWeaponInfo["Thunderspear"]["StunBlockRadius"].AsFloat / 1.6f;
            float range = CharacterData.HumanWeaponInfo["Thunderspear"]["StunRange"].AsFloat / 1.6f;
            Vector3 direction = _owner.Cache.Transform.position - transform.position;
            RaycastHit hit;
            if (Vector3.Distance(_owner.Cache.Transform.position, transform.position) < range)
            {
                if (Physics.SphereCast(transform.position, radius, direction.normalized, out hit, range, _blockMask))
                {
                    if (hit.collider.transform.root.gameObject == _owner.gameObject)
                    {
                        ((Human)_owner).DieToTS();
                    }
                }
            }
        }
        //added by Sysyfus Dec 6 2023 to make damage proportional to titan health and affected by distance of explosion from target
        int CalculateDamage2(BaseTitan titan, float radius, Collider collider)
        {
            float falloff = 1 - Mathf.Clamp((((-0.75f * radius) + Vector3.Distance(this.transform.position, collider.transform.position)) / (0.5f * radius)), 0f, 0.5f); //falloff should not exceed 50%
            int damage = (int)(falloff * (float)titan.GetComponent<BaseCharacter>().MaxHealth /* * (1 + InitialPlayerVelocity.magnitude / 250f)*/);
            int commonDamage = (int)(falloff * InitialPlayerVelocity.magnitude * 10f); //regular blade calculation
            if (damage < commonDamage) //damage back to regular blade calculation if exceeds necessary damage to kill
            {
                damage = commonDamage;
            }
            if (damage < 10) //minimum 10 damage no matter what
            {
                damage = 10;
            }
            return damage;
        }
    }
}
