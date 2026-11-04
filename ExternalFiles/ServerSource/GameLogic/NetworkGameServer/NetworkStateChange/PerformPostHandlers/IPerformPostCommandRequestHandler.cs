// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using System;

namespace EternalWar.Server.Application.NetworkStateChange.PerformPostHandlers
{
    internal interface IPerformPostCommandRequestHandler
    {
        Type RequestType { get; }
        void PerformPostRequest(NetworkGameCommandRequest request);
    }

    internal abstract class PerformPostCommandRequestHandler<T> : IPerformPostCommandRequestHandler where T : NetworkGameCommandRequest
    {
        public Type RequestType => typeof(T);

        public void PerformPostRequest(NetworkGameCommandRequest request)
        {
            if (request is not T typedRequest)
                throw new InvalidOperationException($"{GetType()} for {RequestType} cannot handle a request of {request}");
            PerformPostRequest(typedRequest);
        }

        protected abstract void PerformPostRequest(T request);
    }
}
