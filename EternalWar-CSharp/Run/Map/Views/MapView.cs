// Original Authors - Jack Dekko
using EternalWar.Client.Application.Run.Map.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Map.Runtime;
using EternalWar.Core.Network;
using System.Collections.Generic;

namespace EternalWar.Client.Application.Run.Map.Views
{
    internal sealed class MapView(IMapViewActions viewActions)
    {
        private readonly IMapViewActions _viewActions = viewActions;

        public void Activate(LocationMapInstance mapInstance)
        {
            for (int i = 0; i < mapInstance.Nodes.Count; i++)
            {
                LocationNodeInstance node = mapInstance.Nodes[i];
                int nodeLayer = CalculateNodeLayer(mapInstance, i);
                _viewActions.CreateNode(node.NodeDefinitionId, nodeLayer, i);
            }
        }

        public void Deactivate()
        {
            _viewActions.DestroyAllNodes();
        }

        public ChangeHandledResult HandleEvent(NetworkGameEvent_Run_Map mapEvent, ViewHandleChangeContext context)
        {
            return mapEvent switch
            {
                NetworkGameEvent_Run_Map_Created => HandleEvent_Created(),
                NetworkGameEvent_Run_Map_Show => HandleEvent_Show(),
                NetworkGameEvent_Run_Map_Hide => HandleEvent_Hide(),
                NetworkGameEvent_Run_Map_VotingChanged votingChangedEvent => HandleEvent_Votes(votingChangedEvent),
                _ => ChangeHandledResult_Finished.Instance
            };
        }

        private ChangeHandledResult_Finished HandleEvent_Created()
        {
            // TODO: Make this actually contain the map data that needs to be given to the view actions to display. Then loop over them and call create node.
            _viewActions.Show();
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleEvent_Show()
        {
            _viewActions.Show();
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleEvent_Hide()
        {
            _viewActions.Hide();
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleEvent_Votes(NetworkGameEvent_Run_Map_VotingChanged votesChange)
        {
            foreach (KeyValuePair<ClientInfo, int> kvp in votesChange.Votes)
            {
                ClientInfo clientInfo = kvp.Key;
                int nodeIndex = kvp.Value;
                _viewActions.SetVote(nodeIndex, clientInfo.PlayerIndex);
            }
            return ChangeHandledResult_Finished.Instance;
        }

        private static int CalculateNodeLayer(LocationMapInstance mapInstance, int nodeIndex)
        {
            int layer = 0;
            HashSet<int> visitedNodes = new();
            Queue<int> queue = new();
            queue.Enqueue(0);
            visitedNodes.Add(0);

            while (queue.Count > 0)
            {
                int currentIndex = queue.Dequeue();
                if (currentIndex == nodeIndex)
                {
                    return layer;
                }

                LocationNodeInstance currentNode = mapInstance.Nodes[currentIndex];
                if (currentNode.ChildIndices.Count > 0)
                {
                    int nextLayerSize = currentNode.ChildIndices.Count;
                    for (int i = 0; i < nextLayerSize && queue.Count < mapInstance.Nodes.Count; i++)
                    {
                        int childIndex = currentNode.ChildIndices[i];
                        if (!visitedNodes.Contains(childIndex))
                        {
                            queue.Enqueue(childIndex);
                            visitedNodes.Add(childIndex);
                        }
                    }

                    if (queue.Count > 0)
                    {
                        layer++;
                    }
                }
            }

            return layer;
        }
    }
}
