using UnityEngine;
using System;

/// <summary>
/// 广告接口（当前版本：跳过广告直接给奖励）
/// 预留广告接入接口
/// </summary>
public class AdInterface : MonoBehaviour
{
    private static AdInterface instance;
    public static AdInterface Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("AdInterface");
                var newInstance = go.AddComponent<AdInterface>();
                DontDestroyOnLoad(go);
                instance = newInstance;
            }
            return instance;
        }
    }

    // 奖励类型
    public enum RewardType
    {
        Coins,
        Item,
        Energy
    }

    // 回调定义
    public event Action<RewardType, int> OnRewardEarned;
    public event Action OnAdSkipped;
    public event Action OnAdFailed;

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
        }
    }

    /// <summary>
    /// 显示奖励广告
    /// 当前实现：跳过广告直接给奖励
    /// </summary>
    /// <param name="rewardType">奖励类型</param>
    /// <param name="amount">奖励数量</param>
    /// <param name="callback">回调</param>
    public void ShowRewardedAd(RewardType rewardType, int amount, Action<bool> callback = null)
    {
        Debug.Log($"[AdInterface] Requesting rewarded ad - Type: {rewardType}, Amount: {amount}");

        // 当前版本：跳过广告直接给奖励
        // TODO: 后续接入真实广告SDK时替换此实现

        // 直接发放奖励
        GrantReward(rewardType, amount);

        // 触发奖励回调
        callback?.Invoke(true);
        OnRewardEarned?.Invoke(rewardType, amount);
    }

    /// <summary>
    /// 显示插屏广告
    /// 当前实现：跳过广告
    /// </summary>
    /// <param name="callback">回调</param>
    public void ShowInterstitialAd(Action<bool> callback = null)
    {
        Debug.Log("[AdInterface] Requesting interstitial ad - Skipping...");

        // 当前版本：跳过广告
        // TODO: 后续接入真实广告SDK时替换此实现

        callback?.Invoke(true);
        OnAdSkipped?.Invoke();
    }

    /// <summary>
    /// 发放奖励
    /// </summary>
    private void GrantReward(RewardType rewardType, int amount)
    {
        switch (rewardType)
        {
            case RewardType.Coins:
                GameManager.Instance?.AddCoins(amount);
                break;

            case RewardType.Item:
                // TODO: 道具系统接入
                Debug.Log($"[AdInterface] Granting item reward: {amount}");
                break;

            case RewardType.Energy:
                // TODO: 体力系统接入
                Debug.Log($"[AdInterface] Granting energy reward: {amount}");
                break;
        }
    }

    /// <summary>
    /// 检查广告是否可用
    /// </summary>
    public bool IsAdAvailable()
    {
        // 当前版本始终返回true（跳过模式）
        return true;
    }

    /// <summary>
    /// 预加载广告
    /// </summary>
    public void PreloadAd()
    {
        Debug.Log("[AdInterface] Preloading ads...");
        // TODO: 后续接入真实广告SDK时实现
    }

    /// <summary>
    /// 设置广告监听器（供外部扩展）
    /// </summary>
    public void SetAdListeners(
        Action<RewardType, int> onRewardEarned,
        Action onAdSkipped,
        Action onAdFailed)
    {
        if (onRewardEarned != null) OnRewardEarned += onRewardEarned;
        if (onAdSkipped != null) OnAdSkipped += onAdSkipped;
        if (onAdFailed != null) OnAdFailed += onAdFailed;
    }
}
