# Fishing Game 2D

Description:
This is a simple 2D online multiplayer game where players can create new games or join existing ones to catch fish. The game features basic controls for casting and retracting the fishing hook, as well as a pause menu for game management.

Controls:

    Left Mouse Button: Casts the fishing line into the water.
    Right Mouse Button: Retracts the fishing line and attempts to catch fish.
    ESC Button: Opens the pause menu.

Gameplay:

    Players can create new games or join existing ones.
    Fish randomly spawn in the lake, with two types available: Rare (gold) and Common (grey).
    Players aim to catch fish by timing their casts and retractions.
    A simple probability system determines the likelihood of a fish biting.
    After each successful catch, statistics are tracked, including attempts and success rates.
    The game maintains a list of the last 10 caught fish.

Implementation:

    The game is built using Unity and relies on a client-server architecture for online multiplayer functionality.
    Networking features allow for seamless gameplay between multiple players.
    Player actions are synchronized across all connected clients in real-time.
    The game employs a probability system to simulate fish behavior and catch rates.

Installation:

    Clone the repository to your local machine.
    Open the project in Unity.
    Build and run the game to start playing.
