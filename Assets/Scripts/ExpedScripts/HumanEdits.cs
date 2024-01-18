using GameManagers;
using Photon.Pun;
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

        private void OnTriggerEnter(Collider other)
        {
            //Added by Momo to check for Survey Point collision on 18th january 2024
            if (other.gameObject.tag == "SurveyPoint")
            {
                BoxCollider collider = other.gameObject.GetComponent<BoxCollider>();
                MeshRenderer obj = other.gameObject.GetComponentInChildren<MeshRenderer>();
                AudioSource audio = other.gameObject.GetComponentInChildren<AudioSource>();
                ParticleSystem ps = other.gameObject.GetComponentInChildren<ParticleSystem>();
                collider.enabled = false;
                RPCManager.PhotonView.RPC("CollectEmblemRPC", RpcTarget.AllViaServer, collider, obj, audio, ps);
            }
        }
    }
}
