import os
import cv2
import copy
import sys

sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from Camera import Camera
from HandTracker import HandTracker

def ensure_frames_folder(folder_name):
    if not os.path.exists(folder_name):
        os.makedirs(folder_name)
        return 0
    files = [f for f in os.listdir(folder_name) if f.endswith(".jpg")]
    indices = [int(os.path.splitext(file)[0]) for file in files if file.split('.')[0].isdigit()]
    return max(indices, default=-1) + 1

if __name__ == "__main__":
    camera = Camera(camera_index=2) 
    hand_tracker = HandTracker(camera)
    camera.open()

    # folder_name = "dollarpy Model/Like"
    folder_name = "dollarpy Model/DisLike"


    index = ensure_frames_folder(folder_name)

    try:
        while True:
            frame = camera.get_frame()
            if frame is not None:
                annotated_frame = hand_tracker.process_frame()
                cv2.imshow("Camera Feed", annotated_frame)

                key = cv2.waitKey(1) & 0xFF
                if key == ord(' '):  
                    file_name = os.path.join(folder_name, f"{index}.jpg")
                    cv2.imwrite(file_name, frame)
                    print(f"Saved: {file_name}")
                    index += 1
                elif key == ord('q'):
                    break
    finally:
        camera.release()
        cv2.destroyAllWindows()
