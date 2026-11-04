// Original Authors - Wyatt Senalik
using Godot;
using System;

namespace EternalWar.Engine.Godot.Lobby.Nodes
{
    public sealed partial class GodotLobbyNodeCollection : Node
    {
        public Control Root => _root ?? throw new ArgumentNullException(nameof(_root));
        public Control LandingScreenParent => _landingScreenParent ?? throw new ArgumentNullException(nameof(_landingScreenParent));
        public LineEdit PortLineEdit => _portLineEdit ?? throw new ArgumentNullException(nameof(_portLineEdit));
        public LineEdit IPAddressLineEdit => _ipAddressLineEdit ?? throw new ArgumentNullException(nameof(_ipAddressLineEdit));
        public Button HostButton => _hostButton ?? throw new ArgumentNullException(nameof(_hostButton));
        public Button JoinButton => _joinButton ?? throw new ArgumentNullException(nameof(_joinButton));
        public Control LoadingScreenParent => _loadingScreenParent ?? throw new ArgumentNullException(nameof(_loadingScreenParent));
        public Control ConnectedScreenParent => _connectedScreenParent ?? throw new ArgumentNullException(nameof(_connectedScreenParent));
        public Label IAmReadyLabel => _iAmReadyLabel ?? throw new ArgumentNullException(nameof(_iAmReadyLabel));
        public Button ToggleReadyButton => _toggleReadyButton ?? throw new ArgumentNullException(nameof(_toggleReadyButton));
        public Label PlayersReadyTextLabel => _playersReadyTextLabel ?? throw new ArgumentNullException(nameof(_playersReadyTextLabel));
        public Button DisconnectButton => _disconnectButton ?? throw new ArgumentNullException(nameof(_disconnectButton));

        [Export] private Control? _root;

        // Landing Screen
        [Export] private Control? _landingScreenParent;
        [Export] private LineEdit? _portLineEdit;
        [Export] private LineEdit? _ipAddressLineEdit;
        [Export] private Button? _hostButton;
        [Export] private Button? _joinButton;

        // Loading Screen
        [Export] private Control? _loadingScreenParent;

        // Connected Screen
        [Export] private Control? _connectedScreenParent;
        [Export] private Label? _iAmReadyLabel;
        [Export] private Button? _toggleReadyButton;
        [Export] private Label? _playersReadyTextLabel;
        [Export] private Button? _disconnectButton;
    }
}