using UnityEngine;
using Photon.Pun;

public class DistanceCheck : MonoBehaviourPun
{
    [Header("Load For...")]
    public bool LocalOnly = true;
    public bool MasterClient = true;

    [Header("Collider")]
    public bool DisableColliders = true;
    public float DisableCollidersDistance = 500f;

    [Header("Game Object")]
    public bool DisableGameObjects = true;
    public float DisableChildrenDistance = 1000f;

    [Header("Check Interval")]
    public float Seconds = 5f;

    private void Start()
    {
        InvokeRepeating("CheckDistance", 0f, Seconds);
    }

    void CheckDistance()
    {
        GameObject player = FindClosestPlayer();
        Debug.Log(player);

        if (!LocalOnly)
            MasterClient = false;

        if (player == null)
            return;

        if (LocalOnly && !player.GetPhotonView().IsMine)
        {
            if (MasterClient && PhotonNetwork.LocalPlayer.IsMasterClient) { }
            else 
                return;
        }

        if (MasterClient && !PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            if (LocalOnly && player.GetPhotonView().IsMine) { }
            else
                return;
        }


        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (DisableGameObjects)
        {
            if (distance < DisableChildrenDistance)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        if (DisableColliders)
        {
            if (distance < DisableCollidersDistance)
            {
                foreach (Transform child in transform)
                {
                    Collider childCollider = child.GetComponent<Collider>();
                    if (childCollider != null)
                    {
                        childCollider.enabled = true;
                    }
                }
            }
            else
            {
                foreach (Transform child in transform)
                {
                    Collider childCollider = child.GetComponent<Collider>();
                    if (childCollider != null)
                    {
                        childCollider.enabled = false;
                    }
                }
            }
        }
    }

    GameObject FindClosestPlayer()
    {
        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject player in players)
        {
            Vector3 diff = player.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = player;
                distance = curDistance;
            }
        }
        return closest;
    }
}