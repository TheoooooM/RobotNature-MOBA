using System;
using System.Collections;
using System.Collections.Generic;
using Entities.Capacities;
using Entities.FogOfWar;
using GameStates;
using Photon.Pun;
using UI.InGame;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.Minion
{
    public class Minion : Entity, IMovable, IAttackable, IActiveLifeable
    {
        #region Minion Variables

        public NavMeshAgent myAgent;
        private MinionController myController;

        [Header("Pathfinding")] 
        public List<Transform> myWaypoints = new List<Transform>();
        public List<Building.Building> TowersList = new List<Building.Building>();
        public int waypointIndex;
        public int towerIndex;

        public enum MinionAggroState
        {
            None,
            Tower,
            Minion,
            Champion
        };

        public enum MinionAggroPreferences
        {
            Tower,
            Minion,
            Champion
        }

        [Header("Attack Logic")] 
        public MinionAggroState currentAggroState = MinionAggroState.None;
        public MinionAggroPreferences whoAggro = MinionAggroPreferences.Tower;
        public LayerMask enemyMinionMask;
        public GameObject currentAttackTarget;
        public List<GameObject> whoIsAttackingMe = new List<GameObject>();
        public bool attackCycle;

        [Header("Stats")]
        public float attackDamage;
        public float attackSpeed;
        [Range(2, 8)] public float attackRange;
        public float delayBeforeAttack;

        public Transform meshParent;

        #endregion

        protected override void OnStart()
        {
            base.OnStart();
            myAgent = GetComponent<NavMeshAgent>();
            myController = GetComponent<MinionController>();
            UIManager.Instance.InstantiateHealthBarForEntity(entityIndex);
            UIManager.Instance.InstantiateResourceBarForEntity(entityIndex);
            if (GameStateMachine.Instance.GetPlayerTeam() != team)
            {
                meshParent.gameObject.SetActive(false);
            }
            elementsToShow.Add(meshParent.gameObject);
        }

        private void OnEnable()
        {
            RequestSetCurrentHp(maxHp);
            waypointIndex = 0;
        }

        #region State Methods

        public void IdleState()
        {
            myAgent.isStopped = true;
            CheckObjectives();
        }

        public void WalkingState()
        {
            CheckMyWaypoints();
            CheckObjectives();
            CheckEnemiesMinion();
        }

        public void LookingForPathingState()
        {
            if (myWaypoints is null) return;
            if (myWaypoints.Count > 0)
            {
                myAgent.SetDestination(myWaypoints[waypointIndex].position);
                myController.currentState = MinionController.MinionState.Walking;
            }
            else
            {
                myController.currentState = MinionController.MinionState.Idle;
            }
            
        }

        public void AttackingState()
        {
            if (currentAggroState == MinionAggroState.Minion)
            {
                if (currentAttackTarget.activeSelf)
                {
                    var q = Quaternion.LookRotation(currentAttackTarget.transform.position - transform.position);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 50f * Time.deltaTime);

                    if (attackCycle == false)
                    {
                        StartCoroutine(AttackLogic());
                    }
                }
                else
                {
                    myController.currentState = MinionController.MinionState.LookingForPathing;
                    currentAggroState = MinionAggroState.None;
                    currentAttackTarget = null;
                }
            }
            else if (currentAggroState == MinionAggroState.Tower)
            {
                if (TowersList[towerIndex].isAlive)
                {
                    var q = Quaternion.LookRotation(currentAttackTarget.transform.position - transform.position);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 50f * Time.deltaTime);
                    if (attackCycle == false)
                    {
                        StartCoroutine(AttackLogic());
                    }
                }
                else
                {
                    myController.currentState = MinionController.MinionState.LookingForPathing;
                    currentAggroState = MinionAggroState.None;
                    currentAttackTarget = null;
                    towerIndex++;
                }
            }
        }

        #endregion
        
        #region Check Methods

        private void CheckMyWaypoints()
        {
            if (Vector3.Distance(transform.position, myWaypoints[waypointIndex].transform.position) <= myAgent.stoppingDistance)
            {
                if (waypointIndex < myWaypoints.Count - 1)
                {
                    waypointIndex++;
                    myAgent.SetDestination(myWaypoints[waypointIndex].position);
                }
                else
                {
                    myController.currentState = MinionController.MinionState.Idle;
                }
            }
        }

        private void CheckObjectives()
        {
            if (TowersList is null) return;
            if (!TowersList[towerIndex].isAlive) return;

            if (Vector3.Distance(transform.position, TowersList[towerIndex].transform.position) > attackRange)
            {
                myController.currentState = MinionController.MinionState.Walking;
            }
            else
            {
                myAgent.SetDestination(transform.position);
                myController.currentState = MinionController.MinionState.Attacking;
                currentAggroState = MinionAggroState.Tower;
                currentAttackTarget = TowersList[towerIndex].gameObject;
            }
        }
        
        private void CheckEnemiesMinion()
        {
            var size = Physics.OverlapSphere(transform.position, attackRange, enemyMinionMask);
            if (size.Length < 1) return;
        
            for (int i = 0; i < size.Length; i++)
            {
                if (!size[i].CompareTag(gameObject.tag))
                {
                    myAgent.SetDestination(transform.position);
                    myController.currentState = MinionController.MinionState.Attacking;
                    currentAggroState = MinionAggroState.Minion;
                    currentAttackTarget = size[i].gameObject;
                    break;
                }
            }
        }
        
        #endregion

        private IEnumerator AttackLogic()
        {
            if (TowersList[towerIndex].isAlive)
            {
                attackCycle = true;
                AttackTarget(currentAttackTarget);
                // TODO: Already have cooldown in Capacity, remove this
                yield return new WaitForSeconds(attackSpeed);
                attackCycle = false;
            }
        }

        private void AttackTarget(GameObject target)
        {
            int[] targetEntity = new[] { target.GetComponent<Entity>().entityIndex };

            RequestAttack(2, targetEntity, Array.Empty<Vector3>());
        }

        #region Attackable

        private bool canAttack = true;
        
        public bool CanAttack()
        {
            return canAttack;
        }

        public void RequestSetCanAttack(bool value)
        {
            photonView.RPC("SetCanAttackRPC", RpcTarget.MasterClient, value);
        }

        public void SyncSetCanAttackRPC(bool value)
        {
            OnSetCanAttackFeedback?.Invoke(value);
        }

        public void SetCanAttackRPC(bool value)
        {
            OnSetCanAttack?.Invoke(value);
            photonView.RPC("SyncSetCanAttackRPC", RpcTarget.All, value);
        }

        public event GlobalDelegates.BoolDelegate OnSetCanAttack;
        public event GlobalDelegates.BoolDelegate OnSetCanAttackFeedback;

        public float GetAttackDamage()
        {
            throw new System.NotImplementedException();
        }

        public void RequestSetAttackDamage(float value)
        {
            photonView.RPC("SetAttackDamageRPC", RpcTarget.MasterClient, value);
        }

        [PunRPC]
        public void SyncSetAttackDamageRPC(float value)
        {
            OnSetAttackDamageFeedback?.Invoke(value);
        }

        [PunRPC]
        public void SetAttackDamageRPC(float value)
        {
            OnSetAttackDamage?.Invoke(value);
            photonView.RPC("SyncSetAttackDamageRPC", RpcTarget.All, value);
        }

        public event GlobalDelegates.FloatDelegate OnSetAttackDamage;
        public event GlobalDelegates.FloatDelegate OnSetAttackDamageFeedback;

        public void RequestAttack(byte capacityIndex, int[] targetedEntities, Vector3[] targetedPositions)
        {
            photonView.RPC("AttackRPC", RpcTarget.MasterClient, capacityIndex, targetedEntities, targetedPositions);
        }

        [PunRPC]
        public void SyncAttackRPC(byte capacityIndex, int[] targetedEntities, Vector3[] targetedPositions)
        {
            var attackCapacity = CapacitySOCollectionManager.CreateActiveCapacity(capacityIndex, this);
            attackCapacity.PlayFeedback(capacityIndex, targetedEntities, targetedPositions);
            OnAttackFeedback?.Invoke(capacityIndex, targetedEntities, targetedPositions);
        }

        [PunRPC]
        public void AttackRPC(byte capacityIndex, int[] targetedEntities, Vector3[] targetedPositions)
        {
            var attackCapacity = CapacitySOCollectionManager.CreateActiveCapacity(capacityIndex, this);

            if (!attackCapacity.TryCast(entityIndex, targetedEntities, targetedPositions)) return;

            OnAttack?.Invoke(capacityIndex, targetedEntities, targetedPositions);
            photonView.RPC("SyncAttackRPC", RpcTarget.All, capacityIndex, targetedEntities, targetedPositions);
        }

        public event GlobalDelegates.ByteIntArrayVector3ArrayDelegate OnAttack;
        public event GlobalDelegates.ByteIntArrayVector3ArrayDelegate OnAttackFeedback;

        public void RequestIncreaseAttackDamage(float value)
        {
            photonView.RPC("IncreaseAttackDamageRPC", RpcTarget.MasterClient, value);
        }

        public void SyncIncreaseAttackDamageRPC(float value)
        {
            OnIncreaseAttackDamageFeedback?.Invoke(value);
        }

        public void IncreaseAttackDamageRPC(float value)
        {
            OnIncreaseAttackDamage?.Invoke(value);
            photonView.RPC("SyncIncreaseAttackDamageRPC", RpcTarget.All, value);
        }

        public event GlobalDelegates.FloatDelegate OnIncreaseAttackDamage;
        public event GlobalDelegates.FloatDelegate OnIncreaseAttackDamageFeedback;

        public void RequestDecreaseAttackDamage(float value)
        {
            photonView.RPC("DecreaseAttackDamageRPC", RpcTarget.MasterClient, value);
        }

        public void SyncDecreaseAttackDamageRPC(float value)
        {
            OnDecreaseAttackDamageFeedback?.Invoke(value);
        }

        public void DecreaseAttackDamageRPC(float value)
        {
            OnDecreaseAttackDamage?.Invoke(value);
            photonView.RPC("SyncDecreaseAttackDamageRPC", RpcTarget.All, value);
        }

        public event GlobalDelegates.FloatDelegate OnDecreaseAttackDamage;
        public event GlobalDelegates.FloatDelegate OnDecreaseAttackDamageFeedback;

        public void RequestIncreaseAttackSpeed(float value)
        {
            photonView.RPC("IncreaseAttackSpeedRPC", RpcTarget.MasterClient, value);
        }

        public void SyncIncreaseAttackSpeedRPC(float value)
        {
            OnIncreaseAttackSpeedFeedback?.Invoke(value);
        }

        public void IncreaseAttackSpeedRPC(float value)
        {
            OnIncreaseAttackSpeed?.Invoke(value);
            photonView.RPC("SyncIncreaseAttackSpeedRPC", RpcTarget.All, value);
        }

        public event GlobalDelegates.FloatDelegate OnIncreaseAttackSpeed;
        public event GlobalDelegates.FloatDelegate OnIncreaseAttackSpeedFeedback;

        public void RequestDecreaseAttackSpeed(float value)
        {
            photonView.RPC("DecreaseAttackSpeedRPC", RpcTarget.MasterClient, value);
        }

        public void SyncDecreaseAttackSpeedRPC(float value)
        {
            OnDecreaseAttackSpeedFeedback?.Invoke(value);
        }

        public void DecreaseAttackSpeedRPC(float value)
        {
            OnDecreaseAttackSpeed?.Invoke(value);
            photonView.RPC("SyncDecreaseAttackSpeedRPC", RpcTarget.All, value);
        }

        public event GlobalDelegates.FloatDelegate OnDecreaseAttackSpeed;
        public event GlobalDelegates.FloatDelegate OnDecreaseAttackSpeedFeedback;

        #endregion
        
        #region Moveable

        public float GetReferenceMoveSpeed()
        {
            throw new System.NotImplementedException();
        }

        public float GetCurrentMoveSpeed()
        {
            throw new System.NotImplementedException();
        }

        public void RequestSetReferenceMoveSpeed(float value)
        {
            throw new System.NotImplementedException();
        }

        public void SyncSetReferenceMoveSpeedRPC(float value)
        {
            throw new System.NotImplementedException();
        }

        public void SetReferenceMoveSpeedRPC(float value)
        {
            throw new System.NotImplementedException();
        }

        public event GlobalDelegates.FloatDelegate OnSetReferenceMoveSpeed;
        public event GlobalDelegates.FloatDelegate OnSetReferenceMoveSpeedFeedback;

        public void RequestIncreaseReferenceMoveSpeed(float amount)
        {
            throw new System.NotImplementedException();
        }

        public void SyncIncreaseReferenceMoveSpeedRPC(float amount)
        {
            throw new System.NotImplementedException();
        }

        public void IncreaseReferenceMoveSpeedRPC(float amount)
        {
            throw new System.NotImplementedException();
        }

        public event GlobalDelegates.FloatDelegate OnIncreaseReferenceMoveSpeed;
        public event GlobalDelegates.FloatDelegate OnIncreaseReferenceMoveSpeedFeedback;

        public void RequestDecreaseReferenceMoveSpeed(float amount)
        {
            throw new System.NotImplementedException();
        }

        public void SyncDecreaseReferenceMoveSpeedRPC(float amount)
        {
            throw new System.NotImplementedException();
        }

        public void DecreaseReferenceMoveSpeedRPC(float amount)
        {
            throw new System.NotImplementedException();
        }

        public event GlobalDelegates.FloatDelegate OnDecreaseReferenceMoveSpeed;
        public event GlobalDelegates.FloatDelegate OnDecreaseReferenceMoveSpeedFeedback;

        public void RequestSetCurrentMoveSpeed(float value)
        {
            throw new System.NotImplementedException();
        }

        public void SyncSetCurrentMoveSpeedRPC(float value)
        {
            throw new System.NotImplementedException();
        }

        public void SetCurrentMoveSpeedRPC(float value)
        {
            throw new System.NotImplementedException();
        }

        public event GlobalDelegates.FloatDelegate OnSetCurrentMoveSpeed;
        public event GlobalDelegates.FloatDelegate OnSetCurrentMoveSpeedFeedback;

        public void RequestIncreaseCurrentMoveSpeed(float amount)
        {
            throw new System.NotImplementedException();
        }

        public void SyncIncreaseCurrentMoveSpeedRPC(float amount)
        {
            throw new System.NotImplementedException();
        }

        public void IncreaseCurrentMoveSpeedRPC(float amount)
        {
            throw new System.NotImplementedException();
        }

        public event GlobalDelegates.FloatDelegate OnIncreaseCurrentMoveSpeed;
        public event GlobalDelegates.FloatDelegate OnIncreaseCurrentMoveSpeedFeedback;

        public void RequestDecreaseCurrentMoveSpeed(float amount)
        {
            throw new System.NotImplementedException();
        }

        public void SyncDecreaseCurrentMoveSpeedRPC(float amount)
        {
            throw new System.NotImplementedException();
        }

        public void DecreaseCurrentMoveSpeedRPC(float amount)
        {
            throw new System.NotImplementedException();
        }

        public event GlobalDelegates.FloatDelegate OnDecreaseCurrentMoveSpeed;
        public event GlobalDelegates.FloatDelegate OnDecreaseCurrentMoveSpeedFeedback;
        
        #endregion

        #region ActiveLifeable

        [SerializeField] private bool attackAffected = true;
        [SerializeField] private bool abilitiesAffected = true;
        [SerializeField] private float maxHp;
        private float currentHp;

        public float GetMaxHp()
        {
            return maxHp;
        }

        public float GetCurrentHp()
        {
            return currentHp;
        }

        public bool AttackAffected()
        {
            return attackAffected;
        }

        public bool AbilitiesAffected()
        {
            return abilitiesAffected;
        }

        public void RequestSetMaxHp(float value)
        {
            photonView.RPC("SetMaxHpRPC", RpcTarget.MasterClient, value);
        }

        [PunRPC]
        public void SyncSetMaxHpRPC(float value)
        {
            maxHp = value;
            currentHp = value;
            OnSetMaxHpFeedback?.Invoke(value);
        }

        [PunRPC]
        public void SetMaxHpRPC(float value)
        {
            maxHp = value;
            currentHp = value;
            OnSetMaxHp?.Invoke(value);
            photonView.RPC("SyncSetMaxHpRPC", RpcTarget.All, maxHp);
        }

        public event GlobalDelegates.FloatDelegate OnSetMaxHp;
        public event GlobalDelegates.FloatDelegate OnSetMaxHpFeedback;

        public void RequestIncreaseMaxHp(float amount)
        {
            photonView.RPC("IncreaseMaxHpRPC", RpcTarget.MasterClient, amount);
        }

        [PunRPC]
        public void SyncIncreaseMaxHpRPC(float amount)
        {
            maxHp = amount;
            currentHp = amount;
            OnIncreaseMaxHpFeedback?.Invoke(amount);
        }

        [PunRPC]
        public void IncreaseMaxHpRPC(float amount)
        {
            maxHp += amount;
            currentHp = amount;
            if (maxHp < currentHp) currentHp = maxHp;
            OnIncreaseMaxHp?.Invoke(amount);
            photonView.RPC("SyncIncreaseMaxHpRPC", RpcTarget.All, maxHp);
        }

        public event GlobalDelegates.FloatDelegate OnIncreaseMaxHp;
        public event GlobalDelegates.FloatDelegate OnIncreaseMaxHpFeedback;

        public void RequestDecreaseMaxHp(float amount)
        {
            photonView.RPC("DecreaseMaxHpRPC", RpcTarget.MasterClient, amount);
        }

        [PunRPC]
        public void SyncDecreaseMaxHpRPC(float amount)
        {
            maxHp = amount;
            if (maxHp < currentHp) currentHp = maxHp;
            OnDecreaseMaxHpFeedback?.Invoke(amount);
        }

        [PunRPC]
        public void DecreaseMaxHpRPC(float amount)
        {
            maxHp -= amount;
            if (maxHp < currentHp) currentHp = maxHp;
            OnDecreaseMaxHp?.Invoke(amount);
            photonView.RPC("SyncDecreaseMaxHpRPC", RpcTarget.All, maxHp);
        }

        public event GlobalDelegates.FloatDelegate OnDecreaseMaxHp;
        public event GlobalDelegates.FloatDelegate OnDecreaseMaxHpFeedback;

        public void RequestSetCurrentHp(float value)
        {
            photonView.RPC("SetCurrentHpRPC", RpcTarget.MasterClient, value);
        }

        [PunRPC]
        public void SyncSetCurrentHpRPC(float value)
        {
            currentHp = value;
            OnSetCurrentHpFeedback?.Invoke(value);
        }

        [PunRPC]
        public void SetCurrentHpRPC(float value)
        {
            currentHp = value;
            OnSetCurrentHp?.Invoke(value);
            photonView.RPC("SyncSetCurrentHpRPC", RpcTarget.All, value);
        }

        public event GlobalDelegates.FloatDelegate OnSetCurrentHp;
        public event GlobalDelegates.FloatDelegate OnSetCurrentHpFeedback;

        public void RequestIncreaseCurrentHp(float amount)
        {
            photonView.RPC("IncreaseCurrentHpRPC", RpcTarget.MasterClient, amount);
        }

        [PunRPC]
        public void SyncIncreaseCurrentHpRPC(float amount)
        {
            currentHp = amount;
            OnIncreaseCurrentHpFeedback?.Invoke(amount);
        }

        [PunRPC]
        public void IncreaseCurrentHpRPC(float amount)
        {
            currentHp += amount;
            if (currentHp > maxHp) currentHp = maxHp;
            OnIncreaseCurrentHp?.Invoke(amount);
            photonView.RPC("SyncIncreaseCurrentHpRPC", RpcTarget.All, currentHp);
        }

        public event GlobalDelegates.FloatDelegate OnIncreaseCurrentHp;
        public event GlobalDelegates.FloatDelegate OnIncreaseCurrentHpFeedback;

        public void RequestDecreaseCurrentHp(float amount)
        {
            photonView.RPC("DecreaseCurrentHpRPC", RpcTarget.MasterClient, amount);
        }

        [PunRPC]
        public void SyncDecreaseCurrentHpRPC(float amount)
        {
            currentHp = amount;
            if (currentHp <= 0)
            {
                currentHp = 0;
                RequestDie();
            }
            OnDecreaseCurrentHpFeedback?.Invoke(amount);
        }

        [PunRPC]
        public void DecreaseCurrentHpRPC(float amount)
        {
            currentHp -= amount;
            OnDecreaseCurrentHp?.Invoke(amount);
            photonView.RPC("SyncDecreaseCurrentHpRPC", RpcTarget.All, currentHp);
        }

        public event GlobalDelegates.FloatDelegate OnDecreaseCurrentHp;
        public event GlobalDelegates.FloatDelegate OnDecreaseCurrentHpFeedback;
        
        #endregion
        
        #region Deadable

        public bool isAlive;
        public bool canDie; 
        
        public bool IsAlive()
        {
            return isAlive;
        }

        public bool CanDie()
        {
            return canDie;
        }

        public void RequestSetCanDie(bool value)
        {
            photonView.RPC("SetCanDieRPC", RpcTarget.MasterClient, value);
        }

        public void SyncSetCanDieRPC(bool value)
        {
            canDie = value;
            OnSetCanDieFeedback?.Invoke(value);
        }

        public void SetCanDieRPC(bool value)
        {
            canDie = value;
            OnSetCanDie?.Invoke(value);
            photonView.RPC("SyncSetCanDieRPC", RpcTarget.All, value);
        }

        public event GlobalDelegates.BoolDelegate OnSetCanDie;
        public event GlobalDelegates.BoolDelegate OnSetCanDieFeedback;

        public void RequestDie()
        {
            photonView.RPC("DieRPC", RpcTarget.MasterClient);
        }

        [PunRPC]
        public void SyncDieRPC()
        {
            FogOfWarManager.Instance.RemoveFOWViewable(this);
            gameObject.SetActive(false);
        }

        [PunRPC]
        public void DieRPC()
        {
            photonView.RPC("SyncDieRPC", RpcTarget.All);
        }

        public event GlobalDelegates.NoParameterDelegate OnDie;
        public event GlobalDelegates.NoParameterDelegate OnDieFeedback;

        public void RequestRevive() { }

        [PunRPC]
        public void SyncReviveRPC() { }

        [PunRPC]
        public void ReviveRPC() { }

        public event GlobalDelegates.NoParameterDelegate OnRevive;
        public event GlobalDelegates.NoParameterDelegate OnReviveFeedback;
        
        #endregion
    }
}