using ApplicationManagers;
using GameManagers;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Characters
{
    internal partial class Human : BaseCharacter
    {
        public Vector3 InitialPlayerVelocity;
        public void DieToTS()
        {
            GetHit(this, 100, "Thunderspear", "");
            Die();
        }
    }
}
