
using System;
using UnityEngine;
namespace Zombies
{
	public class CameraMovement : MonoBehaviour{

		int sensitivity = 200;
		int scrollSensitivity = 500;
		int scrollMaxDistance = 9999;
		int scrollMinDistance = 5;
		int mouseBorder = 20;
	
		void Start() {
		}
				
		// Update is called once per frame
		void Update () {
			float theScreenWidth = Screen.width;
			float theScreenHeight = Screen.height;


			float moveRate = sensitivity * Time.deltaTime;
			float scrollRate = scrollSensitivity * Time.deltaTime;

			float deltaX = 0;
			float deltaY = 0;
			float deltaZ = 0;

			// Move the camera with the arrow keys or with the mouse.
			if ( Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y > theScreenHeight - mouseBorder){
					deltaY = moveRate;
			}
			if ( Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y < 0 + mouseBorder){
					deltaY = -moveRate;
			}
			if ( Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x > theScreenWidth - mouseBorder){
					deltaX = moveRate;
			}
			if ( Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x < 0 + mouseBorder){
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

