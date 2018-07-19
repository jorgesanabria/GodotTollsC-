using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaquinaDeEstados.FSM
{
    public class Fsm<TE> where TE : struct
    {
        protected TE _current;
        protected Dictionary<TE, List<Func<TE, float, TE>>> _states;
        public TE Initial { set { _current = value; } }
        public Fsm()
        {
            _states = new Dictionary<TE, List<Func<TE, float, TE>>>();
        }
        public virtual void Add(TE state, Func<TE, float, TE> func)
        {
            _createState(state);
            _states[state].Add(func);
        }

        private void _createState(TE state)
        {
            if (!_states.ContainsKey(state)) _states[state] = new List<Func<TE, float, TE>>();
        }

        public virtual void Tick(float delta)
        {
            foreach(var listener in _states[_current])
            {
                var captured = listener(_current, delta);
                if (!captured.Equals(_current))
                {
                    _createState(captured);
                    _current = captured;
                    break;
                }
            }
        }
    }
}
