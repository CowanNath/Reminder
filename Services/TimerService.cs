namespace Reminder.Services;

/// <summary>
/// 单例定时器服务，减少内存占用
/// </summary>
public class TimerService : IDisposable
{
    private static TimerService? _instance;
    private static readonly object _lock = new();

    private readonly System.Windows.Forms.Timer _timer;
    public event EventHandler? Tick;

    private TimerService()
    {
        _timer = new System.Windows.Forms.Timer
        {
            Interval = 5000 // 改为5秒更新一次，减少CPU和内存占用
        };
        _timer.Tick += (s, e) => Tick?.Invoke(this, EventArgs.Empty);
    }

    public static TimerService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new TimerService();
                }
            }
            return _instance;
        }
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void Dispose()
    {
        // 单例不主动释放，由应用程序退出时清理
    }
}
