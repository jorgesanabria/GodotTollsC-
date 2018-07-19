using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaquinaDeEstados.FSM
{
    public class FiniteStateMachine<TEnum, TInjectable>
    {
        protected TEnum _current;
        protected IDictionary<TEnum, IList<Func<TEnum, TInjectable, TEnum>>> _states;
        protected readonly Func<TEnum, TEnum, bool> _equalizer;
        public TEnum InitialState { set { _current = value; } }
        public FiniteStateMachine(Func<TEnum, TEnum, bool> equalizer)
        {
            _equalizer = equalizer;
            _states = new Dictionary<TEnum, IList<Func<TEnum, TInjectable, TEnum>>>();
        }
        public virtual void Add(TEnum state, Func<TEnum, TInjectable, TEnum> func)
        {
            _initializeState(state);
            _states[state].Add(func);
        }

        protected virtual void _initializeState(TEnum state)
        {
            if (!_states.ContainsKey(state)) _states[state] = new List<Func<TEnum, TInjectable, TEnum>>();
        }
        public virtual void Tick(TInjectable injectable)
        {
            foreach (var listener in _states[_current])
            {
                var captured = listener(_current, injectable);
                if (!_equalizer(_current, captured))
                {
                    _initializeState(captured);
                    _current = captured;
                    break;
                }
            }
        }
    }
}
