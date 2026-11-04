// Original Authors - Jack Dekko & Wyatt Senalik
using EternalWar.Core.Model.Map.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Core.Model.Map.Runtime
{
    public sealed class LocationNode(LocationNodeDefinition definition)
    {
        public LocationNodeDefinition Definition => _definition;
        public IReadOnlyCollection<LocationNode> Next => _nextNodes;
        public IReadOnlyCollection<LocationNode> Previous => _previousNodes;
        public int Id => _id;

        private readonly HashSet<LocationNode> _nextNodes = new();
        private readonly HashSet<LocationNode> _previousNodes = new();
        private readonly LocationNodeDefinition _definition = definition;
        private int _id; // internal id assigned by LocationMap when added to map

        public void SetId(int id)
        {
            _id = id;
        }

        public void AddNext(LocationNode node)
        {
            ArgumentNullException.ThrowIfNull(node, nameof(node));

            if (node == this)
                throw new ArgumentException($"Cannot add {nameof(node)} as the next node in {nameof(node)} because it would create a self-loop.", nameof(node));

            if (_nextNodes.Add(node))
                node._previousNodes.Add(this);
        }

        public void RemoveNext(LocationNode node)
        {
            ArgumentNullException.ThrowIfNull(node, nameof(node));

            if (_nextNodes.Remove(node))
                node._previousNodes.Remove(this);
        }

        public void AddPrevious(LocationNode node)
        {
            ArgumentNullException.ThrowIfNull(node, nameof(node));

            if (node == this)
                throw new ArgumentException($"Cannot add {nameof(node)} as the previous node in {nameof(node)} because it would create a self-loop.", nameof(node));

            if (_previousNodes.Add(node))
                node._nextNodes.Add(this);
        }

        public void RemovePrevious(LocationNode node)
        {
            ArgumentNullException.ThrowIfNull(node, nameof(node));

            if (_previousNodes.Remove(node))
                node._nextNodes.Remove(this);
        }

        public void ClearNext()
        {
            foreach (LocationNode next in _nextNodes.ToList())
            {
                RemoveNext(next);
            }
        }

        public void ClearPrevious()
        {
            foreach (LocationNode prev in _previousNodes.ToList())
            {
                RemovePrevious(prev);
            }
        }

        // Remove any connection between this and the provided node (both directions)
        public void RemoveConnection(LocationNode node)
        {
            if (node == null || node == this)
                return;

            RemoveNext(node);
            RemovePrevious(node);

            // make sure reciprocal links are removed as well
            node._nextNodes.Remove(this);
            node._previousNodes.Remove(this);
        }

        // Convenience: iterate all connected neighbors (both directions)
        public IEnumerable<LocationNode> GetConnections()
        {
            foreach (LocationNode next in _nextNodes)
            {
                yield return next;
            }

            foreach (LocationNode previous in _previousNodes)
            {
                if (!_nextNodes.Contains(previous))
                {
                    yield return previous;
                }
            }
        }
    }
}
