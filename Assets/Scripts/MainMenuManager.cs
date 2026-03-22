using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 主菜单管理器
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Text coinText;
    [SerializeField] private GameObject settingsPanel;

    [Header("场景配置")]
    [SerializeField] private string levelSelectSceneName = "LevelSelectScene";

    private void Start()
    {
        // 设置游戏状态
        GameManager.Instance.SetGameState(GameState.MainMenu);

        // 绑定按钮事件
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        // 更新金币显示
        UpdateCoinDisplay();

        // 监听金币变化
        GameManager.Instance.OnCoinsChanged += (total, delta) => UpdateCoinDisplay();
    }

    /// <summary>
    /// 开始游戏按钮点击
    /// </summary>
    private void OnPlayClicked()
    {
        // 加载关卡选择场景
        SceneManager.LoadSceneAsync(levelSelectSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 设置按钮点击
    /// </summary>
    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 更新金币显示
    /// </summary>
    private void UpdateCoinDisplay()
    {
        if (coinText != null)
        {
            coinText.text = GameManager.Instance.Coins.ToString();
        }
    }

    private void OnDestroy()
    {
        // 取消监听
        GameManager.Instance.OnCoinsChanged -= (total, delta) => UpdateCoinDisplay();
    }
}
