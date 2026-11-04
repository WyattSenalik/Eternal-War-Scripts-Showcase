// Original Authors - Jack Dekko
using EternalWar.Core.Model.Map.Definitions;
using EternalWar.Core.Model.Map.Runtime;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Map.Simulation.Helpers
{
    public sealed partial class LocationMapBuilder(GameDatabase gameDatabase, LocationMapInstance locationMapInstance)
    {
        private readonly GameDatabase _gameDatabase = gameDatabase;
        private readonly LocationMapInstance _locationMapInstance = locationMapInstance;

        public LocationMap BuildLocationMap()
        {
            // Create a location map and cache of nodes by index
            LocationMap map = new();
            Dictionary<int, LocationNode> nodesByIndex = new();

            // First pass: Create all LocationNode instances from the database
            for (int i = 0; i < _locationMapInstance.Nodes.Count; i++)
            {
                LocationNodeDefinition definition = GetNodeDefinition(i);
                LocationNode node = new(definition);
                map.AddNode(node);
                nodesByIndex[i] = node;
            }

            // Second pass: Connect nodes based on ChildIndices
            for (int i = 0; i < _locationMapInstance.Nodes.Count; i++)
            {
                LocationNodeInstance nodeInstance = _locationMapInstance.Nodes[i];
                LocationNode currentNode = nodesByIndex[i];

                foreach (int childIndex in nodeInstance.ChildIndices)
                {
                    if (childIndex >= 0 && childIndex < nodesByIndex.Count)
                    {
                        LocationNode childNode = nodesByIndex[childIndex];
                        currentNode.AddNext(childNode);
                    }
                }
            }

            // Set the head node (first node in the map)
            if (_locationMapInstance.Nodes.Count > 0 && nodesByIndex.TryGetValue(0, out LocationNode? headNode))
            {
                map.SetHead(headNode);
            }

            return map;
        }

        private LocationNodeDefinition GetNodeDefinition(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= _locationMapInstance.Nodes.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(nodeIndex), $"Node index {nodeIndex} is out of range for map with {_locationMapInstance.Nodes.Count} nodes.");
            }

            LocationNodeInstance nodeInstance = _locationMapInstance.Nodes[nodeIndex];
            return _gameDatabase.GetLocationNodeDefinition(nodeInstance.NodeDefinitionId);
        }
    }
}
