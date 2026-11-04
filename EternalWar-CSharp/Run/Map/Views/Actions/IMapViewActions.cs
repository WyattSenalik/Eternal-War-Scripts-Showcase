// Original Authors - Jack Dekko
namespace EternalWar.Client.Application.Run.Map.Views.Actions
{
    public interface IMapViewActions
    {
        void CreateNode(string nodeType, int nodeLayer, int nodeIndex);
        void DestroyAllNodes();
        void SetVote(int nodeIndex, int playerIndex);
        void Show();
        void Hide();
    }
}
