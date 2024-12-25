import cv2
from deepface import DeepFace
from Camera import Camera

class EmotionDetector:
    def __init__(self, camera):
        """
        Initializes the EmotionDetector with a given Camera instance.
        :param camera: An instance of the Camera class.
        """
        self.camera = camera

    def detect_emotion(self):
        """
        Captures a frame, detects faces, and classifies the dominant emotion using DeepFace.
        :return: A dictionary containing the detected emotions and their probabilities, or None if no face is detected.
        """
        # Get the frame from the camera
        frame = self.camera.get_frame()
        if frame is None:
            return None

        try:
           
            analysis = DeepFace.analyze(frame, actions=['emotion'], enforce_detection=False)

            
            print("Analysis result:", analysis)

            
            if isinstance(analysis, list) and len(analysis) > 0:
               
                emotion_data = analysis[0]

                
                dominant_emotion = emotion_data.get('dominant_emotion', 'Unknown')
                emotion_confidence = emotion_data.get('emotion', {}).get(dominant_emotion, 0)

                
                cv2.putText(frame, f"{dominant_emotion}: {emotion_confidence:.2f}", 
                            (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2)

               
                if 'region' in emotion_data:
                    region = emotion_data['region']
                    if isinstance(region, dict):
                        x, y, w, h = region.get('x', 0), region.get('y', 0), region.get('w', 0), region.get('h', 0)
                        cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)

                

                return emotion_data['emotion']

            else:
                print("No faces detected or invalid analysis result.")
                return None

        except Exception as e:
            print(f"Error in emotion detection: {e}")
            return None

    def release(self):
        """ Releases resources used by the EmotionDetector. """
        self.camera.release()
        cv2.destroyAllWindows()


if __name__ == "__main__":
    camera = Camera(camera_index=0)  # Create Camera instance
    camera.open()  

    emotion_detector = EmotionDetector(camera)  # Create EmotionDetector instance

    try:
        while True:
            emotions = emotion_detector.detect_emotion()
            if emotions:
                print(f"Detected Emotions: {emotions}")

            if cv2.waitKey(1) & 0xFF == ord('q'):  
                break
    finally:
        emotion_detector.release() 
