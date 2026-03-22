using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 开始按钮处理脚本
/// </summary>
public class StartButtonHandler : MonoBehaviour
{
    [Header("目标场景")]
    [SerializeField] private string targetSceneName = "GameScene";

    [Header("按钮配置")]
    [SerializeField] private Button startButton;

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
        }
    }

    private void OnStartClicked()
    {
        Debug.Log("开始按钮点击，加载场景: " + targetSceneName);
        SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartClicked);
        }
    }
}
