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
            float radius = CharacterData.HumanWeaponInfo["Thunderspear"]["StunBlockRadius"].AsFloat / 1.5f;
            float range = CharacterData.HumanWeaponInfo["Thunderspear"]["StunRange"].AsFloat / 1.5f;
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
    }
}
