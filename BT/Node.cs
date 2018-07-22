using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaquinaDeEstados.BT
{
    public abstract class Node<TContext> where TContext : class
    {
        public enum Status
        {
            Success = 1,
            Failure = 2,
            Prossess = 3
        }
        public abstract Status Tick(TContext context);
    }
    /// <summary>
    /// Usar este nodo si el estado requerido debe ser State.Success en todos los nodos de la secuencia
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class Sequense<TContext> : Node<TContext> where TContext : class
    {
        protected IList<Node<TContext>> _children;
        protected int _currentIndex;
        public Sequense()
        {
            _children = new List<Node<TContext>>();
        }
        public override Status Tick(TContext context)
        {
            var state = _children[_currentIndex % _children.Count].Tick(context);
            switch (state)
            {
                case Status.Failure:
                    _currentIndex = 0;
                    return Status.Failure;
                case Status.Success:
                    if (_currentIndex < _children.Count)
                    {
                        _currentIndex++;
                    }
                    else
                    {
                        _currentIndex = 0;
                        return Status.Success;
                    }
                    break;
            }
            return Status.Prossess;
        }
    }
    /// <summary>
    /// Usar este nodo si el estado requerido solo necesita ser Status.Succes en un unico nodo de la secuencia
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class Selector<TContext> : Node<TContext> where TContext : class
    {
        protected IList<Node<TContext>> _children;
        protected int _currentIndex;
        public override Status Tick(TContext context)
        {
            var state = _children[_currentIndex % _children.Count].Tick(context);
            switch (state)
            {
                case Status.Failure:
                    if (_currentIndex < _children.Count)
                    {
                        _currentIndex++;
                    }
                    else
                    {
                        _currentIndex = 0;
                        return Status.Failure;
                    }
                    break;
                case Status.Success:
                    _currentIndex = 0;
                    return Status.Success;
            }
            return Status.Failure;
        }
    }
    public class Invert<TContext> : Node<TContext> where TContext : class
    {
        protected Node<TContext> _invertible;
        public Invert(Node<TContext> invertible)
        {
            _invertible = invertible;
        }
        public override Status Tick(TContext context)
        {
            var state = _invertible.Tick(context);
            switch (state)
            {
                case Status.Success:
                    return Status.Failure;
                case Status.Failure:
                    return Status.Success;
            }
            return Status.Prossess;
        }
    }
    public class Wait<TContext> : Node<TContext> where TContext : class
    {
        protected float _waitTime;
        protected DateTime? _lastTime;
        public Wait(float secons)
        {
            _waitTime = secons;
        }
        public override Status Tick(TContext context)
        {
            if (!_lastTime.HasValue) _lastTime = DateTime.Now;
            if ((DateTime.Now - _lastTime.Value).Seconds >= _waitTime)
            {
                _lastTime = null;
                return Status.Success;
            }
            return Status.Prossess;
        }
    }
    public class Caller<TContext> : Node<TContext> where TContext : class
    {
        protected Action<TContext> _callable;
        public Caller(Action<TContext> callable)
        {
            _callable = callable;
        }
        public override Status Tick(TContext context)
        {
            _callable(context);
            return Status.Success;
        }
    }
    public class Condition<TContext> : Node<TContext> where TContext : class
    {
        protected Func<TContext, bool> _condition;
        public Condition(Func<TContext, bool> condition)
        {
            _condition = condition;
        }
        public override Status Tick(TContext context)
        {
            var result = _condition(context);
            return result ? Status.Success : Status.Failure;
        }
    }
    public class Root<TContext> : Node<TContext> where TContext : class
    {
        protected IList<Node<TContext>> _children;
        protected int _currentIndex;
        public Root()
        {
            _children = new List<Node<TContext>>();
        }
        public override Status Tick(TContext context)
        {
            var state = _children[_currentIndex % _children.Count].Tick(context);
            switch (state)
            {
                case Status.Failure:
                    _currentIndex = 0;
                    return Status.Failure;
                case Status.Success:
                    _currentIndex = _currentIndex < _children.Count ? _currentIndex++ : 0;
                    return Status.Success;                
            }
            return Status.Prossess;
        }
    }
}
