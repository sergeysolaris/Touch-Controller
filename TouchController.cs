using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchExt { 							//Some extention of UnityEngine.Input.Touch
	public int id;
	public Vector2 startPos;
	public Vector2 curPos;
	public Vector2 endPos;
	public bool isActive;
	public float startTime;
	public float endTime;
	public bool isHolding;
}


public class TouchController : MonoBehaviour {
	
	TouchExt[] ExtTouches = new TouchExt[5];
	
	public int swipeDetectionCoef = 10;				//Devider of screen diagonal to define minimum swipe distance
													//1 = whole screen diagonal; 10 = 0.1 of screen diagonal
	
	public float maxSwipeTime = 0.8f;				//If touch time less than this value (seconds) a swipe may be detected
													//If not user probably just moves an object
	
	public float maxTapTime = 0.3f;					//If touch time less than this value (seconds) a tap may be detected
	
	public float minHoldTime = 0.7f;				//If finger is in static position longer than this time, "hold" will be detected 

	public bool enableObjectHold = false;			//If true, script will detect holds on objects in 3D space
	public bool enableObjectTap = false;			//If true, script will detect taps on objects in 3D space
								
													//Will detect:	
	public bool detectTaps = false;					//taps
	public bool detectSwipes = false;				//swipes
	public bool detectMoves = false;				//finger move
	public bool detectHolds = false;				//finger static

	public string objectInteractionTag;  			//Define tag for those objects which are tapable or holdable

	public List<GameObject> tapReceivers;			//Following objects will receive a SendMessage notification "TriggerTap(Vector2 tapPosition)" when screen tap detected 
	public List<GameObject> swipeReceivers;
	public List<GameObject> moveReceivers;
	public List<GameObject> holdReceivers;
	

	int swipeDelta;
	

	//list of game objects to notify
	//prevent from reaction on buttons - tag as interface
	//move = distance, speed

	void Start () {
		for (int i = 0; i < 5; i++) {
			ExtTouches[i] = new TouchExt();
		}
		
		if (swipeDetectionCoef == 0) {
			swipeDetectionCoef = 1;
		}

		Vector2 diagonal = new Vector2(Screen.width, Screen.height);

		swipeDelta = (int)Mathf.Round(diagonal.magnitude / 10) ;
	}
	
	void Update () {
		if (Input.touches.Length > 0) {
			foreach (Touch t in Input.touches) {
				switch (t.phase) {
					case TouchPhase.Began:
						TouchDetected(t);
					break;
					
					case TouchPhase.Ended:
						TouchEnded(t);
					break;
					
					case TouchPhase.Moved:
						if (detectMoves) {
							MoveDetected(t);
						}
					break;
					
					case TouchPhase.Stationary:
					if (detectHolds && (Input.touches.Length == 1)) {
							StationaryDetected(t);
						}
					break;
				}
			}
		}
	}
	
	void TouchDetected(Touch t) {
		ExtTouches[t.fingerId].id = t.fingerId;
		ExtTouches[t.fingerId].isActive = true;
		ExtTouches[t.fingerId].startPos = t.position;
		ExtTouches[t.fingerId].curPos = t.position;
		ExtTouches[t.fingerId].startTime = Time.time;
	}
	
	void TouchEnded(Touch t) {
		ExtTouches[t.fingerId].endPos = t.position;
		ExtTouches[t.fingerId].curPos = t.position;
		ExtTouches[t.fingerId].endTime = Time.time;

		float deltaTime = ExtTouches[t.fingerId].endTime - ExtTouches[t.fingerId].startTime;
		float deltaX = Mathf.Abs(ExtTouches[t.fingerId].endPos.x - ExtTouches[t.fingerId].startPos.x);
		float deltaY = Mathf.Abs(ExtTouches[t.fingerId].endPos.y - ExtTouches[t.fingerId].startPos.y);

		if (deltaTime <= maxSwipeTime && detectSwipes) {
			if (deltaX >= swipeDelta) {
				TriggerSwipe(t.fingerId, true);
			} else {
				if (deltaY >= swipeDelta) {
					TriggerSwipe(t.fingerId, false);
				}
			}
		}

		if (detectTaps) {
			if ((deltaTime <= maxTapTime) && (deltaX < swipeDelta*0.5) && (deltaY < swipeDelta*0.5)) {
				TriggerTap(t.position);
			}
		}

		ExtTouches[t.fingerId].isActive = false;
		ExtTouches[t.fingerId].isHolding = false;

		
	}
	
	void MoveDetected(Touch t) {
		ExtTouches[t.fingerId].curPos = t.position;
		switch (Input.touches.Length) {
			case 1:
				TriggerSingleMove(t.deltaTime, t.deltaPosition);
			break;
			case 2:
				TriggerDoubleMove(t.fingerId, t.deltaTime, t.deltaPosition);
			break;	
		}
	}
	
	void StationaryDetected(Touch t) {
		if (!ExtTouches[t.fingerId].isHolding && (Input.touches.Length == 1)) { 
			if ((Time.time - ExtTouches[t.fingerId].startTime) >= minHoldTime) {
				TriggerHold(t.position);
				ExtTouches[t.fingerId].isHolding = true;
			}
		}
	}

	void TriggerSingleMove(float deltaTime, Vector2 deltaPosition) {
		//Debug.Log("Single Move : " + deltaTime + " || " + deltaPosition);
	}

	void TriggerDoubleMove(int fId ,float deltaTime, Vector2 deltaPosition){
			Debug.Log("Double Move : " + fId + " || " + deltaTime + " || " + deltaPosition);
	}

	void TriggerHold(Vector2 position) {
		Debug.Log("Hold Detected");
	}

	void TriggerTap(Vector2 position) {
		Debug.Log("Tap Detected " + position);
	}

	void TriggerSwipe(int fId, bool isHorizontal) {

		float deltaX = ExtTouches[fId].startPos.x - ExtTouches[fId].endPos.x;
		float deltaY = ExtTouches[fId].startPos.y - ExtTouches[fId].endPos.y;

		if (isHorizontal) {
			if (deltaX > 0) {
				TriggerSwipeDirection("left");
			} else {
				TriggerSwipeDirection("right");
			}
		} else {
			if (deltaY > 0) {
				TriggerSwipeDirection("down");
			} else {
				TriggerSwipeDirection("up");
			}
		}
	}

	void TriggerSwipeDirection(string direction) {
		switch (direction) {
			case "up":
				Debug.Log("Swipe Up");
			break;

			case "down":
				Debug.Log("Swipe Down");
				break;

			case "left":
				Debug.Log("Swipe Left");
				break;

			case "right":
				Debug.Log("Swipe Right");
				break;
		}
	}
}
