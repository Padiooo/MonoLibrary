using System;

namespace MonoLibrary.Engine.Objects;

public class Chrono(float period, float delay = 0, bool reset = false, object state = null)
{
    private readonly object _state = state;
    private float _time = delay;
    private float _period = period;
    private bool _stop;

    public event Action<Chrono, object> OnTick;

    public void Update(float time)
    {
        if (_stop)
            return;

        _time -= time;

        if (_time < 0)
        {
            OnTick?.Invoke(this, _state);
            if (reset)
                _time += _period;
            else
                _stop = true;
        }
    }

    public void Change(float delay, float period)
    {
        _time = delay;
        _period = period;
        _stop = false;
    }
}
