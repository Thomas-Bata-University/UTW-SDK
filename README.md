# UTW - Procedural Terrain Generator

This repository is part of the UTW SDK — a Unity-based development toolkit.  
It provides a modular and extendable system for procedural terrain generation aimed at realistic simulation environments.  

This branch implements a specialized procedural terrain generation tool.  
The generated landscapes are designed to be tank-traversable, visually realistic, and highly adjustable via editor interfaces.

---

## Overview

This Unity Editor extension allows users to create terrain maps procedurally based on defined rules, using:
- heightmap generation,
- biome weight distribution,
- vegetation placement,
- erosion simulation.

It offers a structured, yet flexible interface where users can customize:
- map dimensions,
- terrain roughness,
- biome count and properties,
- vegetation coverage per biome.

---

## How to Use

1. **Install Project**  
   - Open Unity Hub and load the UTW SDK project.
   - Ensure all dependencies (e.g., FastNoiseLite, Vegetation assets) are imported.

2. **Create or Open a Terrain Project**
   - In the Unity top menu, go to `UTW > Tools > Create or Open Project`.
   - In the dialog window, select `MAP` as the project type.
   - Create a new folder for your map project and confirm it via `Select Folder`.

3. **Configure Parameters**  
   - Set the `Seed` manually or enable `Random Seed`.
   - Define map size using `Map Width` and `Map Height`.
   - Adjust `Height Range` to control terrain elevation.
   - Set the `Number of Biomes` and assign existing or newly created ones.
   - (Optional) Enable `Generate Vegetation` to populate the terrain.

4. **Select or Create Biomes**
   - Use the interface to either assign existing biomes or create new ones.
   - Each biome allows assigning custom vegetation prefabs (trees, grass, shrubs).
   - You can use included prefabs or import your own into the project (e.g., from Asset Store or custom sources).

5. **Generate Terrain**  
   - Click the `Generate Terrain` button at the bottom of the panel.
   - The tool will create a terrain mesh, apply height and biome blending, and optionally add vegetation.

---

## Features

- **Editor-based terrain configuration**  
  Easily accessible Unity window for setting generation parameters.

- **Custom biomes**  
  Create and edit biomes as `ScriptableObject` assets, including materials, vegetation density, and distribution rules.

- **Vegetation generation**  
  Supports grass, shrubs, trees. Includes clustering and Poisson disk sampling for realistic distribution.

- **Erosion simulation**  
  Thermal and hydraulic erosion algorithms simulate natural shaping of the landscape.

- **Mesh generation**  
  High-resolution mesh generated with color blending based on biome weights.

- **Extensibility**  
  Future-proof system suitable for adding road or city generation logic.

---

## Requirements

- Unity 2021.3 or later
- .NET 4.x / C# 8+
- FastNoiseLite (included or downloadable)
- Vegetation prefabs (e.g. free assets from Unity Asset Store)

---

## License

This project uses assets under Unity Asset Store license. The core generator logic is distributed under the MIT License unless otherwise noted.

