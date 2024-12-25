import cv2

class Camera:
    def __init__(self, camera_index=0):
        
        self.camera_index = camera_index
        self.cap = None
        self.is_open = False

    def open(self):
        if self.cap is None:
            self.cap = cv2.VideoCapture(self.camera_index)
        if self.cap.isOpened():
            self.is_open = True
            print("Camera opened successfully!")
        else:
            print("Error: Unable to open camera!")

    def get_frame(self):
        """ Returns the latest captured frame. """
        if self.is_open:
            ret, frame = self.cap.read()
            if ret:
                return frame
            else:
                print("Error: Failed to capture frame.")
                return None
        else:
            print("Camera not open.")
            return None

    def release(self):
        if self.cap is not None:
            self.cap.release()
            self.is_open = False
            print("Camera released.")


if __name__ == "__main__":
    camera = Camera(camera_index=1)  # Create camera object
    camera.open()     

    while True:
        frame = camera.get_frame()
        if frame is not None:
            cv2.imshow("Frame", frame)  # Display the frame

        if cv2.waitKey(1) & 0xFF == ord('q'): 
            break

    camera.release() 
    cv2.destroyAllWindows()  
