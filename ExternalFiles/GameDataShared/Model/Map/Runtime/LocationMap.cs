//Original Authors - Jack Dekko
using EternalWar.Core.Helpers;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Map.Runtime
{
    // Minimal port of old LocationMap logic into a pure C# MapInstance
    public sealed class LocationMap
    {
        public IReadOnlyDictionary<int, LocationNode> NodeMap => _locationNodeMap;

        private readonly Dictionary<int, LocationNode> _locationNodeMap = new();
        private readonly IdLibrary _idLibrary = new();
        private LocationNode? _head;
        private LocationNode? _tail;

        public int AddNode(LocationNode newNode)
        {
            ArgumentNullException.ThrowIfNull(newNode, nameof(newNode));

            int id = _idLibrary.CheckoutID();
            if (_locationNodeMap.ContainsKey(id))
                throw new InvalidOperationException($"Node with ID {id} already exists in the location map.");

            _locationNodeMap.Add(id, newNode);
            return id;
        }

        public void RemoveNode(int id)
        {
            if (!_locationNodeMap.TryGetValue(id, out LocationNode? nodeToRemove))
                return;

            if (_head == nodeToRemove)
                SetHead(null);
            if (_tail == nodeToRemove)
                SetTail(null);

            _locationNodeMap.Remove(id);
            _idLibrary.ReturnID(id);
        }

        public void SetHead(LocationNode? head) => _head = head;
        public void SetTail(LocationNode? tail) => _tail = tail;

        public IEnumerable<LocationNode> GetAllNodes() => _locationNodeMap.Values;

        public override string ToString() => $"Map(Nodes={_locationNodeMap.Count})";
    }
}
