/*
This camera smoothes out rotation around the y-axis and height.
Horizontal Distance to the target is always fixed.

There are many different ways to smooth the rotation but doing it this way gives you a lot of control over how the camera behaves.

For every of those smoothed values we calculate the wanted value and the current value.
Then we smooth it using the Lerp function.
Then we apply the smoothed values to the transform's position.
*/

// The target we are following.
var target : Transform;

// The distance in the x-z plane to the target.
var distance : float = 10.0;

// The height we want the camera to be above the target.
var cameraHeight : float = 3.0;

var heightDamping : float = 2.0;
var rotationDamping : float = 3.0;
var rotationOffset : float = 0.0;
var lookVerticalOffset : float = 2.0f;

// Place the script in the Camera-Control group in the component menu.
@script AddComponentMenu("Camera-Control/Smooth Follow")

function LateUpdate () {
	// Early out if we don't have a target.
	if (!target) {
		return;
	}
	
	// Calculate the current rotation angles.
	var wantedRotationAngle : float = target.eulerAngles.y + rotationOffset;
	var wantedHeight : float = target.position.y + cameraHeight;	
	var currentRotationAngle : float = transform.eulerAngles.y;
	
	// Damp the rotation around the y-axis.
	currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

	// Convert the angle into a rotation.
	var wantedRotation : Quaternion = Quaternion.Euler(0, currentRotationAngle, 0);
	
	// Set the position of the camera on the x-z plane to: distance meters behind the target.
	transform.position = target.position;
	transform.position -= wantedRotation * Vector3.forward * distance;

	// Set the height of the camera.
	transform.position.y = wantedHeight;
	
	// Always look at the target.
	var targetLook : Vector3 = target.position + new Vector3(0, lookVerticalOffset, 0);
	transform.LookAt(targetLook);
}
