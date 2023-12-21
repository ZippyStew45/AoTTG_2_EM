using Characters;
using CustomLogic;
using Map;
using Photon.Pun;
using Settings;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Utility;

namespace Projectiles
{
    internal partial class ThunderspearProjectile : BaseProjectile
    {
        bool gravity = true;
        bool attached = false;
        float falloff = 1f;
        GameObject attachParent = null;
        Collider attachCollider = null;
        Vector3 relativeAttachPoint = new Vector3(0f,0f,0f);
        AudioSource tsCharge;

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
            //falloff = 1 - Mathf.Clamp((((-0.75f * radius) + Vector3.Distance(this.transform.position, collider.transform.position)) / (0.5f * radius)), 0f, 0.5f); //falloff should not exceed 50%
            falloff = 1 - Mathf.Clamp((((-0.63f * radius) + Vector3.Distance(this.transform.position, collider.transform.position)) / (0.49333f * radius)), 0f, 0.75f); //falloff should not exceed 75%
            int damage = (int)(falloff * (float)titan.GetComponent<BaseCharacter>().MaxHealth /* * (1 + InitialPlayerVelocity.magnitude / 250f)*/);
            int commonDamage = (int)(falloff * InitialPlayerVelocity.magnitude * 10f); //regular blade calculation
            if (damage < commonDamage) //damage back to regular blade calculation if exceeds necessary damage to kill
            {
                //damage = commonDamage; //commented out for fall off and damage tier testing
            }
            if (damage < 10) //minimum 10 damage no matter what
            {
                damage = 10;
            }
            return damage;
        }
        //added by Sysyfus Dec 20 2023 to use regular damage calc but with falloff
        int CalculateDamage3(BaseTitan titan, float radius, Collider collider)
        {
            int damage = Mathf.Max((int)(InitialPlayerVelocity.magnitude * 10f *
                CharacterData.HumanWeaponInfo["Thunderspear"]["DamageMultiplier"].AsFloat), 10);
            if (_owner != null && _owner is Human)
            {
                var human = (Human)_owner;
                if (human.CustomDamageEnabled)
                    return human.CustomDamage;
            }
            falloff = 1 - Mathf.Clamp((((-0.63f * radius) + Vector3.Distance(this.transform.position, collider.transform.position)) / (0.49333f * radius)), 0f, 0.75f); //falloff should not exceed 75%
            damage = (int)((float)damage * falloff);
            if (damage < 10)
                damage = 10;
            return damage;
        }

        //added by Sysyfus Dec 20 2023
        //when raw damage insufficient to kill titan, hidden bonus damage may apply based on quality of aim
        void AdjustTitanHealth(BaseTitan titan, int damage, Collider collider)
        {
            int newHealth = titan.CurrentHealth - damage;

            int newDamage = 0;
            if (newHealth > 0) //if raw damage insufficient to kill titan, calculate % max HP damage
            {
                if (falloff >= 0.99f)    //A tier, good aim, instant kill regardless of damage 
                {
                    newDamage = titan.MaxHealth;
                }
                else if (falloff > 0.25f) //B tier, decent aim, bonus damage up to 50% of titan's maximum health
                {
                    newDamage = titan.MaxHealth / 2;
                }
                else //C tier, you barely hit the nape, bonus damage up to 25% of titan's maximum health
                {
                    newDamage = titan.MaxHealth / 4;
                }
            }

            if (damage < newDamage) //apply bonus damage if raw damage lower
            {
                newHealth = titan.CurrentHealth - newDamage;
            }

            if (newHealth < 0) // no negative HP >:(
            {
                newHealth = 0;
            }

            titan.GetHit(_owner, damage, "Thunderspear", collider.name); //still show the raw damage number in feed
            titan.SetCurrentHealth(newHealth); //make sure health adjusted to proper level
        }

        //added by Sysyfus Dec 20 2023 to make TS stick to surface before exploding
        void Attach(Collision collision)
        {
            this._timeLeft = 1f; //TS explodes after 1 second

            attachParent = collision.collider.gameObject;
            attachCollider = collision.collider;
            Vector3 point = attachCollider.ClosestPoint(collision.GetContact(0).point);
            relativeAttachPoint = point - attachCollider.transform.position;
            relativeAttachPoint = attachCollider.transform.InverseTransformPoint(point);
            //transform.position = attachCollider.transform.position + relativeAttachPoint;
            transform.position = attachCollider.transform.TransformPoint(relativeAttachPoint);

            //transform.SetParent(attachCollider.transform);;
            tsCharge = GetComponent<AudioSource>();
            tsCharge.Play();
            attached = true;

        }
        void Attach(RaycastHit hit)
        {
            this._timeLeft = 1f; //TS explodes after 1 second


            attachParent = hit.collider.gameObject;
            relativeAttachPoint = attachParent.transform.position - hit.point;
            transform.position = attachParent.transform.position + relativeAttachPoint;
            tsCharge = GetComponent<AudioSource>();
            tsCharge.Play();
            attached = true;

        }


        private Vector3 GetAttachedPosition()
        {
            return attachCollider.transform.TransformPoint(relativeAttachPoint);
            //return attachCollider.transform.position + relativeAttachPoint;
        }

    }
}
