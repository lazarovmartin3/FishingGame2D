const { dir } = require('console');
const express = require('express');
const http = require('http');
const WebSocket = require('ws');
const path = require('path'); 

const PORT = process.env.PORT || 8080;

const app = express();
const server = http.createServer(app);
const wss = new WebSocket.Server({ server });

// Serve static files from the 'WebGL' directory
app.use(express.static(path.join(__dirname, 'WebGL')));

// Store game instances
const gameInstances = new Map();

// Counter for player IDs
let playerIdCounter = 0;
// Initialize game ID counter
let nextGameId = 1; 
let fishIds = [];

// Function to generate player ID
function generatePlayerId() {
    return playerIdCounter++;
}

// Function to send JSON data to a WebSocket client
function sendJSON(ws, data) {
    ws.send(JSON.stringify(data));
}

// Function to broadcast JSON data to all WebSocket clients in a game instance
function broadcastToGame(gameId, data) {
    if (gameInstances.has(gameId)) {
        const gameInstance = gameInstances.get(gameId);
        gameInstance.players.forEach(player => {
            sendJSON(player, data);
        });
    }
}

// WebSocket server logic
wss.on('connection', function connection(ws) {
    console.log('A new player connected');

    // Assign a player ID to the connected client
    const playerId = generatePlayerId();

    // Handle messages from the client
    ws.on('message', function incoming(message) {
        console.log(`Received from player ${playerId}: ${message}`);

        try {
            const data = JSON.parse(message);
            const gameId = data.gameId;
            // Handle different actions from the client
            switch (data.action) {
                case 'JoinGame':
                    if (!gameInstances.has(gameId)) {
                        sendJSON(ws, { action: 'GameNotFound', gameId: gameId });
                    } else {
                        const gameInstance = gameInstances.get(gameId);
                        if (gameInstance.players.size >= 2) {
                            sendJSON(ws, { action: 'GameFull', gameId: gameId });
                        } else {
                            ws.playerId = playerId;
                            gameInstance.players.add(ws);
                            // Determine player roles
                            const playerIds = Array.from(gameInstance.players).map(ws => ws.playerId);
                            const hostPlayerId = playerIds[0]; // The first player to join is the host
                            const joinedPlayerId = playerId; // The player who just joined

                            // Send role information to all players
                            gameInstance.players.forEach(player => {
                                sendJSON(player, { 
                                    action: 'JoinedGame', 
                                    gameId: gameId, 
                                    playerId: joinedPlayerId, 
                                    hostPlayerId: hostPlayerId, // Include host player ID
                                    playerIds: playerIds,
                                    fishIds: fishIds
                                });
                            });
                        }
                    }
                    break;
                case 'CreateGame':
                    const newGameId = nextGameId++;
                    const newGameInstance = { id: newGameId, players: new Set() };
                    ws.playerId = playerId;
                    newGameInstance.players.add(ws);
                    gameInstances.set(newGameId, newGameInstance);
                    fishIds = generateFishIds();
                    
                    sendJSON(ws, { action: 'CreatedGame', gameId: newGameId, playerId: playerId, fishIds: fishIds  });
                    
                    // Call moveFishRandomly every 0.5 seconds for the new game instance
                    const fishMovementInterval = setInterval(() => moveFishRandomly(newGameId), 100);

                    // Store the interval ID in the game instance for later cleanup
                    newGameInstance.fishMovementInterval = fishMovementInterval;

                    break;
                
                    
                case 'GetAllGameIds':
                    const gameIds = Array.from(gameInstances.keys());
                    sendJSON(ws, { action: 'AllGameIds', gameIds: gameIds });
                    break;
                case 'GetServerInstances':
                    const serverInstances = Array.from(gameInstances.keys());
                    sendJSON(ws, { action: 'ServerInstances', instances: serverInstances });
                    break;
                case 'GetPlayerIndex':
                    if (!gameInstances.has(gameId)) {
                        sendJSON(ws, { action: 'GameNotFound', gameId: gameId });
                    } else {
                        const gameInstance = gameInstances.get(gameId);
                        const playerCount = gameInstance ? gameInstance.players.size : 0;
                        const playerIds = [];
                        gameInstance.players.forEach(player => {
                            playerIds.push(getPlayerId(player));
                        });
                        sendJSON(ws, { action: 'PlayerIndex', gameId: gameId, playerCount: playerCount, playerIds: playerIds });
                    }
                    break;
                case 'PlayerUpdate':
                    if (!gameInstances.has(gameId)) {
                        sendJSON(ws, { action: 'PlayerUpdateFailed', gameId: gameId });
                    } else {
                        const gameInstance = gameInstances.get(gameId);
                        gameInstance.players.forEach(player => {
                            player.send(JSON.stringify({ action: 'UpdatePlayer', update: data.update }));
                        });
                    }
                    break;
                case 'ThrowFishingRod':
                    if (!gameInstances.has(gameId)) {
                        sendJSON(ws, { action: 'GameNotFound', gameId: gameId });
                    } else {
                        const { playerId, targetPosition, isCasting, isRetracting, isThrowing, hookInitPosition } = data;
                
                        if (typeof playerId !== 'number' || !targetPosition || typeof targetPosition.x !== 'number' || typeof targetPosition.y !== 'number'|| typeof targetPosition.z !== 'number' || !hookInitPosition || typeof hookInitPosition.x !== 'number' || typeof hookInitPosition.y !== 'number'|| typeof hookInitPosition.z !== 'number') {
                            sendJSON(ws, { action: 'InvalidData', message: 'Invalid player ID, target position, or hook initial position' });
                            return;
                        }
                
                        const hookPosition = { playerId, position: targetPosition };
                
                        broadcastToGame(gameId, { 
                            action: 'UpdateHookPosition', 
                            hookPosition: hookPosition,
                            playerId: playerId,
                            gameId: gameId,
                            targetPosition: targetPosition,
                            hookInitPosition: hookInitPosition,
                            isCasting: isCasting !== undefined ? isCasting : false,
                            isRetracting: isRetracting !== undefined ? isRetracting : false,
                            isThrowing: isThrowing !== undefined ? isThrowing : false
                        });
                    }
                    break;
                    case 'CatchFish':
                        if (!gameInstances.has(gameId)) {
                            sendJSON(ws, { action: 'GameNotFound', gameId: gameId });
                        } else {
                            const gameInstance = gameInstances.get(gameId);
                            const { fishId } = data;
                    
                            // Check if the fish exists in the game instance
                            if (fishIds.includes(fishId)) {
                                // Remove the fish from the fishPositions map to stop its movement
                                fishIds.get(fishId).caught = true;
                    
                                // Broadcast the update to all players to stop displaying the fish
                                broadcastToGame(gameId, { action: 'FishCaught', fishId: fishId });
                            } else {
                                sendJSON(ws, { action: 'FishNotFound', fishId: fishId });
                            }
                        }
                    break;
                    
                default:
                    console.log('Unknown action:', data.action);
            }

            // Delete empty game instances
            gameInstances.forEach((gameInstance, gameId) => {
                if (gameInstance.players.size === 0) {
                    gameInstances.delete(gameId);
                }
            });

        } catch (error) {
            console.error('Error handling message:', error);
        }

    });

    ws.on('close', function() {
        console.log(`Player ${playerId} disconnected`);

        // Remove the disconnected player from all game instances
        gameInstances.forEach(gameInstance => {
            gameInstance.players.delete(ws);
        });
    });

});

function generateFishIds() {
    const fishObjects = [];
    for (let i = 1; i <= 20; i++) {
        // Generate a unique ID for each fish
        const fishId = i;
        const direction = getRandomDirection();
        // Create a new Fish object with the ID, positions, and direction
        const fish = new Fish(fishId, direction.x, direction.y);
        // Add the fish object to the array
        fishObjects.push(fish);
    }
    return fishObjects;
}

// Define a function to generate a random direction vector
function getRandomDirection() {
    const randomAngle = Math.random() * Math.PI * 2; // Random angle in radians
    const x = Math.cos(randomAngle);
    const y = Math.sin(randomAngle);
    return { x, y };
}

const BOUNDARY_RADIUS = 6; // Radius of the bounding circle
const MIN_DISTANCE_FROM_BOUNDARY = 2; // Minimum distance from the boundary before changing direction
const SPEED = 0.1;

// Define a function to move all the fish in their respective random directions
function moveFishRandomly(gameId) {
    // Create an array to store all the fish updates
    const fishUpdates = [];
    //fishUpdates.push({ action: "FishUpdate" });

    for (const fish of fishIds) {
        // If the fish doesn't have a direction yet, generate a random one
        if (!fish.direction || Math.abs(fish.x) >= BOUNDARY_RADIUS - MIN_DISTANCE_FROM_BOUNDARY || Math.abs(fish.y) >= BOUNDARY_RADIUS - MIN_DISTANCE_FROM_BOUNDARY) {
            fish.direction = getRandomDirection();
        }
        
        const dx = fish.direction.x * SPEED;
        const dy = fish.direction.y * SPEED;
        // Move the fish in its direction
        fish.x += dx; // Move in the x direction
        fish.y += dy; // Move in the y direction
    
        // Ensure the fish stays within the bounds of the 50x50 rectangle
        fish.x = Math.max(-BOUNDARY_RADIUS, Math.min(BOUNDARY_RADIUS, fish.x));
        fish.y = Math.max(-BOUNDARY_RADIUS, Math.min(BOUNDARY_RADIUS, fish.y));


        fishUpdates.push({
            id: fish.id,
            x: fish.x, y: fish.y 
        });
    }
    broadcastToGame(gameId, { action: 'FishUpdate', fishIds: fishUpdates });
    //broadcastToGame(gameId, fishUpdates);
}

// Start the server
server.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});

class Fish {
    constructor(id, x, y) {
        this.id = id;
        this.x = x;
        this.y = y;
        this.caught = false;
    }
}