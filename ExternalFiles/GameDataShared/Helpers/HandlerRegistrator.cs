// Original Authors - Wyatt Senalik
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Helpers
{
    public sealed class HandlerRegistrator<TKey, THandler> where TKey : Enum
    {
        private readonly Dictionary<TKey, THandler> _handler = new();
        private readonly Func<TKey, THandler> _handlerFactoryFunc;

        public HandlerRegistrator(Func<TKey, THandler> handlerFactoryFunc)
        {
            _handlerFactoryFunc = handlerFactoryFunc;
            foreach (TKey key in Enum.GetValues(typeof(TKey)))
            {
                THandler handler = _handlerFactoryFunc.Invoke(key);
                _handler.Add(key, handler);
            }
        }

        public THandler GetHandler(TKey key)
        {
            return _handler[key];
        }
    }
}
