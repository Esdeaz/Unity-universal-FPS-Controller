# Unity-universal-FPS-Controller
Attention! This controller only works with HDRP lighting, so make sure your project uses the High Defenition Render Pipeline. Instructions for installing scripts on your player:

1. Make the camera a child of your player and put the MouseLook.cs script on the camera
2. Put the PlayerMovement.cs script on your player object
3. If you are using an object in the scene that stores the current HDRP settings, drag this object to the Render field in the PlayerMovement.cs component
4. Create a Layer Mask and enter the text "Ground" in any field. Objects that should be an obstacle for the player must have a "Ground" layer.
5. In the inspector window put the "Ground" mask on the obstacle objects
6. Make sure to add two children groundChecker and obstacleChecker to your player and place them above and below the player, respectively.
7. Drag and drop all the required objects into the fields of the PlayerMovement.cs component.
That's all. Here are the optimal settings I used for my FPS controller:

Player Speed = 5f
Acceleration = 17f
Gravity = -39.2;
Jump Height = 5
Bobbing Speed = 0.18;
Bobbing Amount = 0.2;
Midpoint = 0.457f
