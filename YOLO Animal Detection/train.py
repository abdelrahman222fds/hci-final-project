from ultralytics import YOLO

# Load a pre-trained YOLOv8 model
model = YOLO('yolov8n.pt')  # Use 'n', 's', 'm', 'l', or 'x' for different model sizes

# Train the model
model.train(
    data='data.yaml',  # Path to your custom YAML file
    epochs=120,           # Number of training epochs
    imgsz=416,           # Image size for training
    batch=16,            # Batch size
    workers=8,           # Number hof dataloader workers
    device='cpu'         # Use 'cpu' for training
)
