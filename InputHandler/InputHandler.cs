using System;
using System.Collections.Generic;

namespace MaquinaDeEstados.InputHandler
{
    public class InputHandler<TEnum, TMap> : IInputHandler<TEnum>
    {
        protected readonly bool _multiple;
        protected IDictionary<TEnum, TMap> _map;
        protected IDictionary<TEnum, IList<TMap>> _multipleMap;
        protected const string EmptyPrefix = "";
        protected Func<TMap, bool> _actionPressed;
        protected Func<TMap, bool> _actionJustPressed;
        protected Func<TMap, bool> _actionReleased;
        public InputHandler(IDictionary<TEnum, TMap> map, Func<TMap, bool> actionPressed, Func<TMap, bool> actionJustPressed, Func<TMap, bool> actionReleased, string prefix = EmptyPrefix) : this(actionPressed, actionJustPressed, actionReleased)
        {
            _map = map;
        }
        protected InputHandler(Func<TMap, bool> actionPressed, Func<TMap, bool> actionJustPressed, Func<TMap, bool> actionReleased)
        {
            _actionPressed = actionPressed;
            _actionJustPressed = actionJustPressed;
            _actionReleased = actionReleased;
        }
        public InputHandler(IDictionary<TEnum, IList<TMap>> multipleMap, Func<TMap, bool> actionPressed, Func<TMap, bool> actionJustPressed, Func<TMap, bool> actionReleased, string prefix = EmptyPrefix) : this(actionPressed, actionJustPressed, actionReleased)
        {
            _multiple = true;
            _multipleMap = multipleMap;
        }
        public virtual bool IsActionPressed(TEnum action)
        {
            return _testAction(_actionPressed, action);
        }
        public virtual bool IsActionJustPressed(TEnum action)
        {
            return _testAction(_actionJustPressed, action);
        }
        public virtual bool IsActionReleased(TEnum action)
        {
            return _testAction(_actionReleased, action);
        }
        protected virtual bool _testAction(Func<TMap, bool> action, TEnum map)
        {
            if (!_multiple) return action(_map[map]);
            foreach (var mapedAction in _multipleMap[map])
            {
                if (action(mapedAction))
                    return true;
            }
            return false;
        }
    }
}
