# AdVenture Capital

A satirical game that explores the absurdity of modern mobile game monetization through deliberately intrusive advertising mechanics.

## Credits

**Team Members:**
- Kyle Woo - Programming
- Jaime Abrego - Art & VFX
- Tyler Rund - Programming

**Course:** DIG 4720 - Casual Games Development  
**Institution:** University of Central Florida  
**Semester:** Fall 2025

## Overview

AdVenture Capital is a collection of simple mobile-style games interrupted by intentionally disruptive advertisements. The game satirizes aggressive monetization tactics by making the ads themselves a core part of the experience.

## Controls

### Menu Navigation
- **Mouse Click** - Select menu options and interact with UI elements

### Slice Game
- **Mouse Movement** - Control slice direction
- **Left Click** - Execute slice

### Minesweeper
- **Left Click** - Reveal tile
- **Right Click** - Flag tile as mine

### Block Game
- **Arrow Keys** - Move blocks left/right
- **Down Arrow** - Soft drop
- **Up Arrow** - Rotate block
- **Space** - Hard drop

## Features

### Multiple Game Modes
- **Normal Mode** - Full ad experience with video and banner ads
- **Ad-Free Mode** - Clean gameplay without advertisements (premium experience)
- **No Ad Mode** - Interactive banner ads that trigger unskippable videos

### Integrated Advertising System
- Video advertisements that interrupt gameplay
- Banner advertisements with collision detection
- "Skip Ad" functionality with variable delays
- Ad interaction tracking and analytics

### Three Mini-Games
1. **Slice Game** - Precision slicing mechanics
2. **Minesweeper** - Classic puzzle gameplay
3. **Block Game** - Tetris-style block placement

### Meta Features
- Persistent game mode selection across scenes
- Fake feedback and terms & conditions systems
- Satirical UI elements and messaging

## New Game Mode Iteration

### "No Ad" Mode Feature
- Implemented a satirical "No Ad Mode" where disruptive video ads are removed, but banner ads become even more intrusive and animated
- Banner ads bounce around the screen or slowly grow to take over the game area
- Players must interact with animated banner ads while trying to play
- After 30 seconds of animated banners, an unskippable video ad plays, forcing players back to Normal Mode
- New assets and VFX created by Jaime Abrego enhance the visual comedy of the intrusive banners

### Additional Satirical Elements
- Fake Feedback button that links to a joke website
- Fake Terms & Conditions page with meaningless satirical content
- Mode locking system that traps players in "No Ad Mode" until they watch the full unskippable video

## Technical Implementation

### Architecture
- **Unity 2025.x** - Game engine
- **C# Scripting** - Core gameplay and systems
- **Scene Management** - Persistent mode system across multiple game scenes
- **UI System** - Canvas-based interface with dynamic ad integration

### Key Systems
- **GameModeController** - Manages game mode state and transitions
- **VideoAdSpawner** - Controls video ad playback and skip functionality
- **BounceAd & BannerAdGrow** - Animated banner ad behaviors for No Ad Mode
- **Static Coordination** - Cross-scene persistence and ad behavior synchronization

## Development Notes

This project explores game design as commentary, using exaggerated monetization mechanics to critique the mobile gaming industry's reliance on disruptive advertising. The "No Ad Mode" feature satirizes the false promise of ad-free experiences that often come with their own frustrations.

## Installation

1. Clone the repository
2. Open the project in Unity 2025.x or later
3. Open the main menu scene
4. Build and run for your target platform

## License

Educational project - University of Central Florida, Fall 2025
