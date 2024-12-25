import cv2
import pickle
from Camera import Camera
from dollarpy import Point
import mediapipe as mp

class GestureRecognizer:
    def __init__(self, model_path):
        """
        Initializes the GestureRecognizer with a pre-trained recognizer model.
        :param model_path: Path to the pickled recognizer model.
        """
        self.model_path = model_path
        self.recognizer = self.load_model()

    def load_model(self):
        """Loads the trained recognizer model from the pickle file."""
        with open(self.model_path, 'rb') as f:
            recognizer = pickle.load(f)
        print("Recognizer model loaded successfully.")
        return recognizer

    def predict_gesture(self, landmarks):
        """
        Predicts the gesture based on hand landmarks.
        :param landmarks: A list of tuples [(x1, y1), (x2, y2), ...].
        :return: Predicted gesture label or None if prediction is not possible.
        """
        points = [Point(x, y) for x, y, _ in landmarks]  # Convert landmarks to Point objects
        result = self.recognizer.recognize(points)
        if result:
            return result[0] # Return the name of the best-matching template
        return None


def get_hand_landmarks_from_frame(frame):
    """Detects hand landmarks from a given frame."""
    mp_hands = mp.solutions.hands
    hands = mp_hands.Hands(min_detection_confidence=0.5, min_tracking_confidence=0.5)

    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    result = hands.process(rgb_frame)

    if not result.multi_hand_landmarks:
        return None

    landmarks = result.multi_hand_landmarks[0]
    return [(lm.x, lm.y, lm.z) for lm in landmarks.landmark]


if __name__ == "__main__":
    # Initialize the Camera and GestureRecognizer
    camera = Camera(camera_index=2)
    camera.open()

    model_path = "recognizer.pkl"
    gesture_recognizer = GestureRecognizer(model_path)

    try:
        while True:
            frame = camera.get_frame()
            if frame is None:
                continue

            # Detect hand landmarks
            landmarks = get_hand_landmarks_from_frame(frame)
            if landmarks:
               
                gesture = gesture_recognizer.predict_gesture(landmarks)

                # Annotate the frame with the gesture result
                if gesture:
                    cv2.putText(frame, f"Gesture: {gesture}", (10, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)

            
            cv2.imshow("Gesture Recognition", frame)

           
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
    finally:
        camera.release()
        cv2.destroyAllWindows()
