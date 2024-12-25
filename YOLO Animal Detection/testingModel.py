from ultralytics import YOLO
import cv2

# Load the trained YOLOv8 model
model = YOLO('yolov8n.pt')  # Replace 'best.pt' with your trained model file

# Open the webcam
cap = cv2.VideoCapture(0)  # Use 0 for the default camera, or replace with camera index

# Check if the camera opened successfully
if not cap.isOpened():
    print("Error: Could not open the camera.")
    exit()

# Process each frame from the webcam
while True:
    ret, frame = cap.read()
    if not ret:
        print("Error: Unable to read from the camera.")
        break

    # Predict using the YOLOv8 model
    results = model.predict(source=frame, save=False, conf=0.5, show=False)

    # Draw the detections on the frame
    annotated_frame = results[0].plot()

    # Display the frame
    cv2.imshow("YOLOv8 Detection", annotated_frame)

    # Press 'q' to exit the loop
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release the camera and close OpenCV windows
cap.release()
cv2.destroyAllWindows()
