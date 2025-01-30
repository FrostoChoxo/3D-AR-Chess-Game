# 3D AR Chess Game for HoloLens 2

This project is a **3D chess game** designed for **Augmented Reality (AR)** on the **HoloLens 2**. It offers an immersive chess experience in AR with two game modes: **AI** and **Multiplayer**, both linked to the **Lichess API** to fetch and update moves in real-time.

[![Showcase Video](https://img.youtube.com/vi/4Fq6fquZlXk/0.jpg)](https://www.youtube.com/watch?v=4Fq6fquZlXk)

***NOTE**:The video is not accurate to the actual gameplay, the textures and screen size were inaccurate when recorded with the inbuilt hololens recorder.

The goal of this project is to provide a 3D chess game with **no platform limitations**, allowing players to experience chess in a fully interactive AR environment, whether they’re playing against an AI or competing with other players online.

## Features

- **AI Mode**: Play against an AI powered by the Lichess API, where the AI makes intelligent moves based on the current board state.
- **Multiplayer Mode**: Connect with friends or other players via the Lichess API to play in real-time across platforms, making the game accessible to anyone using Lichess.
- **Seamless Integration with Lichess**: Moves made by either the player or the AI are synced with the Lichess API to ensure consistent gameplay between virtual and real-life boards.
- **3D Chessboard in AR**: Interact with a 3D chessboard placed in the real world through AR, allowing for a more immersive experience.
- **Cross-Platform Play**: Players can enjoy the game in AR without platform constraints, as the Lichess API handles the game logic.

## Technology

- **Unity 3D**: The game is developed using Unity, ensuring a high-quality, interactive 3D experience in AR.
- **Lichess API**: Used to fetch moves for both AI and multiplayer modes.
- **HoloLens 2**: The game is optimized for the HoloLens 2, taking full advantage of spatial mapping, hand gestures, and other AR capabilities.

## Setup Instructions

1. **Clone the repository**:
   ```bash
   git clone https://github.com/FrostoChoxo/3D-AR-Chess-Game.git
2. **Install Dependencies**:
   Ensure you have Unity installed.
   Import the project into Unity.
   Set up the HoloLens 2 SDK for building the application.

3. **API Integration**:
  Set up your Lichess API keys in the Unity project for the Multiplayer and AI modes to function.

4. **Build for HoloLens 2**:
  Configure your project for HoloLens 2 deployment in Unity.
  Build and deploy the application to your HoloLens 2.
