# HoloLens
This repository contains various sample Unity projects designed for the HoloLens 2 spatial computing device.
<br />
<br />

## 01.[Scene Understanding Find Closest Wall](https://github.com/torynfarr/hololens/tree/master/Assets/Samples/01.scene-understanding-find-closest-wall)

This sample uses Microsoft's Scene Understanding SDK and demonstrates how to detect the wall closest to the camera. Specifically, it detects the game objects rendered by the Scene Understanding Display Manager representing wall scene objects. The closest wall is determined by measuring the distance between the camera (the HoloLens 2) and the center of the bounds of each wall game object.

- Look around until your environment is thoroughly mapped and then tap *Stop Mapping.*

- Tap *Spawn New Cube* to instantiate a cube floating above the near menu.

- Poke the cube with your index finger on either hand to send it floating to the wall closest to you.

- Pinch the cube between the thumb and index finger on either hand to grab and move it.

- A cube sent to the wall closest to you will position itself in the center of that wall, at slightly below eye level, and will rotate to sit flush with the wall.
<br />
<img src="https://github.com/torynfarr/hololens/blob/master/docs/images/scene-understanding-find-closest-wall.gif" width="350">
<br />

## Additional Information

- These samples were created using Unity version 2019.4.1f1
- The Mixed Reality Toolkit Unity package is version 2.4.0