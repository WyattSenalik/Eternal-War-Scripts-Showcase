// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Lobby.Interaction.Actions;
using EternalWar.Engine.Godot.Lobby.Nodes;
using GodotLineEdit = Godot.LineEdit;

namespace EternalWar.Engine.Godot.Lobby
{
    public static class GodotLobbyUIBinding
    {
        public static void Bind(GodotLobbyNodeCollection nodeCollectionInstance, ILobbyInteractionActions interactionActions)
        {
            GodotLineEdit portLineEdit = nodeCollectionInstance.PortLineEdit;
            GodotLineEdit ipAddressLineEdit = nodeCollectionInstance.IPAddressLineEdit;

            portLineEdit.TextChanged += interactionActions.SetPort;
            ipAddressLineEdit.TextChanged += interactionActions.SetIPAddress;
            nodeCollectionInstance.HostButton.Pressed += interactionActions.ButtonPressed_HostGame;
            nodeCollectionInstance.JoinButton.Pressed += interactionActions.ButtonPressed_JoinGame;
            nodeCollectionInstance.ToggleReadyButton.Pressed += interactionActions.ButtonPressed_ToggleReady;
            nodeCollectionInstance.DisconnectButton.Pressed += interactionActions.ButtonPressed_Disconnect;

            interactionActions.SetPort(portLineEdit.Text);
            interactionActions.SetIPAddress(ipAddressLineEdit.Text);
        }
    }
}
