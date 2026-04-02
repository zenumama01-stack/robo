recognizer.read('trainer/trainer.yml')   #load trained model
cascadePath = "haarcascade_frontalface_default.xml"
names = ['','avi']  #names, leave first empty bcz counter starts from 0
            id = names[id]
            accuracy = "  {0}%".format(round(100 - accuracy))
            id = "unknown"
        cv2.putText(img, str(id), (x+5,y-5), font, 1, (255,255,255), 2)
        cv2.putText(img, str(accuracy), (x+5,y+h-5), font, 1, (255,255,0), 1)  
    cv2.imshow('camera',img) 
    k = cv2.waitKey(10) & 0xff # Press 'ESC' for exiting video
    if k == 27:
print("Thanks for using this program, have a good day.")
