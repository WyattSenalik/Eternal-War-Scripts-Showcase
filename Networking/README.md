# Networking

`Networking.Core` defines the transport interfaces and delivery types. `Networking.Client` and `Networking.Server` provide transport-independent wrappers. `Networking.LiteNetLib` is the active UDP/IP transport used by the game client and dedicated server. It uses reliable ordered delivery for game messages; LiteNetLib fragments reliable messages that exceed a single datagram.

`Networking.Steam` implements the same backend interfaces through Facepunch Steamworks. It is included in the solution, but the game does not select it yet. Enabling it will also require initializing Steamworks in both the game process and dedicated server, and adding Steam server identity discovery/advertisement so clients know which Steam ID to connect to. The current game still connects by IP address and port. This adapter currently targets Facepunch.Steamworks 2.3.3, so refresh the Steamworks dependency before making Steam transport a release path.

The client polls its selected backend from `GameApplication.Update`. The dedicated server polls its backend on its existing game tick. Keep both poll loops when adding another backend.
