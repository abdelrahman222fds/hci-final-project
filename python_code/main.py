import threading
import json
import cv2
from EmotionDetector import EmotionDetector
from FaceID import FaceRecognitionSystem
from HandTracker import HandTracker
from MySockets import SocketServer
from TUIO import TUIOListener
from Camera import Camera


MSG_TYPE_SEPARATOR = "$"
MSG_DATA_SEPARATOR = "#"
MSG_START_END = "&"


hand_tracking_thread = None
emotion_detection_thread = None
hand_tracking_active = threading.Event()
emotion_detection_active = threading.Event()
like_detection_active = threading.Event()


def show_camera(camera:Camera):
    while True:
        frame = camera.get_frame()
        cv2.imshow("frame", frame)
        if cv2.waitKey(1) & 0xFF == ord("q"):
            break
    camera.release()
    cv2.destroyAllWindows()

def hand_tracking(camera:Camera, hand_tracker:HandTracker, socket_server:SocketServer):
    while hand_tracking_active.is_set():  
        hand_tracker.process_frame()
        coords = hand_tracker.get_index_finger_coordinates()
        if coords:
            message = f"{MSG_START_END}{MSG_TYPE_SEPARATOR}hand_tracking{MSG_DATA_SEPARATOR}{coords}{MSG_START_END}"
            socket_server.send(message)

def like_detection(camera:Camera, hand_tracker:HandTracker, socket_server:SocketServer):
    while like_detection_active.is_set(): 
        hand_tracker.process_frame()
        result = hand_tracker.detect_like_gesture() 
        if result:
            message = f"{MSG_START_END}{MSG_TYPE_SEPARATOR}like_detection{MSG_DATA_SEPARATOR}{result}{MSG_START_END}"
            socket_server.send(message)

def emotion_detection(camera:Camera, emotion_detector:EmotionDetector, socket_server:SocketServer):
    while emotion_detection_active.is_set():  
        emotions = emotion_detector.detect_emotion()
        if emotions:
            dominant_emotion = max(emotions, key=emotions.get)
            dominant_value = emotions[dominant_emotion]
            message = f"{MSG_START_END}{MSG_TYPE_SEPARATOR}emotion_detection{MSG_DATA_SEPARATOR}{dominant_emotion}{MSG_DATA_SEPARATOR}{dominant_value}{MSG_START_END}"
            socket_server.send(message)

def detect_tuio(tuio_listener:TUIOListener, socket_server:SocketServer):
    out = False
    while not out:
        tuio_data = tuio_listener.get_tuio_data()
        if tuio_data:
            for entry in tuio_data:
                if entry["type"] == "object":
                    message = f"{MSG_START_END}{MSG_TYPE_SEPARATOR}TUIO_object{MSG_DATA_SEPARATOR}{entry['class_id']}{MSG_DATA_SEPARATOR}{entry['x_pos']},{entry['y_pos']}{MSG_DATA_SEPARATOR}{entry['angle']}{MSG_START_END}"
                    socket_server.send(message)
                    out = True
                    break

def update_passed_levels(file_path, user_name, level_index):
    """
    Updates the passed levels for a user in the JSON file.

    Parameters:
        file_path (str): Path to the JSON file.
        user_name (str): Name of the user to update.
        level_index (int): The level index to add to the user's passed levels.
    """
    try:
        
        with open(file_path, 'r') as file:
            data = json.load(file)

        
        if user_name not in data:
            print(f"User '{user_name}' not found.")
            return

        
        if level_index not in data[user_name]["passed_levels"]:
            data[user_name]["passed_levels"].append(level_index)
            print(f"Added level {level_index} to {user_name}'s passed levels.")
        else:
            print(f"Level {level_index} is already in {user_name}'s passed levels.")

        
        with open(file_path, 'w') as file:
            json.dump(data, file, indent=4)

    except FileNotFoundError:
        print(f"File '{file_path}' not found.")
    except json.JSONDecodeError:
        print("Error decoding JSON. Please check the file format.")
    except Exception as e:
        print(f"An error occurred: {e}")

def main():
    
    camera = Camera(camera_index=0)
    camera.open()
    camera_thread = threading.Thread(target=show_camera, args=(camera,))
    camera_thread.start()
    
   
    server = SocketServer()
    server.start()

    
    message = ""

    face_recognition_system = FaceRecognitionSystem(camera)
    hand_tracker = HandTracker(camera)
    emotion_detector = EmotionDetector(camera)
    tuio_listener = TUIOListener(ip='127.0.0.1', port=3333)
    tuio_listener.start_server_thread()


    faceIdLoginChoice = "face_login"
    faceIdRegisterChoice = "face_register"
    detect_TUIO_choice = "detect_TUIO"
    end_Program_choice = "end_program"
    start_hand_tracking_choice = "start_hand_tracking"
    end_hand_tracking_choice = "end_hand_tracking"
    start_emotion_detection_choice = "start_emotion_detection"
    end_emotion_detection_choice = "end_emotion_detection"
    start_like_detection_choice = "start_like_detection"
    end_like_detection_choice = "end_like_detection"
    update_passed_levels_choice = "update_passed_levels"
    Start_Detecting_YOLO = "Start_Detecting_YOLO"
    End_Detecting_YOLO = "End_Detecting_YOLO"

    while True:
        received_message = server.receive()  
        if received_message:
            print(f"Received from client: {received_message}")

        
        if received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{faceIdLoginChoice}{MSG_START_END}":
            userName, details = face_recognition_system.login()
            message = f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{faceIdLoginChoice}{MSG_DATA_SEPARATOR}{userName}{MSG_DATA_SEPARATOR}{details}{MSG_START_END}"
            server.send(message)

        elif received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{faceIdRegisterChoice}{MSG_START_END}":
            userName = server.receive()  
            face_recognition_system.register_user(userName.split(MSG_START_END)[1].split(MSG_TYPE_SEPARATOR)[1], camera.get_frame())
            message = f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{faceIdRegisterChoice}{MSG_DATA_SEPARATOR}Registration Successful{MSG_START_END}"
            server.send(message)

        elif received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{detect_TUIO_choice}{MSG_START_END}":
            print("Waiting for TUIO object detection...")
            detect_tuio(tuio_listener, server)

        elif received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{start_hand_tracking_choice}{MSG_START_END}":
            if not hand_tracking_active.is_set():
                hand_tracking_active.set()
                global hand_tracking_thread
                hand_tracking_thread = threading.Thread(target=hand_tracking, args=(camera, hand_tracker, server))
                hand_tracking_thread.daemon = True
                hand_tracking_thread.start()
                server.send(f"{MSG_START_END}{MSG_TYPE_SEPARATOR}hand_tracking_started{MSG_START_END}")
        
        elif received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{end_hand_tracking_choice}{MSG_START_END}":
            if hand_tracking_active.is_set():
                hand_tracking_active.clear()
                if hand_tracking_thread:
                    hand_tracking_thread.join()  
                server.send(f"{MSG_START_END}{MSG_TYPE_SEPARATOR}hand_tracking_stopped{MSG_START_END}")

        elif received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{start_like_detection_choice}{MSG_START_END}":
            if not like_detection_active.is_set():
                like_detection_active.set()
                global like_detection_thread
                like_detection_thread = threading.Thread(target=like_detection, args=(camera, hand_tracker, server))
                like_detection_thread.daemon = True
                like_detection_thread.start()
                server.send(f"{MSG_START_END}{MSG_TYPE_SEPARATOR}like_detection_started{MSG_START_END}")

        elif received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{end_like_detection_choice}{MSG_START_END}":
            if like_detection_active.is_set():
                like_detection_active.clear()
                if like_detection_thread:
                    like_detection_thread.join()
                server.send(f"{MSG_START_END}{MSG_TYPE_SEPARATOR}like_detection_stopped{MSG_START_END}")

        elif received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{start_emotion_detection_choice}{MSG_START_END}":
            if not emotion_detection_active.is_set():
                emotion_detection_active.set()
                global emotion_detection_thread
                emotion_detection_thread = threading.Thread(target=emotion_detection, args=(camera, emotion_detector, server))
                emotion_detection_thread.daemon = True
                emotion_detection_thread.start()
                server.send(f"{MSG_START_END}{MSG_TYPE_SEPARATOR}emotion_detection_started{MSG_START_END}")

        elif received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{end_emotion_detection_choice}{MSG_START_END}":
            if emotion_detection_active.is_set():
                emotion_detection_active.clear()
                if emotion_detection_thread:
                    emotion_detection_thread.join()  
                server.send(f"{MSG_START_END}{MSG_TYPE_SEPARATOR}emotion_detection_stopped{MSG_START_END}")

        elif received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{end_Program_choice}{MSG_START_END}":
            if hand_tracking_active.is_set():
                hand_tracking_active.clear()
                if hand_tracking_thread:
                    hand_tracking_thread.join()
            if emotion_detection_active.is_set():
                emotion_detection_active.clear()
                if emotion_detection_thread:
                    emotion_detection_thread.join()
            camera_thread.join()
            server.stop()
            cv2.destroyAllWindows()
            break
        
        elif received_message == f"{MSG_START_END}{MSG_TYPE_SEPARATOR}{update_passed_levels_choice}{MSG_START_END}":
           
            
            update_details = server.receive()  
            if update_details:
                try:
                    
                    parts = update_details.split(MSG_START_END)[1].split(MSG_DATA_SEPARATOR)
                    print(parts)
                    if len(parts) >= 3 and parts[0] == f"${update_passed_levels_choice}":
                        user_name = parts[1]
                        level_index = int(parts[2])  
                        
                        
                        update_passed_levels("user_data.json", user_name, level_index)
                        
                        
                        server.send(f"{MSG_START_END}{MSG_TYPE_SEPARATOR}update_passed_levels_success{MSG_DATA_SEPARATOR}{user_name}{MSG_DATA_SEPARATOR}{level_index}{MSG_START_END}")
                    else:
                        raise ValueError("Invalid message format.")
                except Exception as e:
                    
                    error_message = f"Error processing update_passed_levels: {str(e)}"
                    print(error_message)
                    server.send(f"{MSG_START_END}{MSG_TYPE_SEPARATOR}update_passed_levels_failed{MSG_DATA_SEPARATOR}{error_message}{MSG_START_END}")
        
        received_message = ""
        

if __name__ == "__main__":
    main()