
using System;
using UnityEngine;
namespace Zombies
{
	public class CameraMovement : MonoBehaviour{

		int sensitivity = 200;
		int scrollSensitivity = 500;
		int scrollMaxDistance = 9999;
		int scrollMinDistance = 5;
	
		void Start() {
		}
				
		// Update is called once per frame
		void Update () {

			float moveRate = sensitivity * Time.deltaTime;
			float scrollRate = scrollSensitivity * Time.deltaTime;

			float deltaX = 0;
			float deltaY = 0;
			float deltaZ = 0;

			// Move the camera with the arrow keys or with the mouse.
			if ( Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)){
					deltaY = moveRate;
			}
			if ( Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)){
					deltaY = -moveRate;
			}
			if ( Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)){
					deltaX = moveRate;
			}
			if ( Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)){
					deltaX = -moveRate;
			}

			// Zoom in/out with the scroll wheel.
			if (Input.GetAxis("Mouse ScrollWheel") > 0){
				if (camera.orthographicSize - scrollRate > scrollMinDistance)
					camera.orthographicSize = camera.orthographicSize - scrollRate;
				else
					camera.orthographicSize = scrollMinDistance;
			}
			if (Input.GetAxis("Mouse ScrollWheel") < 0){
				if (camera.orthographicSize + scrollRate < scrollMaxDistance)
					camera.orthographicSize = camera.orthographicSize + scrollRate;
				else
					camera.orthographicSize = scrollMaxDistance;

			}

			moveCamera(deltaX, deltaY, deltaZ);
		}

		private void moveCamera(float x, float y, float z){
			// Different coordinate standards.
			transform.position = new Vector3(transform.position.x + x, transform.position.y + z, transform.position.z + y);
		}

		public void setStartingPosition(Vector3 position){
			transform.position = new Vector3(position.x, transform.position.y, position.z);
		}

		public float GetDistanceRatio(){
			return camera.orthographicSize / scrollMaxDistance;
		}
	
	}
}

