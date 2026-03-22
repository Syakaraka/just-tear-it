using UnityEngine;
using System;

/// <summary>
/// 道具数据
/// </summary>
[Serializable]
public class ItemData
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    public ItemType type;
    public int price;
    public int quantity;
    public string description;
}

/// <summary>
/// 道具类型
/// </summary>
public enum ItemType
{
    PowerBoost,     // 力量提升：加速撕裂
    Hint,           // 提示：显示撕裂路径
    Shield,         // 护盾：失败保护
    CoinBonus,      // 金币加成
    TimeBonus       // 时间加成
}

/// <summary>
/// 道具系统管理器
/// </summary>
public class ItemSystem : MonoBehaviour
{
    private static ItemSystem instance;
    public static ItemSystem Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("ItemSystem");
                var newInstance = go.AddComponent<ItemSystem>();
                DontDestroyOnLoad(go);
                instance = newInstance;
            }
            return instance;
        }
    }

    [Header("道具配置")]
    [SerializeField] private ItemData[] availableItems;

    private ItemData[] ownedItems;
    private const string ITEM_DATA_KEY = "OwnedItems";

    public event Action<ItemData> OnItemUsed;
    public event Action<ItemData> OnItemAcquired;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        LoadOwnedItems();
    }

    /// <summary>
    /// 加载拥有的道具
    /// </summary>
    private void LoadOwnedItems()
    {
        string data = PlayerPrefs.GetString(ITEM_DATA_KEY, "");
        if (string.IsNullOrEmpty(data))
        {
            ownedItems = new ItemData[0];
        }
        else
        {
            // TODO: JSON反序列化
            ownedItems = new ItemData[0];
        }
    }

    /// <summary>
    /// 保存拥有的道具
    /// </summary>
    private void SaveOwnedItems()
    {
        // TODO: JSON序列化
        PlayerPrefs.SetString(ITEM_DATA_KEY, "");
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 获取道具数量
    /// </summary>
    public int GetItemCount(string itemId)
    {
        foreach (var item in ownedItems)
        {
            if (item.itemId == itemId)
                return item.quantity;
        }
        return 0;
    }

    /// <summary>
    /// 使用道具
    /// </summary>
    public bool UseItem(string itemId)
    {
        int count = GetItemCount(itemId);
        if (count <= 0) return false;

        // 减少数量
        foreach (var item in ownedItems)
        {
            if (item.itemId == itemId)
            {
                item.quantity--;
                OnItemUsed?.Invoke(item);
                SaveOwnedItems();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获得道具
    /// </summary>
    public void AcquireItem(string itemId, int quantity = 1)
    {
        // 查找是否已拥有
        bool found = false;
        foreach (var item in ownedItems)
        {
            if (item.itemId == itemId)
            {
                item.quantity += quantity;
                found = true;
                OnItemAcquired?.Invoke(item);
                break;
            }
        }

        // 如果没有，添加新道具
        if (!found)
        {
            // 查找道具配置
            foreach (var config in availableItems)
            {
                if (config.itemId == itemId)
                {
                    var newItem = new ItemData
                    {
                        itemId = config.itemId,
                        itemName = config.itemName,
                        icon = config.icon,
                        type = config.type,
                        price = config.price,
                        quantity = quantity,
                        description = config.description
                    };

                    // 扩展数组
                    Array.Resize(ref ownedItems, ownedItems.Length + 1);
                    ownedItems[ownedItems.Length - 1] = newItem;

                    OnItemAcquired?.Invoke(newItem);
                    break;
                }
            }
        }

        SaveOwnedItems();
    }

    /// <summary>
    /// 购买道具
    /// </summary>
    public bool PurchaseItem(string itemId)
    {
        // 查找道具配置
        ItemData config = null;
        foreach (var item in availableItems)
        {
            if (item.itemId == itemId)
            {
                config = item;
                break;
            }
        }

        if (config == null) return false;

        // 检查金币是否足够
        if (!GameManager.Instance.SpendCoins(config.price))
        {
            return false;
        }

        // 购买成功
        AcquireItem(itemId, 1);
        return true;
    }

    /// <summary>
    /// 获取所有可用道具
    /// </summary>
    public ItemData[] GetAvailableItems()
    {
        return availableItems;
    }

    /// <summary>
    /// 获取所有拥有的道具
    /// </summary>
    public ItemData[] GetOwnedItems()
    {
        return ownedItems;
    }

    /// <summary>
    /// 检查是否有可用的道具类型
    /// </summary>
    public bool HasItemOfType(ItemType type)
    {
        foreach (var item in ownedItems)
        {
            if (item.type == type && item.quantity > 0)
                return true;
        }
        return false;
    }
}
