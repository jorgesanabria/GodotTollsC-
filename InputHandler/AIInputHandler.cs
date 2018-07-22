using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaquinaDeEstados.InputHandler
{
    public class AIInputHandler<TEnum> : IAIInputHandler<TEnum>, IInputHandler<TEnum>
    {
        protected class ActionStatus
        {
            private bool _pressed;
            private bool _firtsPressed;
            private bool _enableReleased;
            public bool Pressed {
                get { return _pressed; }
                set
                {
                    _enableReleased = true;
                    if (_pressed && value)
                    {
                        _firtsPressed = false;
                    }
                    else if (!_pressed && value)
                    {
                        _pressed = true;
                        _firtsPressed = true;
                    }
                    else if (!value)
                    {
                        _firtsPressed = false;
                        _pressed = false;
                    }
                }
            }
            public bool FirtsPressed { get { return _firtsPressed; } }
            public bool Releaset { get { return _enableReleased && !_pressed; } }
        }
        protected IDictionary<TEnum, ActionStatus> _actions;
        public AIInputHandler()
        {
            _actions = new Dictionary<TEnum, ActionStatus>();
        }
        protected void _validateAction(TEnum action)
        {
            if (!_actions.ContainsKey(action))
            {
                _actions[action] = new ActionStatus();
            }
        }
        public virtual bool IsActionJustPressed(TEnum action)
        {
            _validateAction(action);
            return _actions[action].FirtsPressed;
        }

        public virtual bool IsActionPressed(TEnum action)
        {
            _validateAction(action);
            return _actions[action].Pressed;
        }

        public virtual bool IsActionReleased(TEnum action)
        {
            return _actions[action].Releaset;
        }

        public virtual void SetActionPressed(TEnum action)
        {
            _validateAction(action);
            _actions[action].Pressed = true;
        }

        public virtual void SetActionReleased(TEnum action)
        {
            _validateAction(action);
            _actions[action].Pressed = false;
        }
    }
}
