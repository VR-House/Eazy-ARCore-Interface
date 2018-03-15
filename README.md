![Eazy ARCore Interface](http://i65.tinypic.com/2z8tc43.png)

**Eazy ARCore Interface** is a Unity3D plugin which makes development and debugging of ARCore projects easier. Specifically, it simulates how ARCore works in an Android device inside of Unity3D editor. Thus, it allows for faster development of ARCore apps, without the need to build and deploy to the device in order to test fuctionality. Debugging of ARCore apps becomes way easier as well, since  executing directly in the editor allows you to utilize Unity debug tools. Moreover, to deploy to the actual device, no changes whatsoever are needed. The exact same project can run in the editor and on the device.

ARCore simulation in the editor is done by automatically creating and simulatating a tracked plane, with which you can interact the same way as you can with an actual tracked plane on a device. **Tracked points are not yet supported for simulation.** However, they can still be used on a device by accessing them using the native ARCore SDK.

**This tools is experimental and not intended for commercial use. Only the basic features of ARCore were implemented and simulated. If complex features and funtionality are desired, you have to make use of the native ARCore SDK or implement them on your own.**

## Features
- ARCore simulation in Unity3D editor
- Control over simulated ARCore state
- Ground plane detection
- Tracked planes and points visualization
- Allows for physics interaction with tracked planes (Collisions or triggers)
- API very similar to native ARCore
- No changes required for device deployment

## Getting Started
- Import ARCore SDK for Unity (v1.0.0 already included).
- Import Eazy ARCore Interface.
- Add a Layer and a Tag "EazyARTrackedPlane".
- Drag an ARCore Device prefab into your scene (Assets > GoogleARCore > Prefabs).
- Drag an EazyARCoreInterface prefab into your scene (Assets > Eazy Tools > ARCore Interface > Prefabs).
- In the EazyARCoreInterface AR Camera field, assign the first person camera found in the ARCore Device object.
- Write your own game logic or use the example one (Assets > Eazy Tools > ARCore Interface > Demo > Scripts > HelloEazyARController).
- Play in editor, for development and debugging.
- Deploy to device.

## License
MIT License. Copyright 2018.
