using ApplicationManagers;
using Characters;
using Effects;
using GameManagers;
using Photon.Realtime;
using Settings;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Utility;
using static UnityEngine.EventSystems.StandaloneInputModule;

//WaterBehaviors created by Sysyfus Jan 9 2024

namespace Characters
{

    internal partial class BaseCharacter : Photon.Pun.MonoBehaviourPunCallbacks
    {

        public bool isInWater = false;
        public Collider[] waterColliders = new Collider[1];
        public int numWaterColliders = 0;
        public bool shoulderIsInWater = false;

        public virtual void FixedUpdateInWater()
        {
            isInWater = false;

            int numWaterColliders = Physics.OverlapSphereNonAlloc(this.transform.position + Vector3.up * 0.8f, 0.4f, waterColliders, (1 << 28));
            if (numWaterColliders > 0)
            {
                isInWater = true;
            }

            if (isInWater)
            {
                GetComponent<Rigidbody>().velocity *= 0.954992586021f; //0.978267385729 reduce speed by 2/3 per second 0.954992586021 90% per sec
            }
        }

    }


    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    internal partial class Human : BaseCharacter
    {
        float timeSinceLastBounce = 0f;
        float waterSpeed;

        public override void FixedUpdateInWater()
        {
            base.FixedUpdateInWater();

            timeSinceLastBounce += Time.fixedDeltaTime;

            if (isInWater && photonView.IsMine)
            {

                #region Floating
                shoulderIsInWater = false;
                Transform shoulderpos = this.HumanCache.Neck;
                /*var colliders2 = Physics.OverlapSphere(shoulderpos.position, 0.05f);
                if (colliders2.Length > 0)
                {
                    foreach (Collider collider in colliders2)
                    {
                        if (collider.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
                        {
                            shoulderIsInWater = true;
                            break;
                        }
                    }
                }*/

                numWaterColliders = Physics.OverlapSphereNonAlloc(shoulderpos.position, 0.05f, waterColliders, (1 << 28));
                if (numWaterColliders > 0)
                {
                    shoulderIsInWater = true;
                }
                if (shoulderIsInWater)
                {
                    GetComponent<Rigidbody>().velocity += Vector3.up * 0.7f;
                }
                #endregion

                #region Swimming Controller
                float num;
                if(SettingsManager.InputSettings.General.Forward.GetKey())
                {
                    num = 1f;
                }
                else if (SettingsManager.InputSettings.General.Back.GetKey())
                {
                    num = -1f;
                }
                else
                {
                    num = 0f;
                }
                float num2;
                if (SettingsManager.InputSettings.General.Left.GetKey())
                {
                    num2 = -1f;
                }
                else if (SettingsManager.InputSettings.General.Right.GetKey())
                {
                    num2 = 1f;
                }
                else
                {
                    num2 = 0f;
                }

                waterSpeed = this.RunSpeed / 2f;
                if (num != 0f && num2 != 0f)
                {
                    waterSpeed *= 0.707107f;
                }
                float upDown = Vector3.Dot(SceneLoader.CurrentCamera.Camera.transform.forward, Vector3.up);

                if (num > 0f)
                {
                    Cache.Rigidbody.AddForce(SceneLoader.CurrentCamera.Camera.transform.forward * waterSpeed + 7.1f * new Vector3(0f, Mathf.Clamp(upDown, -1f, 0f), 0f));
                }
                if (num < 0f)
                {
                    Cache.Rigidbody.AddForce(-(SceneLoader.CurrentCamera.Camera.transform.forward * waterSpeed + 7.1f * new Vector3(0f, Mathf.Clamp(upDown, 0f, 1f), 0f)));
                }
                if (num2 > 0f)
                {
                    Cache.Rigidbody.AddForce(SceneLoader.CurrentCamera.Camera.transform.right * waterSpeed);
                }
                if (num2 < 0f)
                {
                    Cache.Rigidbody.AddForce(-SceneLoader.CurrentCamera.Camera.transform.right * waterSpeed);
                }
                #endregion
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    internal partial class Horse : BaseCharacter
    {
        public override void FixedUpdateInWater()
        {
            base.FixedUpdateInWater();

            float xRotation = 0f;

            float floatDepth = 2.1f;
            floatDepth -= 0.5f * Cache.Rigidbody.velocity.magnitude / this.RunSpeed;
            if (_owner.MountState == HumanMountState.Horse)
            {
                floatDepth += 0.1f;
            }

            shoulderIsInWater = false;
            /*Collider[] hitColliders = Physics.OverlapSphere(this.transform.position + (Vector3.up * floatDepth), 0.05f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
                {
                    shoulderIsInWater = true;
                }
            }*/
            numWaterColliders = Physics.OverlapSphereNonAlloc(this.transform.position + (Vector3.up * floatDepth), 1f, waterColliders, (1 << 28));
            if (numWaterColliders > 0)
            {
                shoulderIsInWater = true;
            }

            if (isInWater)
            {
                if (!this.Grounded)
                {
                    xRotation = -20f;
                }

                Cache.Rigidbody.velocity *= 0.95f;
                if (shoulderIsInWater)
                    Cache.Rigidbody.velocity += Vector3.up * 0.7f;

                /* drowning
                bool headIsInWater = false;
                
                var colliders2 = Physics.OverlapSphere(this.transform.position + this.transform.forward * 1.5f + Vector3.up * 2.3f, 0.05f);
                if (colliders2.Length > 0)
                {
                    foreach (Collider collider in colliders2)
                    {
                        if (collider.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
                        {
                            headIsInWater = true;
                            break;
                        }
                    }
                }
                if (headIsInWater)
                {
                    //drown?
                }
                */

            }
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(xRotation, this.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * 2.5f);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    internal partial class Hook : MonoBehaviour
    {
        bool isInWater = false;
        Collider[] waterColliders = new Collider[1];

        public void FixedUpdateInWater()
        {
            /*
            var colliders = Physics.OverlapSphere(_hookPosition, 0.05f);
            isInWater = false;
            if (colliders.Length > 0)
            {
                foreach(Collider collider in colliders)
                {
                    if (collider.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
                    {
                        isInWater = true;
                        break;
                    }
                }
            }
            */
            isInWater = false;
            int numWaterColliders = Physics.OverlapSphereNonAlloc(_hookPosition, 0.05f, waterColliders, (1 << 28));
            if (numWaterColliders > 0)
            {
                isInWater = true;
            }

            if (isInWater)
            {
                _baseVelocity *= 0.963f; //0.927842 original value, ~37-38 units range. Changed to 0.963 to get ~60 range
                _baseVelocity += Vector3.down * 0.0075f; //original value 0.005, changed to 0.0075 to make more visible with higher range
                _relativeVelocity *= 0.963f;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    abstract partial class BaseTitan : BaseCharacter
    {
        override public void FixedUpdateInWater()
        {
            base.FixedUpdateInWater();
            if (isInWater)
            {
                //Cache.Rigidbody.velocity = new Vector3(Cache.Rigidbody.velocity.x * 0.5f, Cache.Rigidbody.velocity.y * 0.99f, Cache.Rigidbody.velocity.z * 0.5f);
                //Cache.Rigidbody.velocity += Vector3.up * 1.9f;
                //Cache.Rigidbody.velocity += Vector3.up * 30f;

                #region Floating
                if (this.State != TitanState.Jump)
                {
                    shoulderIsInWater = false;
                    Transform shoulderpos = this.BaseTitanCache.NapeHurtbox.transform;

                    numWaterColliders = Physics.OverlapSphereNonAlloc(shoulderpos.position, 1f, waterColliders, (1 << 28));
                    if (numWaterColliders > 0)
                    {
                        shoulderIsInWater = true;
                    }

                    if (shoulderIsInWater)
                        Cache.Rigidbody.AddForce(-1.1f * Gravity, ForceMode.Acceleration);
                    //if (shoulderIsInWater)
                    //{
                    //Cache.Rigidbody.AddForce(Vector3.up * 210f, ForceMode.Acceleration);
                    //Cache.Rigidbody.velocity += Vector3.up * 110f;
                    //ChatManager.AddLine("shoulderinwater");
                    //}
                    //else
                    //ChatManager.AddLine("shouldernotinwater");
                }
                #endregion
            }
        }

        public virtual void LandNoVFX()
        {
            StateAction(TitanState.Land, BaseTitanAnimations.Land);
        }
    }

}

//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

namespace Projectiles
{

    internal partial class BaseProjectile : BaseMovementSync
    {
        bool isInWater = false;
        Collider[] waterColliders = new Collider[1];

        public void FixedUpdateInWater()
        {
            /*var colliders = Physics.OverlapSphere(transform.position, 0.05f);
            isInWater = false;
            if (colliders.Length > 0)
            {
                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
                    {
                        isInWater = true;
                        break;
                    }
                }
            }*/

            isInWater = false;
            int numWaterColliders = Physics.OverlapSphereNonAlloc(transform.position, 0.05f, waterColliders, (1 << 28));
            if (numWaterColliders > 0)
            {
                isInWater = true;
            }

            if (isInWater)
            {
                //ChatManager.AddLine("projectileinwater");
                GetComponent<Rigidbody>().velocity *= 0.927842f;

            }
        }
    }

}

//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

namespace Cameras
{
    partial class InGameCamera : BaseCamera
    {
        public bool isInWater = false;
        Collider[] waterColliders = new Collider[1];

        private void UpdateWater()
        {
            int numWaterColliders = Physics.OverlapSphereNonAlloc(Camera.transform.position, 0.05f, waterColliders, (1 << 28));
            if (numWaterColliders > 0)
            {
                isInWater = true;
            }
            //if(isInWater)
            //{
                //ChatManager.AddLine(this.GameObject().layer.ToString());
            //}
            //else
                //ChatManager.AddLine("Camera out of water");

        }
    }
}

//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

public class UnderWaterRendererFeature : ScriptableRendererFeature
{
    public override void Create()
    {

    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {

    }
}

//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

public class UnderWaterRenderPass : ScriptableRenderPass
{
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

    }
}