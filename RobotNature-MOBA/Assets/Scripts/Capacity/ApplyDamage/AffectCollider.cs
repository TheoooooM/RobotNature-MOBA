using System.Collections.Generic;
using Entities;
using Entities.Capacities;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AffectCollider : Entity
{
    [HideInInspector] public Entity caster;
    [HideInInspector] public ActiveCapacity capacitySender;
    [HideInInspector] public float maxDistance = 0;
    [HideInInspector] public Vector3 casterPos;
    [Header("=== AFFECT COLLIDER")]
    [SerializeField] private List<byte> effectIndex = new List<byte>();
    [SerializeField] private bool affectEntityOnly;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!PhotonNetwork.IsMasterClient) GetComponent<Collider>().enabled = false;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (maxDistance == 0) return;
        if (Vector3.Distance(casterPos, transform.position) > maxDistance)
        {
            switch (capacitySender.SO.shootType)
            {
                case Enums.CapacityShootType.skillshot:
                    Disable();
                    break;

                case Enums.CapacityShootType.targetEntity:
                    break;

                case Enums.CapacityShootType.targetPosition:
                    rb.velocity = Vector3.zero;
                    break;
            }
        }
    }

    public void Launch(Vector3 moveVector)
    {
        rb.velocity = moveVector;
    }

    private void OnTriggerEnter(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        
        // if the entity is not null and is not the caster
        if (entity && entity != caster)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                capacitySender.CollideEntityEffect(entity);
            }
        }
        else if (!entity && !affectEntityOnly)
        {
            capacitySender.CollideObjectEffect(other.gameObject);
        }
    }
    
    public void Disable()
    {
        photonView.RPC("SyncDisableRPC", RpcTarget.All);
    }
    
    [PunRPC]
    public void SyncDisableRPC()
    {
        gameObject.SetActive(false);
    }
}