A first person controller for your games

## How to use
To get started easily, you can use the prefab in the Samples/ folder

## Configure the player controller
PlayerController.cs fields:

### Configuration
- **Mouvement Speed**: Speed at which the player will walk
- **Horizontal Sensitivity**: Sensitivity of the mouse on the X axis
- **Vertical Sensitivity**: Sensitivity of the mouse on the Y axis
- **Controller Sensitivity**: Sensitivity multiplier when using a controller
- **Running Multiplier**: Speed to which the mouvement speed is multiplied, when running
- **Jump Force**: Vertical force applied when jumping
- **Gravity Multiplier**: Multiplier applied to the gravity when calculating falling speed

### Data
- **Head** (mandatory): Object used as the head that will rotate, assumed to be the camera or a child of it
- **PInput**: Player input object that control the mouvements, used for mobile controls
- **Trigger Area**: Object that detect collisions to interact with objects, allowing you to interact with them with `OnInteract`
- **Interaction Text**: Text that will display a hint when an object can be interacted with
