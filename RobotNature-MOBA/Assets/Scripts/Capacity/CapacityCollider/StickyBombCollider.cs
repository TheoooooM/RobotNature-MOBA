using Entities;
using Entities.Capacities;
using Photon.Pun;
using UnityEngine;

public class StickyBombCollider : Entity
{
    [HideInInspector] public Entity caster;
    [HideInInspector] public ActiveCapacity capacity;
    [HideInInspector] public float distance;
    
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (!CanDisable()) return;
        if (Vector3.Distance(caster.transform.position, transform.position) > distance)
        {
            rb.isKinematic = true;
            GetComponent<PhotonView>().RPC("DeactivateParticleSystemRPC", RpcTarget.All);
        }
    }

    protected virtual bool CanDisable()
    {
        return distance != 0;
    }

    public void Launch(Vector3 direction)
    {
        rb.isKinematic = false;
        rb.velocity = direction;
    }

    private void OnTriggerEnter(Collider other)
    {
        var affectedEntity = other.GetComponent<Entity>();
        if (!affectedEntity || affectedEntity == caster) return;
        if (PhotonNetwork.IsMasterClient) capacity.CollideEntityEffect(affectedEntity);
    }

    public virtual void Disable()
    {
        photonView.RPC("SyncDisableRPC", RpcTarget.All);
    }
    
    [PunRPC]
    public void SyncDisableRPC()
    {
        gameObject.SetActive(false);
    }
    
    [PunRPC]
    private void DeactivateParticleSystemRPC()
    {
        foreach (var componentParticleSystem in GetComponentsInChildren<ParticleSystem>())
        {
            componentParticleSystem.Stop();
        }
    }
}