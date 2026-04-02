import wikipedia
import webbrowser
import datetime
import os
import sys
from news import speak_news, getNewsUrl
from OCR import OCR
from diction import translate
from helpers import *
from youtube import youtube
from sys import platform
import getpass
# print(voices[0].id)
class Jarvis:
    def __init__(self) -> None:
        if platform == "linux" or platform == "linux2":
            self.chrome_path = '/usr/bin/google-chrome'
        elif platform == "darwin":
            self.chrome_path = 'open -a /Applications/Google\ Chrome.app'
        elif platform == "win32":
            self.chrome_path = 'C:\Program Files (x86)\Google\Chrome\Application\chrome.exe'
            print('Unsupported OS')
            exit(1)
        webbrowser.register(
            'chrome', None, webbrowser.BackgroundBrowser(self.chrome_path)
        )
    def wishMe(self) -> None:
        hour = int(datetime.datetime.now().hour)
        if hour >= 0 and hour < 12:
            speak("Good Morning SIR")
        elif hour >= 12 and hour < 18:
            speak("Good Afternoon SIR")
            speak('Good Evening SIR')
        weather()
        speak('I am JARVIS. Please tell me how can I help you SIR?')
    def sendEmail(self, to, content) -> None:
        server.login('email', 'password')
        server.sendmail('email', to, content)
    def execute_query(self, query):
        # TODO: make this more concise
        if 'wikipedia' in query:
            speak('Searching Wikipedia....')
            query = query.replace('wikipedia', '')
            results = wikipedia.summary(query, sentences=2)
            speak('According to Wikipedia')
            print(results)
            speak(results)
        elif 'youtube downloader' in query:
            exec(open('youtube_downloader.py').read())
        elif 'voice' in query:
            if 'female' in query:
                engine.setProperty('voice', voices[1].id)
            speak("Hello Sir, I have switched my voice. How is it?")
        if 'jarvis are you there' in query:
            speak("Yes Sir, at your service")
        if 'jarvis who made you' in query:
            speak("Yes Sir, my master build me in AI")
        elif 'open youtube' in query:
            webbrowser.get('chrome').open_new_tab('https://youtube.com')
        elif 'open amazon' in query:
            webbrowser.get('chrome').open_new_tab('https://amazon.com')
        elif 'cpu' in query:
            cpu()
        elif 'joke' in query:
            joke()
        elif 'screenshot' in query:
            speak("taking screenshot")
            screenshot()
        elif 'open google' in query:
            webbrowser.get('chrome').open_new_tab('https://google.com')
        elif 'open stackoverflow' in query:
            webbrowser.get('chrome').open_new_tab('https://stackoverflow.com')
        elif 'play music' in query:
            os.startfile("D:\\RoiNa.mp3")
        elif 'search youtube' in query:
            speak('What you want to search on Youtube?')
            youtube(takeCommand())
        elif 'the time' in query:
            strTime = datetime.datetime.now().strftime("%H:%M:%S")
            speak(f'Sir, the time is {strTime}')
        elif 'search' in query:
            speak('What do you want to search for?')
            search = takeCommand()
            url = 'https://google.com/search?q=' + search
            webbrowser.get('chrome').open_new_tab(
                url)
            speak('Here is What I found for' + search)
        elif 'location' in query:
            speak('What is the location?')
            location = takeCommand()
            url = 'https://google.nl/maps/place/' + location + '/&amp;'
            webbrowser.get('chrome').open_new_tab(url)
            speak('Here is the location ' + location)
        elif 'your master' in query:
            if platform == "win32" or "darwin":
                speak('Gaurav is my master. He created me couple of days ago')
            elif platform == "linux" or platform == "linux2":
                name = getpass.getuser()
                speak(name, 'is my master. He is running me right now')
        elif 'your name' in query:
            speak('My name is JARVIS')
        elif 'who made you' in query:
            speak('I was created by my AI master in 2021')
        elif 'stands for' in query:
            speak('J.A.R.V.I.S stands for JUST A RATHER VERY INTELLIGENT SYSTEM')
        elif 'open code' in query:
            if platform == "win32":
                os.startfile(
                    "C:\\Users\\gs935\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe")
            elif platform == "linux" or platform == "linux2" or "darwin":
                os.system('code .')
        elif 'shutdown' in query:
                os.system('shutdown /p /f')
                os.system('poweroff')
        elif 'your friend' in query:
            speak('My friends are Google assisstant alexa and siri')
        elif 'github' in query:
                'https://github.com/gauravsingh9356')
        elif 'remember that' in query:
            speak("what should i remember sir")
            rememberMessage = takeCommand()
            speak("you said me to remember"+rememberMessage)
            remember = open('data.txt', 'w')
            remember.write(rememberMessage)
            remember.close()
        elif 'do you remember anything' in query:
            remember = open('data.txt', 'r')
            speak("you said me to remember that" + remember.read())
        elif 'sleep' in query:
            sys.exit()
        elif 'dictionary' in query:
            speak('What you want to search in your intelligent dictionary?')
            translate(takeCommand())
        elif 'news' in query:
            speak('Ofcourse sir..')
            speak_news()
            speak('Do you want to read the full news...')
            test = takeCommand()
            if 'yes' in test:
                speak('Ok Sir, Opening browser...')
                webbrowser.open(getNewsUrl())
                speak('You can now read the full news from this website.')
                speak('No Problem Sir')
        elif 'email to gaurav' in query:
                speak('What should I say?')
                content = takeCommand()
                to = 'email'
                self.sendEmail(to, content)
                speak('Email has been sent!')
                speak('Sorry sir, Not able to send email at the moment')
def wakeUpJARVIS():
    bot_ = Jarvis()
    bot_.wishMe()
        query = takeCommand().lower()
        bot_.execute_query(query)
    recognizer = cv2.face.LBPHFaceRecognizer_create() # Local Binary Patterns Histograms
    recognizer.read('./Face-Recognition/trainer/trainer.yml')   #load trained model
    cascadePath = "./Face-Recognition/haarcascade_frontalface_default.xml"
    faceCascade = cv2.CascadeClassifier(cascadePath) #initializing haar cascade for object detection approach
    font = cv2.FONT_HERSHEY_SIMPLEX #denotes the font type
    id = 2 #number of persons you want to Recognize
    names = ['','Gaurav']  #names, leave first empty bcz counter starts from 0
    cam = cv2.VideoCapture(0, cv2.CAP_DSHOW) #cv2.CAP_DSHOW to remove warning
    cam.set(3, 640) # set video FrameWidht
    cam.set(4, 480) # set video FrameHeight
    # Define min window size to be recognized as a face
    minW = 0.1*cam.get(3)
    minH = 0.1*cam.get(4)
    # flag = True
        ret, img =cam.read() #read the frames using the above created object
        converted_image = cv2.cvtColor(img,cv2.COLOR_BGR2GRAY)  #The function converts an input image from one color space to another
        faces = faceCascade.detectMultiScale( 
            converted_image,
            scaleFactor = 1.2,
            minNeighbors = 5,
            minSize = (int(minW), int(minH)),
        for(x,y,w,h) in faces:
            cv2.rectangle(img, (x,y), (x+w,y+h), (0,255,0), 2) #used to draw a rectangle on any image
            id, accuracy = recognizer.predict(converted_image[y:y+h,x:x+w]) #to predict on every single image
            # Check if accuracy is less them 100 ==> "0" is perfect match 
            if (accuracy < 100):
                # Do a bit of cleanup
                speak("Optical Face Recognition Done. Welcome")
                cam.release()
                cv2.destroyAllWindows()
                wakeUpJARVIS()
                speak("Optical Face Recognition Failed")
                break;
import webbrowser as wb
# Check if PyAudio is available
    import pyaudio
    PYAUDIO_AVAILABLE = True
        import sounddevice
        PYAUDIO_AVAILABLE = False
        SOUNDDEVICE_AVAILABLE = True
        SOUNDDEVICE_AVAILABLE = False
# Add parent directory to path for module imports
current_dir = Path(__file__).parent.parent
sys.path.insert(0, str(current_dir))
# Import new modules
    from modules.computer_vision import ocr
    from modules.computer_vision.face_recognition import recognizer
    from modules.integrations import amazon_service
    MODULES_AVAILABLE = True
except ImportError as e:
    print(f"Warning: Some modules not available: {e}")
    MODULES_AVAILABLE = False
engine.setProperty('rate', 150)
engine.setProperty('volume', 1)
def time() -> None:
    """Tells the current time."""
    current_time = datetime.datetime.now().strftime("%I:%M:%S %p")
    speak("The current time is")
    speak(current_time)
    print("The current time is", current_time)
def date() -> None:
    """Tells the current date."""
    now = datetime.datetime.now()
    speak("The current date is")
    speak(f"{now.day} {now.strftime('%B')} {now.year}")
    print(f"The current date is {now.day}/{now.month}/{now.year}")
def wishme() -> None:
    """Greets the user based on the time of day."""
    speak("Welcome back, sir!")
    print("Welcome back, sir!")
    hour = datetime.datetime.now().hour
    if 4 <= hour < 12:
        speak("Good morning!")
        print("Good morning!")
    elif 12 <= hour < 16:
        speak("Good afternoon!")
        print("Good afternoon!")
    elif 16 <= hour < 24:
        speak("Good evening!")
        print("Good evening!")
        speak("Good night, see you tomorrow.")
    assistant_name = load_name()
    speak(f"{assistant_name} at your service. Please tell me how may I assist you.")
    print(f"{assistant_name} at your service. Please tell me how may I assist you.")
    """Takes a screenshot and saves it."""
    img_path = os.path.expanduser("~\\Pictures\\screenshot.png")
    img.save(img_path)
    speak(f"Screenshot saved as {img_path}.")
    print(f"Screenshot saved as {img_path}.")
def takecommand() -> str:
    """Takes microphone input from the user and returns it as text.
    Falls back to text input if PyAudio is not available."""
    # If PyAudio is not available, use text input fallback immediately
    if not PYAUDIO_AVAILABLE:
        print("\n⚠️  PyAudio not installed. Using text input mode instead.")
        print("(Type your command and press Enter)")
            query = input("You: ").strip()
            if query:
                print(f"Recognized: {query}")
                return query.lower()
        except KeyboardInterrupt:
            print(f"Error: {e}")
    # Try voice mode with proper error handling for PyAudio issues
        except (ImportError, AttributeError) as e:
            # PyAudio not available - fall back to text input
            print(f"\n⚠️  PyAudio initialization failed: {e}")
            print("Using text input mode instead.")
            print("🎤 Listening...")
                audio = r.listen(source, timeout=5)
            except sr.WaitTimeoutError:
                speak("Timeout occurred. Please try again.")
            print("🔄 Recognizing...")
            query = r.recognize_google(audio, language="en-in")
            print(f"📝 You said: {query}")
        except sr.UnknownValueError:
            speak("Sorry, I did not understand that.")
            print("Could not understand audio")
        except sr.RequestError as e:
            speak("Speech recognition service is unavailable.")
            print(f"Service error: {e}")
            speak(f"An error occurred: {e}")
        print(f"\n⚠️  Voice input error: {e}")
        print("Falling back to text input mode...")
def play_music(song_name=None) -> None:
    """Plays music from the user's Music directory."""
    song_dir = os.path.expanduser("~\\Music")
    songs = os.listdir(song_dir)
    if song_name:
        songs = [song for song in songs if song_name.lower() in song.lower()]
    if songs:
        song = random.choice(songs)
        os.startfile(os.path.join(song_dir, song))
        speak(f"Playing {song}.")
        print(f"Playing {song}.")
        speak("No song found.")
        print("No song found.")
def set_name() -> None:
    """Sets a new name for the assistant."""
    speak("What would you like to name me?")
    name = takecommand()
    if name:
        with open("assistant_name.txt", "w") as file:
            file.write(name)
        speak(f"Alright, I will be called {name} from now on.")
        speak("Sorry, I couldn't catch that.")
def load_name() -> str:
    """Loads the assistant's name from a file, or uses a default name."""
        with open("assistant_name.txt", "r") as file:
            return file.read().strip()
    except FileNotFoundError:
        return "Jarvis"  # Default name
def search_wikipedia(query):
    """Searches Wikipedia and returns a summary."""
        speak("Searching Wikipedia...")
        result = wikipedia.summary(query, sentences=2)
        speak(result)
        print(result)
    except wikipedia.exceptions.DisambiguationError:
        speak("Multiple results found. Please be more specific.")
    except Exception:
        speak("I couldn't find anything on Wikipedia.")
def recognize_face():
    """Recognizes faces using the face recognition module."""
    if not MODULES_AVAILABLE:
        speak("Face recognition module is not available.")
        speak("Starting face recognition...")
        recognizer.recognize_faces()
        speak("Face recognition completed.")
        speak(f"Error in face recognition: {str(e)}")
def extract_text_from_image():
    """Extracts text from an image using OCR."""
        speak("OCR module is not available.")
        speak("Starting text extraction...")
        result = ocr.extract_text()
        if result:
            speak(f"Extracted text: {result}")
            speak("No text found in the image.")
        speak(f"Error in OCR: {str(e)}")
def check_amazon_prices():
    """Checks Amazon prices and sends email."""
        speak("Amazon service module is not available.")
        speak("Checking Amazon prices...")
        amazon_service.check_prices()
        speak("Price check completed. Email sent if prices changed.")
        speak(f"Error checking prices: {str(e)}")
    wishme()
        query = takecommand()
        if not query:
            continue
        if "time" in query:
            time()
        elif "date" in query:
            date()
        elif "wikipedia" in query:
            query = query.replace("wikipedia", "").strip()
            search_wikipedia(query)
        elif "play music" in query:
            song_name = query.replace("play music", "").strip()
            play_music(song_name)
        elif "open youtube" in query:
            wb.open("youtube.com")
        elif "open google" in query:
            wb.open("google.com")
        elif "change your name" in query:
            set_name()
        elif "screenshot" in query:
            speak("I've taken screenshot, please check it")
        elif "tell me a joke" in query:
            joke = pyjokes.get_joke()
            speak(joke)
            print(joke)
        elif "face recognition" in query or "recognize face" in query:
            recognize_face()
        elif "extract text" in query or "ocr" in query:
            extract_text_from_image()
        elif "check prices" in query or "amazon prices" in query:
            check_amazon_prices()
        elif "shutdown" in query:
            speak("Shutting down the system, goodbye!")
            os.system("shutdown /s /f /t 1")
        elif "restart" in query:
            speak("Restarting the system, please wait!")
            os.system("shutdown /r /f /t 1")
        elif "offline" in query or "exit" in query:
            speak("Going offline. Have a good day!")
