using System;
using System.Data;

public class MiscEvent
{
    public event Action<int> GoldValueChange;
    public void OnGoldValueChange(int gold)
    {
        GoldValueChange?.Invoke(gold);
    }

    public event Action<int> TimerValueChange;
    public void OnTimerChange(int time)
    {
        TimerValueChange?.Invoke(time);
    }

    public event Action<int> WaveStart;
    public void OnStartWave(int wave)
    {
        WaveStart?.Invoke(wave);
    }

}
