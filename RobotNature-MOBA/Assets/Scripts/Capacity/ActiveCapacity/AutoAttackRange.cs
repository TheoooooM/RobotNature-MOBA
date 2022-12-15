using System.Collections;
using System.Collections.Generic;
using Entities;
using Entities.Capacities;
using Photon.Pun;
using UnityEngine;

public class AutoAttackRange : ActiveCapacity
{
    private Champion champion;
    
    private AutoAttackRangeSO SOType;
    private Vector3 lookDir;
    private GameObject bullet;
    private AffectCollider collider;

    public override void OnStart()
    {
        SOType = (AutoAttackRangeSO)SO;
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
        champion.GetPassiveCapacity(CapacitySOCollectionManager.GetPassiveCapacitySOIndex(SOType.attackSlowSO)).OnAdded();
        champion.OnCastAnimationCast += CapacityEffect;
        champion.OnCastAnimationEnd += CapacityEndAnimation; 
        champion.canRotate = false;
    }

    public override void CapacityEffect(Transform castTransform)
    {
        champion.OnCastAnimationCast -= CapacityEffect;
        champion.GetPassiveCapacity(CapacitySOCollectionManager.GetPassiveCapacitySOIndex(SOType.overheatSO)).OnAdded();
        lookDir = targetPositions[0]-castTransform.position;
        lookDir.y = 0;
        var shootDir = lookDir;
        if (champion.isOverheat)
        {
            var rdm = Random.Range(-(SOType.sprayAngle / 2), (SOType.sprayAngle / 2));
            shootDir += new Vector3(Mathf.Cos(rdm), 0, Mathf.Sin(rdm)).normalized;
        };
        bullet = PoolNetworkManager.Instance.PoolInstantiate(SOType.bulletPrefab.GetComponent<Entity>(), castTransform.position, Quaternion.LookRotation(shootDir)).gameObject;
        collider = bullet.GetComponent<AffectCollider>();
        collider.caster = caster;
        collider.casterPos = castTransform.position;
        collider.maxDistance = SOType.maxRange;
        collider.capacitySender = this;
        collider.Launch(shootDir.normalized*SOType.bulletSpeed);
    }

    public override void CapacityEndAnimation()
    {
        champion.OnCastAnimationEnd -= CapacityEndAnimation;
        champion.GetPassiveCapacity(CapacitySOCollectionManager.GetPassiveCapacitySOIndex(SOType.attackSlowSO)).OnRemoved();
        champion.canRotate = true;
    }

    public override void CollideEntityEffect(Entity entityAffect)
    {
        if (caster.team == entityAffect.team) return;
        if (PhotonNetwork.IsMasterClient)
        {
            var lifeable = entityAffect.GetComponent<IActiveLifeable>();
            if (lifeable != null)
            {
                if (!lifeable.AttackAffected()) return;
                entityAffect.photonView.RPC("DecreaseCurrentHpRPC", RpcTarget.All, caster.GetComponent<Champion>().attackDamage * SOType.percentageDamage);
                collider.Disable();
            }
        }
        else
        { 
            bullet.gameObject.SetActive(false);
        }
    }

    public override void CollideObjectEffect(GameObject obj)
    {
       collider.Disable();
    }

    public override void PlayFeedback(int casterIndex, int[] targetsEntityIndexes, Vector3[] targetPositions)
    {
        /*if (PhotonNetwork.IsMasterClient) return;
        lookDir = targetPositions[0]-casterTransform.position;
        lookDir.y = 0;
        bullet = PoolLocalManager.Instance.PoolInstantiate(SOType.bulletPrefab, casterTransform.position, Quaternion.LookRotation(lookDir));
        bullet.GetComponent<AffectCollider>().Launch(lookDir*SOType.bulletSpeed);
        bullet.GetComponent<AffectCollider>().caster = caster;*/
    }
}
