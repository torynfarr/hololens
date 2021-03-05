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
<br />

## 02.[Azure Spatial Anchors Litecoin Mining Stats](https://github.com/torynfarr/hololens/tree/master/Assets/Samples/02.azure-spatial-anchors-litecoin-mining-stats)

This sample demonstrates how to create, find, and delete an Azure Spatial Anchor. It also showcases several features of the Mixed Reality Toolkit including hand rays, spatial awareness, surface magnetism, tap to place, hand menus, and eye tracking.

- Move either hand around to position the Litecoin symbol and then tap your thumb and index finger together to place it.

- Turn either hand so that your palm is facing you to display the hand menu. 

- Tap *Create Spatial Anchor* to create an Azure Spatial Anchor. The Litecoin symbol will be replaced with a slate.

- When you look at the slate, it will update the data displayed by making an http request to litecoinpool.org.

- Look away and the slate will stop updating.

- Stare at the slate for five seconds and it will start a timer loop which causes the data to update every five seconds.

- The ID of the Azure Spatial Anchor is stored in the Unity PlayerPrefs. When you close the application and relaunch it, it will ask you to look around to locate the Azure Spatial Anchor.

- When the Azure Spatial Anchor is located, the slate will be reloaded in exactly the same position it was in before.

- Change the currency conversion shown in the slate by tapping the *Currency* button in the hand menu. Select the currency of your choice. Your selection will be used the next time the slate updates.

- Tap the *Delete Spatial Anchor* button in the hand menu to delete the Azure Spatial Anchor. The Litecoin symbol will be displayed again and is ready to be placed in your environment.
<br />
<img src="https://github.com/torynfarr/hololens/blob/master/docs/images/azure-spatial-anchors-litecoin-mining-stats.gif" width="350">
<br />

## Configuration

In Unity, you'll need to right-click and create both an Azure Spatial Anchors and Litecoin Pool configuration asset. With these assets created, you can then view them in the inspector and input your account specific settings.

- To create an Azure Spatial Anchor resource in Azure, please view this [documentation](https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-hololens?tabs=azure-portal#create-a-spatial-anchors-resource).
- Take note of the account ID, key, and domain. Input those values in the Azure Spatial Anchor configuration asset that you created in Unity.

- The settings to input in the Litecoin Pool configuration asset can be obtained from the [My Account](https://www.litecoinpool.org/account) page at litecoinpool.org. 

- Input the following values in the Litecoin Pool configuration asset that you created in Unity:
    - Set the host name to: *www.litecoinpool.org*
    - Set the path to: *api?api_key*
    - Set the API key to the key you obtained from your Litecoin pool account settings.

- Drag the Azure Spatial Anchors configuration asset in Unity to the *Azure Spatial Anchors Configuration* field on the Main Hand Menu in the scene hierarchy.
<br />
<img src="https://github.com/torynfarr/hololens/blob/master/docs/images/azure-spatial-anchors-litecoin-mining-stats-inspector-01.png" width="500">
<br />
- Open the slate prefab and drag the Litecoin Pool configuration asset to the *Litecoin Pool Configuration* field on the ContentQuad in the slate prefab.
<br />
<img src="https://github.com/torynfarr/hololens/blob/master/docs/images/azure-spatial-anchors-litecoin-mining-stats-inspector-02.png" width="500">
<br />
<br />

# Additional Information

- These samples were created using Unity version 2019.4.21f1
- The Mixed Reality Toolkit Unity package is version 2.5.4
- The Azure Spatial Anchors Unity package is version 2.7.2