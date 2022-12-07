using System;
using System.Collections;
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
    [Header("=== AFFECT COLLIDER")]
    [SerializeField] private List<byte> effectIndex = new List<byte>();
    [SerializeField] private bool affectEntityOnly;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!PhotonNetwork.IsMasterClient) GetComponent<Collider>().enabled = false;
    }

    public void Launch(Vector3 moveVector)
    {
        rb.velocity = moveVector;
    }

    private void OnTriggerEnter(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        
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
