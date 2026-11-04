// Original Authors - Jack Dekko
using EternalWar.Client.Application.Run.Map.Views.Actions;
using EternalWar.Engine.Godot.Run.Map.Nodes;
using Godot;
using GodotExtensions;
using System.Collections.Generic;

namespace EternalWar.Engine.Godot.Run.Map.View
{
    public sealed class GodotMapView(GodotGameVisualDatabase visualDatabase, GodotMapNodeCollection nodeCollection, ButtonFactory<int> mapNodeButtonsFactory) : IMapViewActions
    {
        private readonly GodotGameVisualDatabase _visualDatabase = visualDatabase;
        private readonly GodotMapNodeCollection _nodeCollection = nodeCollection;
        private readonly ButtonFactory<int> _mapNodeButtonsFactory = mapNodeButtonsFactory;
        private readonly Dictionary<int, Node> _nodeInstances = new();

        public void CreateNode(string nodeType, int nodeLayer, int nodeIndex)
        {
            if (_nodeInstances.ContainsKey(nodeIndex))
                return;

            Node rowsParent = _nodeCollection.NodeRowsParentNode;
            // Create or get the row node for organizing nodes by layer
            Node rowNode = GetOrCreateRowNode(rowsParent, nodeLayer);

            // Instantiate the node prefab
            // TODO: use the button factory instead.
            PackedScene nodePrefab = _visualDatabase.MapNodePrefab;
            Node nodeInstance = nodePrefab.Instantiate();

            // Store metadata for reference
            nodeInstance.Name = $"Node_{nodeIndex}";

            // Add to the row
            rowNode.AddChild(nodeInstance);
            _nodeInstances[nodeIndex] = nodeInstance;
        }

        public void DestroyAllNodes()
        {
            Node rowsParent = _nodeCollection.NodeRowsParentNode;
            // Remove all child rows
            foreach (Node child in rowsParent.GetChildren())
            {
                child.QueueFree();
            }

            _nodeInstances.Clear();
        }

        public void SetVote(int nodeIndex, int playerIndex)
        {
            if (!_nodeInstances.TryGetValue(nodeIndex, out Node? nodeInstance))
                return;
            // TODO?
        }

        public void Show()
        {
            Node rowsParent = _nodeCollection.NodeRowsParentNode;
            if (rowsParent is CanvasItem canvasItem)
            {
                canvasItem.Visible = true;
            }
        }

        public void Hide()
        {
            Node rowsParent = _nodeCollection.NodeRowsParentNode;
            if (rowsParent is CanvasItem canvasItem)
            {
                canvasItem.Visible = false;
            }
        }

        private Node GetOrCreateRowNode(Node rowsParent, int nodeLayer)
        {
            string rowName = $"MapNodeRow_{nodeLayer}";

            // Check if row already exists
            foreach (Node child in rowsParent.GetChildren())
            {
                if (child.Name == rowName)
                    return child;
            }

            // Create new row node
            PackedScene rowPrefab = _visualDatabase.MapNodeRowPrefab;
            Node rowNode = rowPrefab.Instantiate();
            rowNode.Name = rowName;
            rowsParent.AddChild(rowNode);
            return rowNode;
        }
    }
}
