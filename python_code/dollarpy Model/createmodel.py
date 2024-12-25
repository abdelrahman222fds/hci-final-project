import csv
from dollarpy import Recognizer, Template, Point
import sys
import os
import pickle

sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

def load_gesture_from_csv(csv_file):
    gesture_points = []

    with open(csv_file, mode="r") as file:
        reader = csv.DictReader(file)

        currentFrame = ""
        for row in reader:
            if currentFrame != row["frame"]:
                gesture_points.append([])
                currentFrame = row["frame"]
            
            x = float(row["x"])
            y = float(row["y"])
            gesture_points[-1].append(Point(x, y))

    return gesture_points

def make_template(gesture_points:list, templateName:str, templates:list):
    for gesture in gesture_points:
       templates.append(Template(templateName, gesture))
    return templates
  

def main():
    csv_file_Dislike = "dollarpy Model/Dataset/Dislike.csv" 
    csv_file_Like = "dollarpy Model/Dataset/Like.csv"  


    gesture_points_Dislike = load_gesture_from_csv(csv_file_Dislike)
    gesture_points_Like = load_gesture_from_csv(csv_file_Like)

    templates = []
    templates = make_template(gesture_points_Dislike, "DisLike", templates)
    templates = make_template(gesture_points_Like, "Like", templates)

    
    recognizer = Recognizer(templates=templates)

    with open('recognizer.pkl', 'wb') as f:
        pickle.dump(recognizer, f)

    print("Recognizer saved to 'recognizer.pkl'")

    return recognizer


if __name__ == "__main__":
    main()
