# 🏎️ Crazy Racing Revolution One

[![Gameplay Thumbnail](https://i.ytimg.com/vi/MrilMBBXJaE/maxresdefault.jpg)](https://youtu.be/MrilMBBXJaE)

_Click to watch the gameplay video on YouTube!_

![Release Year](https://img.shields.io/badge/Release_Year-2017-blue) ![Unity Version](https://img.shields.io/badge/Unity-5.6-green) ![3ds Max](https://img.shields.io/badge/3ds_Max-2017-orange) ![Photoshop](https://img.shields.io/badge/Photoshop-CC_2017-purple)

"Crazy Racing Revolution One" is a racing game developed in the Unity engine. This project features high-speed cars, diverse tracks, and multiple game modes inspired by classic racing titles. The primary goal was to create an advanced prototype of a racing game, focusing on a comprehensive architecture, realistic driving mechanics, and a rich user experience.

## 📋 Table of Contents

- [About The Project](#about-the-project)
- [Key Features](#key-features)
- [Game Modes](#game-modes)
- [Screenshots](#screenshots)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Future Development](#future-development)

## ℹ️ About The Project

"Crazy Racing Revolution One" is a racing game where players compete in super-fast cars across desert, city, and seaside tracks. The project was inspired by a passion for motorsports and well-known racing games, with the aim of creating a unique game engine with effective graphics and an original soundtrack for a realistic experience.

Gameplay centers around racing against AI-controlled vehicles. Before each race, the player can select a car, track, and game mode, as well as customize the number of opponents and laps. Money earned from winning races can be used to buy new cars or tune existing ones.

## 🏎️ Key Features

- **AI Opponents**: Compete against up to five AI-controlled rivals with selectable difficulty levels.
- **Vehicle Customization**:
  - Purchase from a selection of 6 car models, including the Porsche 911 and Ferrari 488.
  - Tune performance aspects like max speed, acceleration, handling, and braking.
  - Modify visual elements such as body color, wheels, neons, and vinyls.
- **Dynamic Graphics**:
  - Features like street shadows, realistic lighting (Directional, Point, Spot), and lens flares create an immersive environment.
  - Particle systems are used for realistic exhaust fumes, tire smoke during drifts and hard braking, and a Nitrous Oxide System (NOS) boost effect.
- **Multiple Race Tracks**: Race on two distinct tracks: Laguna Seca and Monte Carlo Circuit.
- **Advanced Camera System**: Switch between a first-person (in-car) view and a third-person chase camera. The first-person view includes functional rear-view and side mirrors.
- **Full-Featured UI/HUD**: A detailed menu for navigating game options, tuning cars, and setting up races. The in-game HUD includes a speedometer, mini-map, nitro gauge, lap/position indicators, and more.
- **Save/Load System**: Players can save their progress, including unlocked cars, tuning setups, and race achievements, and load it later. This is achieved through serialization.
- **Input Support**: Play using a keyboard or a gamepad, with reconfigurable controls.

## 🎮 Game Modes

- **CIRCUIT**: A standard race on a closed track for a set number of laps.
- **TIME TRIAL**: Beat the clock by finishing a lap or track within a specified time limit.
- **LAP KNOCKOUT**: After each lap, the racer in last place is eliminated.
- **SPEED TRAP**: The winner is the player with the highest cumulative speed recorded at various speed traps along the track.
- **CHECKPOINTS**: A point-A-to-point-B race where players must pass through a series of checkpoints.

## 📸 Screenshots

<table>
  <tr>
    <td width="50%"><strong>Main Menu</strong><br><img src="https://i.imgur.com/72fNgaK.png" width="100%" alt="Main Menu"></td>
    <td width="50%"><strong>Car Selection</strong><br><img src="https://i.imgur.com/1lka8fz.png" width="100%" alt="Car Selection"></td>
  </tr>
  <tr>
    <td width="50%"><strong>Tuning</strong><br><img src="https://i.imgur.com/7pY8XGk.png" width="100%" alt="Tuning"></td>
    <td width="50%"><strong>Track Selection</strong><br><img src="https://i.imgur.com/cjeWKzH.png" width="100%" alt="Track Selection"></td>
  </tr>
  <tr>
    <td width="50%"><strong>Settings</strong><br><img src="https://i.imgur.com/VVboDZr.png" width="100%" alt="Settings"></td>
    <td width="50%"><strong>Profile Creation</strong><br><img src="https://i.imgur.com/IYsUNrA.png" width="100%" alt="Profile Creation"></td>
  </tr>
</table>

## 🛠️ Tech Stack

This project was made possible using the following tools and technologies:

- [Unity](https://unity3d.com/unity/licenses) - The core game engine (version 5.6 from 2017).
- [3ds Max](https://www.autodesk.com/products/3ds-max/overview) - Used for 3D modeling, animation, and rendering (version 2017).
- [Adobe Photoshop](https://www.adobe.com/products/photoshop.html) - Used for creating and editing 2D raster graphics and textures (version CC 2017).

## ⚙️ Getting Started

To get a local copy up and running, follow these steps.

### Prerequisites

- Unity Hub (compatible with Unity version from 2017)
- Unity Editor (recommended version 5.6 or similar from that period)
- Git

**Note**: Since the project dates back to 2017, it may require an older environment to function correctly. Newer Unity versions might cause compatibility issues. If you encounter problems, try installing Unity 5.6 from the Unity archive.

### Installation

1. Clone the repository:
   ```sh
   git clone https://github.com/your_username/CrazyRacingRevolutionOne.git
   ```
2. Open the project in Unity Hub by clicking "Add project" and selecting the cloned directory.
3. Unity will automatically resolve packages and dependencies. If errors occur, ensure you are using the correct Unity version (5.6).
4. Once loaded, open the main menu scene (e.g., `MainMenu.unity`) and press the Play button to start the game.

**Troubleshooting**: If the project does not load correctly in newer Unity versions, try opening it in Unity 5.6. You can download older Unity versions from the archive on the Unity website. Also, ensure all required assets are properly imported.

## 🚀 Future Development

While the current version is a complete prototype, there are several ideas for future expansion:

- **Platform Expansion**: Port the game to consoles like the Xbox and PlayStation.
- **VR Support**: Implement support for VR headsets for a more immersive experience.
- **Multiplayer**: Expand the existing framework to include online multiplayer gameplay.
- **More Content**: Add more cars, tracks, and customization options.
