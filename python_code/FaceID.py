import cv2
import os
import json
from deepface import DeepFace
from Camera import Camera

class FaceRecognitionSystem:
    def __init__(self, camera: Camera, user_data_file="user_data.json"):
        
        self.camera = camera
        self.user_data_file = user_data_file
        
        if not os.path.exists(self.user_data_file):
            with open(self.user_data_file, "w") as f:
                json.dump({}, f)

    def load_user_data(self):
        with open(self.user_data_file, "r") as f:
            return json.load(f)

    def save_user_data(self, data):
        with open(self.user_data_file, "w") as f:
            json.dump(data, f, indent=4)

    def register_user(self, username, frame):
        user_data = self.load_user_data()

        if username in user_data:
            print(f"User '{username}' already exists.")
            return

        # Save the reference image
        ref_image_path = f"UserImages/{username}.jpg"
        cv2.imwrite(ref_image_path, frame)

        # Validate saved image
        if not os.path.exists(ref_image_path):
            print(f"Error: Failed to save reference image for '{username}'.")
            return

        # Add user to the JSON data
        user_data[username] = {
            "image_path": ref_image_path,
            "passed_levels": []
        }
        self.save_user_data(user_data)
        print(f"User '{username}' registered successfully.")

    def login(self):
        print("Attempting login...")

        for attempt in range(1, 3): 
            print(f"Login attempt {attempt}/10...")
            frame = self.camera.get_frame()

            if frame is None:
                print("Error: Failed to capture frame.")
                continue

            # Load user data
            user_data = self.load_user_data()

            for username, details in user_data.items():
                ref_image_path = details["image_path"]
                ref_image = cv2.imread(ref_image_path)

                if ref_image is None:
                    print(f"Error: Reference image for user '{username}' could not be loaded.")
                    continue

                try:
                    # Verify face match
                    if DeepFace.verify(frame, ref_image)["verified"]:
                        print(f"Login successful: {username}")
                        return username, details
                except Exception as e:
                    print(f"Error verifying face for user '{username}': {e}")
                    continue

        print("Login failed after 10 attempts.")
        return None, None

    def run(self):
        self.camera.open()

        try:
            while True:
                frame = self.camera.get_frame()
                if frame is not None:
                    
                    cv2.putText(frame, "Press 'r' to Register, 'l' to Login, 'q' to Quit", (10, 30),
                                cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)
                    cv2.imshow("Face Recognition System", frame)

                key = cv2.waitKey(1)
                if key == ord('q'):  
                    break
                elif key == ord('r'):  
                    username = input("Enter username for registration: ")
                    if frame is not None:
                        self.register_user(username, frame)
                    else:
                        print("Error: No frame captured for registration.")
                elif key == ord('l'):  
                    self.login()
        finally:
            self.camera.release()
            cv2.destroyAllWindows()


# Main program
if __name__ == "__main__":
    camera = Camera(camera_index=0)
    face_recognition_system = FaceRecognitionSystem(camera)
    face_recognition_system.run()
