# Get the directory of this file
current_dir = Path(__file__).parent
def recognize_faces():
    Main face recognition function using LBPH algorithm and Haar Cascades.
    Recognizes faces from trained model.
        # Load recognizer and trained model
        recognizer = cv2.face.LBPHFaceRecognizer_create()
        # Path to trained model
        model_path = current_dir / "models" / "trainer.yml"
        if not model_path.exists():
            print(f"Trained model not found at {model_path}")
            print("Please train the model first using train_model()")
        recognizer.read(str(model_path))
        # Load cascade classifier
        cascade_path = current_dir / "models" / "cascade.xml"
        if not cascade_path.exists():
            cascade_path = cv2.data.haarcascades + "haarcascade_frontalface_default.xml"
        faceCascade = cv2.CascadeClassifier(str(cascade_path))
        # Font for display
        font = cv2.FONT_HERSHEY_SIMPLEX
        # Number of persons and names (configure based on your training)
        num_persons = 2
        names = ['Unknown', 'Person1', 'Person2']  # Update with actual names
        # Open webcam
        cam = cv2.VideoCapture(0, cv2.CAP_DSHOW)
        cam.set(3, 640)  # Width
        cam.set(4, 480)  # Height
        # Min window size to recognize as face
        minW = 0.1 * cam.get(3)
        minH = 0.1 * cam.get(4)
        print("Starting face recognition. Press 'q' to quit.")
            ret, img = cam.read()
            if not ret:
                print("Failed to capture frame")
            converted_image = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
                scaleFactor=1.2,
                minNeighbors=5,
                minSize=(int(minW), int(minH)),
            for (x, y, w, h) in faces:
                cv2.rectangle(img, (x, y), (x+w, y+h), (0, 255, 0), 2)
                id, accuracy = recognizer.predict(converted_image[y:y+h, x:x+w])
                # Check if accuracy is less than 100 (0 is perfect match)
                if accuracy < 100:
                    name = names[id] if id < len(names) else "Unknown"
                    accuracy_pct = f"{round(100 - accuracy)}%"
                    name = "Unknown"
                cv2.putText(img, str(name), (x + 5, y - 5), font, 1, (255, 255, 255), 2)
                cv2.putText(img, str(accuracy_pct), (x + 5, y + h - 5), font, 1, (255, 255, 0), 1)
            cv2.imshow('Face Recognition - Press Q to exit', img)
            k = cv2.waitKey(10) & 0xff
            if k == ord('q') or k == 27:  # q or ESC
        print("Face recognition ended.")
        print(f"Error in face recognition: {e}")
def train_model(training_data_path=None):
    Train the face recognizer with training data.
    Args:
        training_data_path: Path to training data directory
        if training_data_path is None:
            training_data_path = current_dir / "training_data"
        print(f"Training with data from: {training_data_path}")
        face_detector = cv2.CascadeClassifier(
            cv2.data.haarcascades + "haarcascade_frontalface_default.xml"
        print("Training model... This may take a while")
        recognizer.train([], [])  # Placeholder
        model_path.parent.mkdir(parents=True, exist_ok=True)
        recognizer.write(str(model_path))
        print(f"Model trained and saved to {model_path}")
        print(f"Error training model: {e}")
