using ApplicationManagers;
using Characters;
using GameManagers;
using Photon.Realtime;
using Settings;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Utility;
using static UnityEngine.EventSystems.StandaloneInputModule;

//WaterBehaviors created by Sysyfus Jan 9 2024

namespace Characters
{

    internal partial class BaseCharacter : Photon.Pun.MonoBehaviourPunCallbacks
    {

        public bool isInWater = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
            {
                isInWater = true;
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if(other.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
            {
                isInWater = true;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
            {
                isInWater = false;
            }
        }

        public virtual void FixedUpdateInWater()
        {
            var colliders = Physics.OverlapSphere(this.transform.position + Vector3.up * 0.8f, 0.4f);
            if (colliders.Length > 0 && isInWater)
            {
                isInWater = false;
                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
                    {
                        isInWater = true;
                        break;
                    }
                }
            }
            if (isInWater)
            {
                GetComponent<Rigidbody>().velocity *= 0.954992586021f; //0.978267385729 reduce speed by 2/3 per second 0.954992586021 90% per sec
            }
        }

    }

    internal partial class Human : BaseCharacter
    {
        float timeSinceLastBounce = 0f;
        float waterSpeed;
        private void OnTriggerEnter(Collider other)
        {
            #region Bounce on water surface

            if(photonView.IsMine && other.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
            {
                if (Cache.Rigidbody.velocity.magnitude > 35f && timeSinceLastBounce > 0.25f)
                {
                    float horiSpeed = Mathf.Pow((Cache.Rigidbody.velocity.x * Cache.Rigidbody.velocity.x) + (Cache.Rigidbody.velocity.z * Cache.Rigidbody.velocity.z), 0.5f);
                    float vertSpeed = Mathf.Abs(Cache.Rigidbody.velocity.y);

                    //  check for 'shallow' angle of impact              check if falling
                    if (horiSpeed > /*1.1547f * */ vertSpeed && Cache.Rigidbody.velocity.y < 0f)
                    {
                        Cache.Rigidbody.velocity = new Vector3(Cache.Rigidbody.velocity.x * 0.6f, (vertSpeed * 0.08f) + horiSpeed * 0.05f + 2f, Cache.Rigidbody.velocity.z * 0.6f);
                        timeSinceLastBounce = 0f;
                    }
                }
            }

            #endregion
        }
        public override void FixedUpdateInWater()
        {
            base.FixedUpdateInWater();

            timeSinceLastBounce += Time.fixedDeltaTime;

            if (isInWater && photonView.IsMine)
            {

                #region Floating
                bool shoulderIsInWater = false;
                Transform shoulderpos = this.HumanCache.Neck;
                var colliders2 = Physics.OverlapSphere(shoulderpos.position, 0.05f);
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
                //this.RunSpeed = waterSpeed;
                if (num != 0f && num2 != 0f)
                {
                    waterSpeed *= 0.707107f;
                }
                float upDown = Vector3.Dot(SceneLoader.CurrentCamera.Camera.transform.forward, Vector3.up);
                //ChatManager.AddLine(upDown.ToString());

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
            isInWater = false;
            Collider[] hitColliders = Physics.OverlapSphere(this.transform.position + (Vector3.up * floatDepth), 0.05f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.layer == LayerMask.NameToLayer("WaterVolume"))
                {
                    isInWater = true;
                }
            }

            if (isInWater)
            {
                if (!this.Grounded)
                {
                    xRotation = -20f;
                }

                Cache.Rigidbody.velocity *= 0.95f;
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

    internal partial class Hook : MonoBehaviour
    {
        bool isInWater = false;

        public void FixedUpdateInWater()
        {
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
            else
            {
                isInWater = false;
            }

            if (isInWater)
            {
                _baseVelocity *= 0.963f; //0.927842 original value, ~37-38 units range. Changed to 0.963 to get ~60 range
                _baseVelocity += Vector3.down * 0.0075f; //original value 0.005, changed to 0.0075 to make more visible with higher range
                _relativeVelocity *= 0.963f;
            }
        }
    }

    abstract partial class BaseTitan : BaseCharacter
    {
        override public void FixedUpdateInWater()
        {
            if (isInWater)
            {
                Cache.Rigidbody.velocity = new Vector3(Cache.Rigidbody.velocity.x * 0.5f, Cache.Rigidbody.velocity.y * 0.99f, Cache.Rigidbody.velocity.z * 0.5f);
                Cache.Rigidbody.velocity += Vector3.up * 1.9f;
            }
        }
    }

}

//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------

namespace Projectiles
{

    internal partial class BaseProjectile : BaseMovementSync
    {
        bool isInWater = false;

        public void FixedUpdateInWater()
        {
            var colliders = Physics.OverlapSphere(transform.position, 0.05f);
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
            }
            else
            {
                isInWater = false;
            }

            if (isInWater)
            {
                //ChatManager.AddLine("projectileinwater");
                GetComponent<Rigidbody>().velocity *= 0.927842f;

            }
        }
    }

}
