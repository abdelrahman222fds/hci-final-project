import os
import cv2
import csv
import sys
import mediapipe as mp

def get_hand_landmarks_from_frame(frame):
    mp_hands = mp.solutions.hands
    hands = mp_hands.Hands(min_detection_confidence=0.5, min_tracking_confidence=0.5)

    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    result = hands.process(rgb_frame)

    if not result.multi_hand_landmarks:
        return None

    landmarks = result.multi_hand_landmarks[0]
    result = []
    for landmark in landmarks.landmark:
        x = landmark.x
        y = landmark.y
        z = landmark.z
        result.append((x, y, z))
    return result

def extract_hand_data_from_frames(input_folder, output_csv):

    with open(output_csv, mode="w", newline="") as file:
        writer = csv.writer(file)
        header = ["frame", "landmark_index", "x", "y", "z"]
        writer.writerow(header)

        for filename in sorted(os.listdir(input_folder), key=lambda x: int(os.path.splitext(x)[0])):
            if filename.endswith(".jpg"):
                frame_path = os.path.join(input_folder, filename)
                frame = cv2.imread(frame_path)
                if frame is None:
                    print(f"Error reading {filename}. Skipping.")
                    continue

                hand_data = get_hand_landmarks_from_frame(frame)
                if hand_data is None:
                    print(f"No hand landmarks detected in {filename}. Skipping.")
                    continue
                # Write hand data to the CSV file
                landmark_index = 0
                for landmark in hand_data:
                    x, y, z = landmark
                    writer.writerow([filename, landmark_index, x, y, z])
                    landmark_index += 1

    print(f"Hand data extracted and saved to {output_csv}.")




input_folder = "dollarpy Model/Dataset/Like"
output_csv = "dollarpy Model/Dataset/Like.csv"
extract_hand_data_from_frames(input_folder, output_csv)

input_folder = "dollarpy Model/Dataset/DisLike"
output_csv = "dollarpy Model/Dataset/DisLike.csv"
extract_hand_data_from_frames(input_folder, output_csv)



