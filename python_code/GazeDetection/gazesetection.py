import cv2
import mediapipe as mp
import numpy as np
import matplotlib.pyplot as plt

# Initialize Face Mesh module
mp_face_mesh = mp.solutions.face_mesh
cap = cv2.VideoCapture(0)

# Heatmap storage
gaze_points = []

def relative(landmark, shape):
    """Convert normalized landmark to image coordinates."""
    return int(landmark.x * shape[1]), int(landmark.y * shape[0])

def gaze(frame, points):
    """Estimate gaze direction and project it onto the image."""
    global gaze_points

   
    image_points = np.array([
        relative(points.landmark[4], frame.shape),    
        relative(points.landmark[152], frame.shape),  
        relative(points.landmark[263], frame.shape), 
        relative(points.landmark[33], frame.shape),   
        relative(points.landmark[287], frame.shape),  
        relative(points.landmark[57], frame.shape)   
    ], dtype="double")

    # Define 3D model points for head pose estimation
    model_points = np.array([
        (0.0, 0.0, 0.0),      
        (0, -63.6, -12.5),     
        (-43.3, 32.7, -26),   
        (43.3, 32.7, -26),     
        (-28.9, -28.9, -24.1), 
        (28.9, -28.9, -24.1)   
    ], dtype="double")

    # Camera internals
    focal_length = frame.shape[1]
    center = (frame.shape[1] / 2, frame.shape[0] / 2)
    camera_matrix = np.array([
        [focal_length, 0, center[0]],
        [0, focal_length, center[1]],
        [0, 0, 1]
    ], dtype="double")

    dist_coeffs = np.zeros((4, 1))  # Assuming no lens distortion
    success, rotation_vector, translation_vector = cv2.solvePnP(
        model_points, image_points, camera_matrix, dist_coeffs, flags=cv2.SOLVEPNP_ITERATIVE
    )

    if not success:
        return

   
    gaze_distance = 100 
    gaze_point_3D = np.array([0, 0, gaze_distance], dtype="double")
    gaze_point_2D, _ = cv2.projectPoints(
        gaze_point_3D, rotation_vector, translation_vector, camera_matrix, dist_coeffs
    )

  
    nose_tip_2D = tuple(map(int, image_points[0]))
    gaze_tip_2D = tuple(map(int, gaze_point_2D[0][0]))
    cv2.line(frame, nose_tip_2D, gaze_tip_2D, (0, 255, 0), 2)

 
    gaze_points.append(gaze_tip_2D)

def display_heatmap(gaze_points, frame_shape):
    """Display a heatmap of gaze points."""
    heatmap = np.zeros((frame_shape[0], frame_shape[1]), dtype=np.float32)

    for point in gaze_points:
        if 0 <= point[1] < frame_shape[0] and 0 <= point[0] < frame_shape[1]:
            heatmap[point[1], point[0]] += 1

   
    heatmap = cv2.GaussianBlur(heatmap, (51, 51), 0)
    heatmap = heatmap / np.max(heatmap)

    
    plt.imshow(heatmap, cmap='hot', interpolation='nearest')
    plt.title("Gaze Heatmap")
    plt.axis('off')
    plt.show()

with mp_face_mesh.FaceMesh(
        max_num_faces=1,
        refine_landmarks=True,
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5) as face_mesh:
    while cap.isOpened():
        success, frame = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue

        frame.flags.writeable = False
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        results = face_mesh.process(rgb_frame)
        frame.flags.writeable = True

        if results.multi_face_landmarks:
            for face_landmarks in results.multi_face_landmarks:
                gaze(frame, face_landmarks)

        cv2.imshow('Gaze Estimation', frame)
        if cv2.waitKey(5) & 0xFF == 27:  
            break

cap.release()
cv2.destroyAllWindows()


if gaze_points:
    display_heatmap(gaze_points, frame.shape)
