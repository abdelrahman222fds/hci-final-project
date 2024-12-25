import cv2
import mediapipe as mp
from Gesture import GestureRecognizer, get_hand_landmarks_from_frame
from Camera import Camera

class HandTracker:
    def __init__(self, camera):
        """
        Initializes the HandTracker with a given Camera instance.
        :param camera: An instance of the Camera class.
        """
        self.camera = camera
        self.mp_hands = mp.solutions.hands
        self.hands = self.mp_hands.Hands(min_detection_confidence=0.5, min_tracking_confidence=0.5)
        self.mp_drawing = mp.solutions.drawing_utils
        self.current_frame = None
        self.current_landmarks = None
        model_path = "recognizer.pkl"
        self.gesture_recognizer = GestureRecognizer(model_path)

    def update_frame_and_landmarks(self):
        """
        Captures the current frame and detects hand landmarks.
        """
        frame = self.camera.get_frame()
        if frame is None:
            return None

        # Convert the frame to RGB (required by MediaPipe)
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        result = self.hands.process(rgb_frame)

        self.current_frame = frame
        self.current_landmarks = result.multi_hand_landmarks[0] if result.multi_hand_landmarks else None

    def get_index_finger_coordinates(self):
        """
        Extracts the pointer (index finger tip) coordinates from the detected hand landmarks.
        :return: A tuple (x, y) of index finger tip coordinates in pixel values, or None if no hand is detected.
        """
        if self.current_frame is None or self.current_landmarks is None:
            return None

        # Index finger tip landmark is at index 8
        index_finger_tip = self.current_landmarks.landmark[self.mp_hands.HandLandmark.INDEX_FINGER_TIP]

        # Convert normalized coordinates to pixel values
        h, w, _ = self.current_frame.shape
        x = index_finger_tip.x
        y = index_finger_tip.y

        if x > 0.75:
            x = 1
        elif x < 0.25:
            x = 0
        else:
            x = 0.5

        if y > 0.75:
            y = 1
        elif y < 0.25:
            y = 0
        else:
            y = 0.5

        return (x, y)

    def detect_like_gesture(self):
        """
        Detects a "like" or "dislike" hand gesture (thumbs-up or thumbs-down).
        :return: A string indicating the gesture ("like", "dislike"), or None if no gesture is detected.
        """
        if self.current_landmarks is None:
            return None

        # Get required landmarks
        thumb_tip = self.current_landmarks.landmark[self.mp_hands.HandLandmark.THUMB_TIP]
        thumb_ip = self.current_landmarks.landmark[self.mp_hands.HandLandmark.THUMB_IP]
        thumb_mcp = self.current_landmarks.landmark[self.mp_hands.HandLandmark.THUMB_MCP]

        # Check if other fingers are folded (compare tip.y and mcp.y for each finger)
        fingers_folded = all(
            self.current_landmarks.landmark[tip].y > self.current_landmarks.landmark[mcp].y
            for tip, mcp in [
                (self.mp_hands.HandLandmark.INDEX_FINGER_TIP, self.mp_hands.HandLandmark.INDEX_FINGER_MCP),
                (self.mp_hands.HandLandmark.MIDDLE_FINGER_TIP, self.mp_hands.HandLandmark.MIDDLE_FINGER_MCP),
                (self.mp_hands.HandLandmark.RING_FINGER_TIP, self.mp_hands.HandLandmark.RING_FINGER_MCP),
                (self.mp_hands.HandLandmark.PINKY_TIP, self.mp_hands.HandLandmark.PINKY_MCP),
            ]
        )

        if fingers_folded:
            if thumb_tip.y < thumb_ip.y < thumb_mcp.y:  # Thumbs-up condition
                return "like"
            elif thumb_tip.y > thumb_ip.y > thumb_mcp.y:  # Thumbs-down condition
                return "dislike"

      

        return None

    def process_frame(self):
        """
        Processes a single frame to detect the "like", "dislike" gesture and index finger coordinates.
        """
        self.update_frame_and_landmarks()
        if self.current_frame is None:
            return None

        frame = self.current_frame

        if self.current_landmarks:
            # Draw hand landmarks on the frame
            self.mp_drawing.draw_landmarks(frame, self.current_landmarks, self.mp_hands.HAND_CONNECTIONS)

            # Detect thumbs-up or thumbs-down gesture
            gesture = self.detect_like_gesture()
            if gesture == "like":
                cv2.putText(frame, "Thumbs-Up Detected!", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
            elif gesture == "dislike":
                cv2.putText(frame, "Thumbs-Down Detected!", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)

            # Get index finger coordinates and draw them
            index_finger_coords = self.get_index_finger_coordinates()

            direction = ""
            if index_finger_coords[0] == 0:
                direction = "left"
            elif index_finger_coords[0] == 1:
                direction = "right"
            elif index_finger_coords[1] == 0:
                direction = "up"
            elif index_finger_coords[1] == 1:
                direction = "down"
            else:
                direction = "center"
            cv2.putText(frame, f"Direction: {direction}", (10, 70), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2)

        return frame

    def release(self):
        """ Releases resources used by the HandTracker. """
        self.camera.release()
        cv2.destroyAllWindows()

# Example usage
if __name__ == "__main__":
    camera = Camera(camera_index=2)  # Create Camera instance
    camera.open() 

    hand_tracker = HandTracker(camera)  # Create HandTracker instance

    try:
        while True:
            frame = hand_tracker.process_frame()
            if frame is not None:
                cv2.imshow("Hand Tracker", frame)

            if cv2.waitKey(1) & 0xFF == ord('q'): 
                break
    finally:
        hand_tracker.release() 
