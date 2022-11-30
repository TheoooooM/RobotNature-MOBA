using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities.Capacities;
using Entities.Inventory;
using GameStates;
using Items;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Champion : Entity, IMovable, IInventoryable, IResourceable
{
    private Rigidbody rb;

    [SerializeReference] public List<Item> items = new List<Item>();

    public float maxResource;
    public float currentResource;
    
    private UI.InGame.UIManager uiManager;

    protected override void OnStart()
    {
        base.OnStart();
        rb = GetComponent<Rigidbody>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        Move();
    }

    public void ApplyChampionSO(byte championSoIndex, Enums.Team newTeam)
    {
        uiManager = UI.InGame.UIManager.Instance;
        var so = GameStateMachine.Instance.allChampionsSo[championSoIndex];
        // championSo = so;
        //maxHp = championSo.maxHp;
        //currentHp = maxHp;
        maxResource = so.maxRessource;
        currentResource = so.maxRessource;
        //viewRange = championSo.viewRange;
        //referenceMoveSpeed = championSo.referenceMoveSpeed;
        currentMoveSpeed = referenceMoveSpeed;
        //attackDamage = championSo.attackDamage;
        //attackAbilityIndex = championSo.attackAbilityIndex;
        //abilitiesIndexes = championSo.activeCapacitiesIndexes;
        //ultimateAbilityIndex = championSo.ultimateAbilityIndex;
        
        team = newTeam;

        Transform pos = transform;
        switch (team)
        {
            case Enums.Team.Team1:
            {
                for (int i = 0; i < MapLoaderManager.Instance.firstTeamBasePoint.Length; i++)
                {
                    if (MapLoaderManager.Instance.firstTeamBasePoint[i].champion == null)
                    {
                        pos = MapLoaderManager.Instance.firstTeamBasePoint[i].position;
                        MapLoaderManager.Instance.firstTeamBasePoint[i].champion = this;
                        break;
                    }
                }

                break;
            }
            case Enums.Team.Team2:
            {
                for (int i = 0; i < MapLoaderManager.Instance.secondTeamBasePoint.Length; i++)
                {
                    if (MapLoaderManager.Instance.secondTeamBasePoint[i].champion == null)
                    {
                        pos = MapLoaderManager.Instance.secondTeamBasePoint[i].position;
                        MapLoaderManager.Instance.secondTeamBasePoint[i].champion = this;
                        break;
                    }
                }

                break;
            }
            default:
                Debug.LogError("Team is not valid.");
                pos = transform;
                break;
        }

        if (uiManager != null)
        {
            // uiManager.InstantiateHealthBarForEntity(entityIndex);
            uiManager.InstantiateResourceBarForEntity(entityIndex);
        }
    }

    #region Mouvement

    [Header("=== MOUVEMENT")] private Vector3 lastDir;
    private bool isMoving;

    [SerializeField] float referenceMoveSpeed;
    float currentMoveSpeed = 3;

    public void SetMoveDirection(Vector3 dir)
    {
        lastDir = dir;
        isMoving = (dir != Vector3.zero);
    }

    void Move()
    {
        rb.velocity = lastDir * currentMoveSpeed;
    }

    #endregion


    #region Cast

    [Header("=== CAST")] public byte[] abilitiesIndexes = new byte[2];
    public byte ultimateAbilityIndex;

    public bool canCast;


    public void RequestCast(byte capacityIndex, int[] targetedEntities, Vector3[] targetedPositions)
    {
        photonView.RPC("CastRPC", RpcTarget.MasterClient, capacityIndex, targetedEntities, targetedPositions);
    }

    [PunRPC]
    public void CastRPC(byte capacityIndex, int[] targetedEntities, Vector3[] targetedPositions)
    {
        var activeCapacity = CapacitySOCollectionManager.CreateActiveCapacity(capacityIndex, this);

        if (!activeCapacity.TryCast(entityIndex, targetedEntities, targetedPositions)) return;

        OnCast?.Invoke(capacityIndex, targetedEntities, targetedPositions);
        photonView.RPC("SyncCastRPC", RpcTarget.All, capacityIndex, targetedEntities, targetedPositions);
    }

    [PunRPC]
    public void SyncCastRPC(byte capacityIndex, int[] targetedEntities, Vector3[] targetedPositions)
    {
        var activeCapacity = CapacitySOCollectionManager.CreateActiveCapacity(capacityIndex, this);
        activeCapacity.PlayFeedback(capacityIndex, targetedEntities, targetedPositions);
        OnCastFeedback?.Invoke(capacityIndex, targetedEntities, targetedPositions, activeCapacity);
    }

    public event GlobalDelegates.ByteIntArrayVector3ArrayDelegate OnCast;
    public event GlobalDelegates.ByteIntArrayVector3ArrayCapacityDelegate OnCastFeedback;

    #endregion

    #region Inventoryable

    public Item[] GetItems()
    {
        return items.ToArray();
    }

    public Item GetItem(int index)
    {
        if (index < 0 || index >= 3) return null;
        return items[index];
    }

    public Item GetItemOfSo(int soIndex)
    {
        return items.FirstOrDefault(item => item.indexOfSOInCollection == soIndex);
    }

    public void RequestAddItem(byte index)
    {
        photonView.RPC("AddItemRPC", RpcTarget.MasterClient, index);
    }

    [PunRPC]
    public void AddItemRPC(byte index)
    {
        var itemSo = ItemCollectionManager.Instance.GetItemSObyIndex(index);
        if (itemSo.consumable)
        {
            var contains = false;
            foreach (var item in items.Where(item => item.indexOfSOInCollection == index))
            {
                contains = true;
            }

            if (!contains && items.Count >= 3) return;
            photonView.RPC("SyncAddItemRPC", RpcTarget.All, index);
            return;
        }

        if (items.Count >= 3) return;
        photonView.RPC("SyncAddItemRPC", RpcTarget.All, index);
    }

    [PunRPC]
    public void SyncAddItemRPC(byte index)
    {
        var item = ItemCollectionManager.Instance.CreateItem(index, this);
        if (item == null) return;
        if (!items.Contains(item)) items.Add(item);
        if (PhotonNetwork.IsMasterClient)
        {
            item.OnItemAddedToInventory(this);
            OnAddItem?.Invoke(index);
        }

        item.OnItemAddedToInventoryFeedback(this);
        OnAddItemFeedback?.Invoke(index);
    }

    public event GlobalDelegates.ByteDelegate OnAddItem;
    public event GlobalDelegates.ByteDelegate OnAddItemFeedback;

    /// <param name="index">index of Item in this entity's inventory (not in item Collection)</param>
    public void RequestRemoveItem(byte index)
    {
        photonView.RPC("RemoveItemRPC", RpcTarget.MasterClient, index);
    }

    /// <param name="item">Item to remove from this entity's inventory</param>
    public void RequestRemoveItem(Item item)
    {
        if (!items.Contains(item)) return;
        RequestRemoveItem((byte)items.IndexOf(item));
    }

    [PunRPC]
    public void RemoveItemRPC(byte index)
    {
        photonView.RPC("SyncRemoveItemRPC", RpcTarget.All, index);
    }

    public void RemoveItemRPC(Item item)
    {
        if (!items.Contains(item)) return;
        var index = items.IndexOf(item);
        RemoveItemRPC((byte)index);
    }

    [PunRPC]
    public void SyncRemoveItemRPC(byte index)
    {
        if (index >= items.Count) return;
        var item = items[index];
        items.Remove(item);
        if (PhotonNetwork.IsMasterClient)
        {
            item.OnItemRemovedFromInventory(this);
            OnRemoveItem?.Invoke(index);
        }

        item.OnItemRemovedFromInventoryFeedback(this);
        OnRemoveItemFeedback?.Invoke(index);
    }

    public event GlobalDelegates.ByteDelegate OnRemoveItem;
    public event GlobalDelegates.ByteDelegate OnRemoveItemFeedback;

    public void RequestActivateItem(byte itemIndexInInventory, int[] selectedEntities, Vector3[] positions)
    {
        if (itemIndexInInventory >= items.Count) return;
        photonView.RPC("ActivateItemRPC", RpcTarget.MasterClient, itemIndexInInventory, selectedEntities, positions);
    }

    [PunRPC]
    public void ActivateItemRPC(byte itemIndexInInventory, int[] selectedEntities, Vector3[] positions)
    {
        if (itemIndexInInventory >= items.Count) return;
        var item = items[itemIndexInInventory];
        if (item == null) return;

        var successesActives = new bool[item.AssociatedItemSO().activeCapacitiesIndexes.Length];
        var bytes = item.AssociatedItemSO().activeCapacitiesIndexes;
        for (var i = 0; i < bytes.Length; i++)
        {
            var capacityIndex = bytes[i];
            var activeCapacity = CapacitySOCollectionManager.CreateActiveCapacity(capacityIndex, this);
            successesActives[i] = activeCapacity.TryCast(entityIndex, selectedEntities, positions);
        }

        items[itemIndexInInventory].OnItemActivated(selectedEntities, positions);
        OnActivateItem?.Invoke(itemIndexInInventory, selectedEntities, positions);
        photonView.RPC("SyncActivateItemRPC", RpcTarget.All, itemIndexInInventory, selectedEntities, positions,
            successesActives.ToArray());
    }

    [PunRPC]
    public void SyncActivateItemRPC(byte itemIndexInInventory, int[] selectedEntities, Vector3[] positions,
        bool[] castSuccess)
    {
        if (itemIndexInInventory >= items.Count) return;
        var item = items[itemIndexInInventory];
        if (items[itemIndexInInventory] == null) return;
        var bytes = item.AssociatedItemSO().activeCapacitiesIndexes;
        for (var index = 0; index < bytes.Length; index++)
        {
            var capacityIndex = bytes[index];
            var activeCapacity = CapacitySOCollectionManager.CreateActiveCapacity(capacityIndex, this);
            if (castSuccess[index]) activeCapacity.PlayFeedback(entityIndex, selectedEntities, positions);
        }

        items[itemIndexInInventory].OnItemActivatedFeedback(selectedEntities, positions);
        OnActivateItemFeedback?.Invoke(itemIndexInInventory, selectedEntities, positions, castSuccess);
    }

    public event GlobalDelegates.ByteIntArrayVector3ArrayDelegate OnActivateItem;
    public event GlobalDelegates.ByteIntArrayVector3ArrayBoolArrayDelegate OnActivateItemFeedback;

    #endregion

    #region Resourceable

    public float GetMaxResource()
    {
        return maxResource;
    }

    public float GetCurrentResource()
    {
        return currentResource;
    }

    public float GetCurrentResourcePercent()
    {
        return currentResource / maxResource * 100;
    }

    public void RequestSetMaxResource(float value)
    {
        photonView.RPC("SetMaxResourceRPC", RpcTarget.MasterClient, value);
    }

    [PunRPC]
    public void SyncSetMaxResourceRPC(float value)
    {
        maxResource = value;
        OnSetMaxResourceFeedback?.Invoke(value);
    }

    [PunRPC]
    public void SetMaxResourceRPC(float value)
    {
        maxResource = value;
        OnSetMaxResource?.Invoke(value);
        photonView.RPC("SyncSetMaxResourceRPC", RpcTarget.All, value);
    }

    public event GlobalDelegates.FloatDelegate OnSetMaxResource;
    public event GlobalDelegates.FloatDelegate OnSetMaxResourceFeedback;

    public void RequestIncreaseMaxResource(float amount)
    {
        photonView.RPC("IncreaseMaxResourceRPC", RpcTarget.MasterClient, amount);
    }

    [PunRPC]
    public void SyncIncreaseMaxResourceRPC(float amount)
    {
        maxResource = amount;
        OnIncreaseMaxResourceFeedback?.Invoke(amount);
    }

    [PunRPC]
    public void IncreaseMaxResourceRPC(float amount)
    {
        maxResource += amount;
        OnIncreaseMaxResource?.Invoke(amount);
        photonView.RPC("SyncIncreaseMaxResourceRPC", RpcTarget.All, amount);
    }

    public event GlobalDelegates.FloatDelegate OnIncreaseMaxResource;
    public event GlobalDelegates.FloatDelegate OnIncreaseMaxResourceFeedback;

    public void RequestDecreaseMaxResource(float amount)
    {
        photonView.RPC("DecreaseMaxResourceRPC", RpcTarget.MasterClient, amount);
    }

    [PunRPC]
    public void SyncDecreaseMaxResourceRPC(float amount)
    {
        maxResource = amount;
        OnDecreaseMaxResourceFeedback?.Invoke(amount);
    }

    [PunRPC]
    public void DecreaseMaxResourceRPC(float amount)
    {
        maxResource -= amount;
        OnDecreaseMaxResource?.Invoke(amount);
        photonView.RPC("SyncDecreaseMaxResourceRPC", RpcTarget.All, amount);
    }

    public event GlobalDelegates.FloatDelegate OnDecreaseMaxResource;
    public event GlobalDelegates.FloatDelegate OnDecreaseMaxResourceFeedback;

    public void RequestSetCurrentResource(float value)
    {
        photonView.RPC("SetCurrentResourceRPC", RpcTarget.MasterClient, value);
    }

    [PunRPC]
    public void SyncSetCurrentResourceRPC(float value)
    {
        currentResource = value;
        OnSetCurrentResourceFeedback?.Invoke(value);
    }

    [PunRPC]
    public void SetCurrentResourceRPC(float value)
    {
        currentResource = value;
        OnSetCurrentResource?.Invoke(value);
        photonView.RPC("SyncSetCurrentResourceRPC", RpcTarget.All, value);
    }

    public event GlobalDelegates.FloatDelegate OnSetCurrentResource;
    public event GlobalDelegates.FloatDelegate OnSetCurrentResourceFeedback;

    public void RequestIncreaseCurrentResource(float amount)
    {
        photonView.RPC("IncreaseCurrentResourceRPC", RpcTarget.MasterClient, amount);
    }

    [PunRPC]
    public void SyncIncreaseCurrentResourceRPC(float amount)
    {
        currentResource = amount;
        OnIncreaseCurrentResourceFeedback?.Invoke(amount);
    }

    [PunRPC]
    public void IncreaseCurrentResourceRPC(float amount)
    {
        currentResource += amount;
        OnIncreaseCurrentResource?.Invoke(amount);
        photonView.RPC("SyncIncreaseCurrentResourceRPC", RpcTarget.All, amount);
    }

    public event GlobalDelegates.FloatDelegate OnIncreaseCurrentResource;
    public event GlobalDelegates.FloatDelegate OnIncreaseCurrentResourceFeedback;

    public void RequestDecreaseCurrentResource(float amount)
    {
        photonView.RPC("DecreaseCurrentResourceRPC", RpcTarget.MasterClient, amount);
    }

    [PunRPC]
    public void SyncDecreaseCurrentResourceRPC(float amount)
    {
        currentResource = amount;
        OnDecreaseCurrentResourceFeedback?.Invoke(amount);
    }

    [PunRPC]
    public void DecreaseCurrentResourceRPC(float amount)
    {
        currentResource -= amount;
        OnDecreaseCurrentResource?.Invoke(amount);
        photonView.RPC("SyncDecreaseCurrentResourceRPC", RpcTarget.All, amount);
    }

    public event GlobalDelegates.FloatDelegate OnDecreaseCurrentResource;
    public event GlobalDelegates.FloatDelegate OnDecreaseCurrentResourceFeedback;

    #endregion
}