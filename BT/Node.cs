using System;
using System.Collections.Generic;

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
    public class Sequence<TContext> : Node<TContext> where TContext : class
    {
        protected IList<Node<TContext>> _children;
        protected int _currentIndex;
        public Sequence(params Node<TContext>[] nodes)
        {
            _children = nodes;
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
        public Selector(params Node<TContext>[] nodes)
        {
            _children = nodes;
        }
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
        protected float _acumulated;
        protected DateTime? _startTime;
        public Wait(float secons)
        {
            _waitTime = secons;
        }
        public override Status Tick(TContext context)
        {
            if (!_startTime.HasValue) _startTime = DateTime.Now.AddSeconds(_waitTime);
            if (DateTime.Now >= _startTime.Value)
            {
                _startTime = null;
                return Status.Success;
            }
            return Status.Prossess;
        }
    }
    public class Call<TContext> : Node<TContext> where TContext : class
    {
        protected Action<TContext> _callable;
        public Call(Action<TContext> callable)
        {
            _callable = callable;
        }
        public override Status Tick(TContext context)
        {
            _callable(context);
            return Status.Success;
        }
    }
    public class LeafFunction<TContext> : Node<TContext> where TContext : class
    {
        protected Func<TContext, Status> _function;
        public LeafFunction(Func<TContext, Status> function)
        {
            _function = function;
        }
        public override Status Tick(TContext context)
        {
            return _function(context);
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
        public Root(params Node<TContext>[] nodes)
        {
            _children = nodes;
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
                    _currentIndex = _currentIndex < _children.Count ? _currentIndex + 1 : 0;
                    return Status.Success;                
            }
            return Status.Prossess;
        }
    }
    public static class BtExtension
    {
        public static Node<TContext> Sequense<TContext>(this Root<TContext> root, params Node<TContext>[] nodes) where TContext : class
        {
            return new Sequence<TContext>(nodes);
        }
        public static Node<TContext> Select<TContext>(this Root<TContext> root, params Node<TContext>[] nodes) where TContext : class
        {
            return new Sequence<TContext>(nodes);
        }
        public static Node<TContext> Invert<TContext>(this Root<TContext> root, Node<TContext> node) where TContext : class
        {
            return new Invert<TContext>(node);
        }
        public static Node<TContext> Wait<TContext>(this Root<TContext> root, float seconds) where TContext : class
        {
            return new Wait<TContext>(seconds);
        }
        public static Node<TContext> Call<TContext>(this Root<TContext> root, Action<TContext> callable) where TContext : class
        {
            return new Call<TContext>(callable);
        }
        public static Node<TContext> Function<TContext>(this Root<TContext> root, Func<TContext, Node<TContext>.Status> function) where TContext : class
        {
            return new LeafFunction<TContext>(function);
        }
        public static Node<TContext> Condition<TContext>(this Root<TContext> root, Func<TContext, bool> condition) where TContext : class
        {
            return new Condition<TContext>(condition);
        }
        public static Node<TContext> Sub<TContext>(this Root<TContext> root, Node<TContext> node) where TContext : class
        {
            return node;
        }
    }
    public static class Bt
    {
        public static Root<TContext> Root<TContext>(params Node<TContext>[] nodes) where TContext : class
        {
            return new Root<TContext>(nodes);
        }
    }
}
