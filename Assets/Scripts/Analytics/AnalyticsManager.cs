using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 数据分析管理器
/// 负责游戏数据埋点和事件追踪
/// </summary>
public class AnalyticsManager : MonoBehaviour
{
    private static AnalyticsManager instance;
    public static AnalyticsManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("AnalyticsManager");
                var newInstance = go.AddComponent<AnalyticsManager>();
                DontDestroyOnLoad(go);
                instance = newInstance;
            }
            return instance;
        }
    }

    [Header("分析配置")]
    [SerializeField] private bool enableAnalytics = true;  // 是否启用分析
    [SerializeField] private float flushInterval = 30f;   // 数据刷新间隔（秒）

    private List<AnalyticsEvent> eventQueue = new List<AnalyticsEvent>();
    private float lastFlushTime = 0f;

    private const string ANALYTICS_ENABLED_KEY = "AnalyticsEnabled";

    /// <summary>
    /// 事件数据结构
    /// </summary>
    [Serializable]
    private class AnalyticsEvent
    {
        public string eventName;
        public Dictionary<string, object> parameters;
        public long timestamp;

        public AnalyticsEvent(string name, Dictionary<string, object> parameters)
        {
            eventName = name;
            this.parameters = parameters;
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

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

        // 检查用户是否允许数据收集
        enableAnalytics = PlayerPrefs.GetInt(ANALYTICS_ENABLED_KEY, 1) == 1;
    }

    private void Update()
    {
        // 定期刷新数据
        if (Time.time - lastFlushTime > flushInterval)
        {
            FlushEvents();
        }
    }

    /// <summary>
    /// 设置分析是否启用
    /// </summary>
    public void SetAnalyticsEnabled(bool enabled)
    {
        enableAnalytics = enabled;
        PlayerPrefs.SetInt(ANALYTICS_ENABLED_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 记录事件
    /// </summary>
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (!enableAnalytics) return;

        var analyticsEvent = new AnalyticsEvent(eventName, parameters ?? new Dictionary<string, object>());
        eventQueue.Add(analyticsEvent);

        Debug.Log($"[Analytics] Event: {eventName}, Params: {ParametersToString(parameters)}");
    }

    /// <summary>
    /// 记录关卡开始
    /// </summary>
    public void LogLevelStart(int levelId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "level_id", levelId },
            { "chapter", (levelId - 1) / 5 + 1 }
        };

        LogEvent("level_start", parameters);
    }

    /// <summary>
    /// 记录关卡完成
    /// </summary>
    public void LogLevelComplete(int levelId, int stars, float tearPercent)
    {
        var parameters = new Dictionary<string, object>
        {
            { "level_id", levelId },
            { "stars", stars },
            { "tear_percent", tearPercent },
            { "chapter", (levelId - 1) / 5 + 1 }
        };

        LogEvent("level_complete", parameters);
    }

    /// <summary>
    /// 记录关卡失败
    /// </summary>
    public void LogLevelFail(int levelId, float tearPercent)
    {
        var parameters = new Dictionary<string, object>
        {
            { "level_id", levelId },
            { "tear_percent", tearPercent }
        };

        LogEvent("level_fail", parameters);
    }

    /// <summary>
    /// 记录引导步骤
    /// </summary>
    public void LogTutorialStep(string stepId, bool completed)
    {
        var parameters = new Dictionary<string, object>
        {
            { "step_id", stepId },
            { "completed", completed }
        };

        LogEvent("tutorial_step", parameters);
    }

    /// <summary>
    /// 记录道具体使用
    /// </summary>
    public void LogItemUsed(string itemId, int levelId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "item_id", itemId },
            { "level_id", levelId }
        };

        LogEvent("item_used", parameters);
    }

    /// <summary>
    /// 记录广告观看
    /// </summary>
    public void LogAdWatch(string adType, bool completed)
    {
        var parameters = new Dictionary<string, object>
        {
            { "ad_type", adType },
            { "completed", completed }
        };

        LogEvent("ad_watch", parameters);
    }

    /// <summary>
    /// 记录金币变化
    /// </summary>
    public void LogCoinChange(int amount, string reason)
    {
        var parameters = new Dictionary<string, object>
        {
            { "amount", amount },
            { "reason", reason }
        };

        LogEvent("coin_change", parameters);
    }

    /// <summary>
    /// 刷新事件队列
    /// </summary>
    private void FlushEvents()
    {
        if (eventQueue.Count == 0) return;

        lastFlushTime = Time.time;

        // TODO: 实际发送到分析服务器
        // 这里只是模拟发送
        Debug.Log($"[Analytics] Flushing {eventQueue.Count} events to server...");

        // 发送到服务器后清空队列
        // 示例：StartCoroutine(SendToServer(eventQueue));

        eventQueue.Clear();
    }

    /// <summary>
    /// 将参数字典转换为字符串
    /// </summary>
    private string ParametersToString(Dictionary<string, object> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return "{}";

        var parts = new List<string>();
        foreach (var kvp in parameters)
        {
            parts.Add($"{kvp.Key}={kvp.Value}");
        }

        return "{" + string.Join(", ", parts) + "}";
    }

    /// <summary>
    /// 在退出时刷新所有事件
    /// </summary>
    private void OnDestroy()
    {
        FlushEvents();
    }

    /// <summary>
    /// 在应用暂停时刷新事件
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            FlushEvents();
        }
    }
}
