using System;

namespace MonoLibrary.Engine.Objects
{
    public class Chrono
    {
        private readonly object _state;
        private float _time;
        private float _period;
        private bool _stop;

        private bool AutoReset { get; set; }

        public event Action<Chrono, object> OnTick;

        public Chrono(float period, float delay = 0, bool reset = false, object state = null)
        {
            _time = delay;
            _period = period;
            _state = state;
        }

        public void Update(float time)
        {
            if (_stop)
                return;

            _time -= time;

            if (_time < 0)
            {
                OnTick?.Invoke(this, _state);
                if (AutoReset)
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
}
