using Entities;
using Entities.Capacities;
using GameStates;
using Photon.Pun;
using UnityEngine;

public class StickyBomb : ActiveCapacity
{
    private Champion champion;

    private StickyBombSO SOType;
    private GameObject stickyBombGO;
    private double timer;
    private Vector3 lookDir;
    private PhotonView photonView;

    public override void OnStart()
    {
        SOType = (StickyBombSO)SO;
        casterTransform = caster.transform;
        champion = (Champion)caster;
    }
    
    public override bool TryCast(int[] targetsEntityIndexes, Vector3[] targetPositions)
    {
        if(!base.TryCast(targetsEntityIndexes, targetPositions)) return false;
        return true;
    }

    public override void CapacityPress()
    {
        champion.OnCastAnimationCast += CapacityEffect;
        champion.OnCastAnimationEnd += CapacityEndAnimation;
        champion.canRotate = false;
    }

    public override void CapacityEffect(Transform castTransform)
    {
        champion.OnCastAnimationCast -= CapacityEffect;
        lookDir = targetPositions[0]-casterTransform.position;
        lookDir.y = 0;
        stickyBombGO = PoolLocalManager.Instance.PoolInstantiate(SOType.feedbackPrefab, casterTransform.position, Quaternion.LookRotation(lookDir));
        stickyBombGO.GetComponent<MeshRenderer>().enabled = true;
        var collider = stickyBombGO.GetComponent<AffectCollider>(); 
        collider.GetComponent<SphereCollider>().radius = SOType.radiusStick;
        collider.maxDistance = SOType.maxRange;
        collider.casterPos = caster.transform.position;
        collider.capacitySender = this;
        collider.caster = caster;
        collider.Launch(lookDir.normalized * SOType.speedBomb);
        GameStateMachine.Instance.OnTick += TimerBomb;
    }
    
    public override void CapacityEndAnimation()
    {
        champion.OnCastAnimationEnd -= CapacityEndAnimation;
        champion.canRotate = true;
    }

    public override void CollideFeedbackEffect(Entity affectedEntity)
    {
        var liveable = affectedEntity.GetComponent<IActiveLifeable>();
        if (liveable == null) return;
        if (affectedEntity.name.Contains("Minion")) return;
        stickyBombGO.GetComponent<Rigidbody>().isKinematic = true;
        stickyBombGO.transform.parent = affectedEntity.transform;
        stickyBombGO.transform.position += new Vector3(0, affectedEntity.transform.localScale.y, 0) + Vector3.up * 2;
    }

    public override void PlayFeedback(int casterIndex, int[] targetsEntityIndexes, Vector3[] targetPositions)
    {
        if (PhotonNetwork.IsMasterClient) return;
        lookDir = targetPositions[0] - casterTransform.position;
        lookDir.y = 0;
        stickyBombGO = PoolLocalManager.Instance.PoolInstantiate(SOType.feedbackPrefab, casterTransform.position, Quaternion.LookRotation(lookDir));
        
        stickyBombGO.GetComponent<MeshRenderer>().enabled = true;
        var collider = stickyBombGO.GetComponent<AffectCollider>();
        collider.GetComponent<SphereCollider>().radius = SOType.radiusStick;
        collider.maxDistance = SOType.maxRange;
        collider.casterPos = caster.transform.position;
        collider.capacitySender = this;
        collider.caster = caster;
        collider.Launch(lookDir.normalized * SOType.speedBomb);
        GameStateMachine.Instance.OnTick += TimerBomb;
    }

    private void TimerBomb()
    {
        timer += 1;
        if(timer < SOType.durationBomb * GameStateMachine.Instance.tickRate) return;
        GameStateMachine.Instance.OnTick -= TimerBomb;
        timer = 0;
        ExplodeBomb();
    }

    private void ExplodeBomb()
    {
        var entities = Physics.OverlapSphere(stickyBombGO.transform.position, SOType.radiusExplosion);
        foreach (Collider entity in entities)
        {
            var entityAffect = entity.GetComponent<Entity>();
            if (entityAffect == null || caster.team == entityAffect.team) continue;
            var liveable = entity.GetComponent<IActiveLifeable>();
            if (liveable == null || !liveable.AttackAffected()) continue;
            PoolLocalManager.Instance.RequestPoolInstantiate(SOType.feedbackHitPrefab, entityAffect.transform.position, Quaternion.identity);
            liveable.RequestDecreaseCurrentHp(caster.GetComponent<Champion>().attackDamage * SOType.percentageDamage);
        }
        stickyBombGO.GetComponent<MeshRenderer>().enabled = false;
        stickyBombGO.GetComponentInChildren<ParticleSystem>().Play();
        GameStateMachine.Instance.OnTick += DestroyExplosion;
    }

    private void DestroyExplosion()
    {
        if(!stickyBombGO.GetComponentInChildren<ParticleSystem>().isStopped) return;
        if(stickyBombGO) stickyBombGO.gameObject.SetActive(false);
        GameStateMachine.Instance.OnTick -= DestroyExplosion;
    }
}