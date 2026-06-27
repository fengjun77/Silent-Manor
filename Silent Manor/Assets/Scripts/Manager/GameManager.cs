using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // 暂停计数
    public bool IsGamePaused => _pauseCount > 0;
    private int _pauseCount = 0;

    // 新增：菜单是否打开全局标记
    public bool IsMenuOpen { get; set; }

    protected override void Awake()
    {
        base.Awake();
        IsMenuOpen = false;
    }

    public void AddPauseLayer()
    {
        _pauseCount++;
        Time.timeScale = 0f;
    }

    public void RemovePauseLayer()
    {
        _pauseCount--;
        if (_pauseCount <= 0)
        {
            _pauseCount = 0;
            Time.timeScale = 1f;
        }
    }
}
