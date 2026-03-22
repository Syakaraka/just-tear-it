using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 结算场景管理器
/// 显示关卡结算界面，包括星级评价、金币奖励等
/// </summary>
public class ResultSceneManager : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Image[] starImages;               // 星级图片（3个）
    [SerializeField] private Text levelNameText;               // 关卡名称
    [SerializeField] private Text coinRewardText;             // 金币奖励文字
    [SerializeField] private Text tearPercentText;            // 撕裂比例文字

    [SerializeField] private Button nextLevelButton;           // 下一关按钮
    [SerializeField] private Button retryButton;               // 重试按钮
    [SerializeField] private Button homeButton;                // 返回主页按钮
    [SerializeField] private Button adRewardButton;            // 看广告奖励按钮

    [Header("场景配置")]
    [SerializeField] private string levelSelectScene = "LevelSelectScene";
    [SerializeField] private string gameScene = "GameScene";

    [Header("动画配置")]
    [SerializeField] private float starShowDelay = 0.3f;      // 星级展示延迟
    [SerializeField] private float starAnimationDuration = 0.5f;

    private int currentLevelId;
    private int stars;
    private int coinReward;
    private float tearPercent;
    private bool hasNextLevel;

    private void Start()
    {
        // 获取游戏结果数据
        LoadResultData();

        // 初始化UI
        InitUI();

        // 播放结果动画
        PlayResultAnimation();
    }

    /// <summary>
    /// 加载结果数据
    /// </summary>
    private void LoadResultData()
    {
        currentLevelId = GameManager.Instance.CurrentLevel;

        // 从GameManager获取结果
        // 这里应该从上一个场景传递数据，暂时使用GameManager的数据
        stars = GameManager.Instance.GetLevelStars(currentLevelId);
        tearPercent = 1f; // TODO: 从游戏数据获取

        // 计算奖励
        coinReward = CalculateReward(stars);

        // 检查是否有下一关
        hasNextLevel = currentLevelId < GameManager.Instance.TotalLevels;
    }

    /// <summary>
    /// 计算奖励
    /// </summary>
    private int CalculateReward(int stars)
    {
        int baseReward = 10;
        return stars * baseReward;
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitUI()
    {
        // 设置关卡名称
        if (levelNameText != null)
        {
            levelNameText.text = $"Level {currentLevelId}";
        }

        // 设置撕裂比例
        if (tearPercentText != null)
        {
            tearPercentText.text = $"撕裂比例: {Mathf.RoundToInt(tearPercent * 100)}%";
        }

        // 设置金币奖励
        if (coinRewardText != null)
        {
            coinRewardText.text = $"+{coinReward}";
        }

        // 绑定按钮事件
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            nextLevelButton.gameObject.SetActive(hasNextLevel);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(OnHomeClicked);
        }

        if (adRewardButton != null)
        {
            adRewardButton.onClick.AddListener(OnAdRewardClicked);
        }

        // 初始隐藏所有星级
        foreach (var star in starImages)
        {
            if (star != null)
            {
                star.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 播放结果动画
    /// </summary>
    private void PlayResultAnimation()
    {
        // 延迟显示星级
        for (int i = 0; i < stars && i < starImages.Length; i++)
        {
            StartCoroutine(ShowStarCoroutine(i, starShowDelay * (i + 1)));
        }

        // 添加金币动画
        // TODO: 后续实现金币飞入动画
    }

    /// <summary>
    /// 显示星星协程
    /// </summary>
    private System.Collections.IEnumerator ShowStarCoroutine(int starIndex, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (starImages[starIndex] != null)
        {
            starImages[starIndex].gameObject.SetActive(true);
            // TODO: 播放星星出现的缩放动画
        }
    }

    /// <summary>
    /// 下一关按钮点击
    /// </summary>
    private void OnNextLevelClicked()
    {
        int nextLevel = currentLevelId + 1;
        GameManager.Instance.StartLevel(nextLevel);
        SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Single);
    }

    /// <summary>
    /// 重试按钮点击
    /// </summary>
    private void OnRetryClicked()
    {
        GameManager.Instance.StartLevel(currentLevelId);
        SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Single);
    }

    /// <summary>
    /// 返回主页按钮点击
    /// </summary>
    private void OnHomeClicked()
    {
        GameManager.Instance.SetGameState(GameState.MainMenu);
        SceneManager.LoadSceneAsync(levelSelectScene, LoadSceneMode.Single);
    }

    /// <summary>
    /// 看广告奖励按钮点击
    /// </summary>
    private void OnAdRewardClicked()
    {
        // 调用广告接口
        AdInterface.Instance.ShowRewardedAd(AdInterface.RewardType.Coins, 50, (success) =>
        {
            if (success)
            {
                Debug.Log("Ad reward granted!");
                // 更新UI显示
                if (coinRewardText != null)
                {
                    coinRewardText.text = $"+{coinReward + 50}";
                }
                // 禁用按钮
                adRewardButton.interactable = false;
            }
        });
    }
}
