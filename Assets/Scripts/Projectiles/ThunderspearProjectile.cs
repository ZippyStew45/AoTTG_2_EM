﻿using Settings;
using UnityEngine;
using Photon;
using Characters;
using System.Collections.Generic;
using System.Collections;
using Effects;
using ApplicationManagers;
using GameManagers;
using UI;
using Utility;
using CustomLogic;
using Cameras;
using Photon.Pun;

namespace Projectiles
{
    internal partial class ThunderspearProjectile : BaseProjectile
    {
        Color _color;
        float _radius;
        public Vector3 InitialPlayerVelocity;
        Vector3 _lastPosition;
        static LayerMask _collideMask = PhysicsLayer.GetMask(PhysicsLayer.MapObjectAll, PhysicsLayer.MapObjectEntities, PhysicsLayer.MapObjectProjectiles,
            PhysicsLayer.TitanPushbox);
        static LayerMask _blockMask = PhysicsLayer.GetMask(PhysicsLayer.MapObjectAll, PhysicsLayer.MapObjectEntities, PhysicsLayer.MapObjectProjectiles,
            PhysicsLayer.TitanPushbox, PhysicsLayer.Human);

        protected override void SetupSettings(object[] settings)
        {
            _radius = (float)settings[0];
            _color = (Color)settings[1];
            _lastPosition = transform.position;
        }

        protected override void RegisterObjects()
        {
            var trail = transform.Find("Trail").GetComponent<ParticleSystem>();
            var flame = transform.Find("Flame").GetComponent<ParticleSystem>();
            var model = transform.Find("ThunderspearModel").gameObject;
            tsCharge = GetComponent<AudioSource>(); //added by Sysyfus Jan 4 2024
            _hideObjects.Add(flame.gameObject);
            _hideObjects.Add(model);
            if (SettingsManager.AbilitySettings.ShowBombColors.Value)
            {
                var main = trail.main;
                main.startColor = _color;
                main = flame.main;
                main.startColor = _color;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (photonView.IsMine && !Disabled)
            {
                if (CharacterData.HumanWeaponInfo["Thunderspear"]["Ricochet"].AsBool)
                    GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * _velocity.magnitude * CharacterData.HumanWeaponInfo["Thunderspear"]["RicochetSpeed"].AsFloat;
                else
                {
                    //Explode(); //removed by Sysyfus Dec 20 2023 for sticky TS
                    if(!attached)
                        Attach(collision);
                    _rigidbody.velocity = Vector3.zero;
                }
            }
        }

        protected override void OnExceedLiveTime()
        {
            Explode();
        }
        public void Explode()
        {
            if (!Disabled)
            {
                float effectRadius = _radius * 5f;
                if (SettingsManager.InGameCurrent.Misc.ThunderspearPVP.Value)
                    effectRadius = _radius * 2f;
                bool killedPlayer = KillPlayersInRadius(_radius);
                bool killedTitan = KillTitansInRadius(_radius);
                EffectSpawner.Spawn(EffectPrefabs.ThunderspearExplode, transform.position, transform.rotation, effectRadius, true, new object[] { _color, killedPlayer || killedTitan });
                StunMyHuman();
                DestroySelf();
                KillMyHuman(); //Added by Momo Dec 6 2023 to kill people too close to the explosion.
                photonView.RPC("StopChargeEffectRPC", RpcTarget.AllViaServer, new object[0]); //Added by Sysyfus Jan 4 2024
                gravity = false;
            }
        }

        void StunMyHuman()
        {
            if (_owner == null || !(_owner is Human) || !_owner.IsMainCharacter())
                return;
            if (SettingsManager.InGameCurrent.Misc.ThunderspearPVP.Value)
                return;
            float radius = CharacterData.HumanWeaponInfo["Thunderspear"]["StunBlockRadius"].AsFloat;
            float range = CharacterData.HumanWeaponInfo["Thunderspear"]["StunRange"].AsFloat;
            Vector3 direction = _owner.Cache.Transform.position - transform.position;
            RaycastHit hit;
            if (Vector3.Distance(_owner.Cache.Transform.position, transform.position) < range)
            {
                if (Physics.SphereCast(transform.position, radius, direction.normalized, out hit, range, _blockMask))
                {
                    if (hit.collider.transform.root.gameObject == _owner.gameObject)
                    {
                        ((Human)_owner).GetStunnedByTS(transform.position);                                                      
                    }
                }
            }
        }

        bool KillTitansInRadius(float radius)
        {
            var position = transform.position;
            var colliders = Physics.OverlapSphere(position, radius, PhysicsLayer.GetMask(PhysicsLayer.Hurtbox));
            bool killedTitan = false;
            foreach (var collider in colliders)
            {
                var titan = collider.transform.root.gameObject.GetComponent<BaseTitan>();
                var handler = collider.gameObject.GetComponent<CustomLogicCollisionHandler>();
                if (handler != null)
                {
                    handler.GetHit(_owner, "Thunderspear", 100, "Thunderspear");
                    continue;
                }
                if (titan != null && titan != _owner && !TeamInfo.SameTeam(titan, _team) && !titan.Dead)
                {
                    if (collider == titan.BaseTitanCache.NapeHurtbox && CheckTitanNapeAngle(position, titan.BaseTitanCache.Head))
                    {
                        if (_owner == null || !(_owner is Human))
                            titan.GetHit("Thunderspear", 100, "Thunderspear", collider.name);
                        else
                        {
                            var damage = CalculateDamage4(titan, radius, collider); //changed by Sysyfus Dec 21 2023 to CalculateDamage4 //changed by Sysyfus Dec 20 2023 to CalculateDamage3 //changed by Sysyfus Dec 6 2023 from CalculateDamage() to CalculateDamage2()
                            ((InGameMenu)UIManager.CurrentMenu).ShowKillScore(damage);
                            ((InGameCamera)SceneLoader.CurrentCamera).TakeSnapshot(titan.BaseTitanCache.Neck.position, damage);
                            titan.GetHit(_owner, damage, "Thunderspear", collider.name); //removed by Sysyfus Dec 20 2023 to accommodate accuracy tier damage
                            //AdjustTitanHealth(titan, damage, collider); //Added by Sysyfus Dec 20 2023 to accommodate accuracy tier damage
                            //titan.SetCurrentHealth(0); //added by Sysyfus Dec 13 2023 to kill titan regardless of damage
                        }
                        killedTitan = true;
                    }
                }
            }
            return killedTitan;
        }

        bool KillPlayersInRadius(float radius)
        {
            var gameManager = (InGameManager)SceneLoader.CurrentGameManager;
            var position = transform.position;
            bool killedHuman = false;
            foreach (Human human in gameManager.Humans)
            {
                if (human == null || human.Dead)
                    continue;
                if (Vector3.Distance(human.Cache.Transform.position, position) < radius && human != _owner && !TeamInfo.SameTeam(human, _team))
                {
                    if (_owner == null || !(_owner is Human))
                        human.GetHit("", 100, "Thunderspear", "");
                    else
                    {
                        var damage = CalculateDamage();
                        human.GetHit(_owner, damage, "Thunderspear", "");
                        ((InGameMenu)UIManager.CurrentMenu).ShowKillScore(damage);
                        ((InGameCamera)SceneLoader.CurrentCamera).TakeSnapshot(human.Cache.Transform.position, damage);
                    }
                    killedHuman = true;
                }
            }
            return killedHuman;
        }

        int CalculateDamage()
        {
            int damage = Mathf.Max((int)(InitialPlayerVelocity.magnitude * 10f * 
                CharacterData.HumanWeaponInfo["Thunderspear"]["DamageMultiplier"].AsFloat), 10);
            if (_owner != null && _owner is Human)
            {
                var human = (Human)_owner;
                if (human.CustomDamageEnabled)
                    return human.CustomDamage;
            }
            return damage;
        }

        bool CheckTitanNapeAngle(Vector3 position, Transform nape)
        {
            Vector3 direction = (position - nape.position).normalized;
            return Vector3.Angle(-nape.forward, direction) < CharacterData.HumanWeaponInfo["Thunderspear"]["RestrictAngle"].AsFloat;
        }

        protected override void Update()
        {
            base.Update();
            if (_photonView.IsMine)
            {
                if (GetComponent<Rigidbody>().velocity.magnitude > 0f)
                    transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity);
            }
        }

        protected void FixedUpdate()
        {
            
            if (_photonView.IsMine)
            {
                if (!attached)
                {
                    GetComponent<Rigidbody>().velocity *= 0.94f; //added by Sysyfus Dec 6 2023 to simulate wind resistance
                    if (gravity)
                        GetComponent<Rigidbody>().velocity -= new Vector3(0f, 3.2f, 0f); //added by Sysyfus Dec 6 2023 to simulate gravity

                    RaycastHit hit;
                    Vector3 direction = (transform.position - _lastPosition);
                    if (Physics.SphereCast(_lastPosition, 0.5f, direction.normalized, out hit, direction.magnitude, _collideMask))
                    {
                        transform.position = hit.point;
                        Attach(hit);
                    }
                    _lastPosition = transform.position;
                }
                else
                    transform.position = GetAttachedPosition(); //Changed by Sysyfus Dec 20 2023 for sticky TS
            }
        }
    }
}
