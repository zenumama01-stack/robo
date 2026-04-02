🤖 JARVIS - Advanced Voice Assistant with UI
Main launcher with GUI support
import queue
# Add parent directory to path
sys.path.insert(0, str(Path(__file__).parent.parent))
# Import Jarvis
from Jarvis.jarvis import *
from jarvis_ui import JarvisUI, SignalEmitter
from PyQt5.QtCore import QThread, pyqtSignal
class JarvisWorker(QThread):
    """Run Jarvis in separate thread"""
    command_received = pyqtSignal(str)
    response_received = pyqtSignal(str)
        self.is_running = False
    def run(self):
        """Run Jarvis event loop"""
        self.is_running = True
        self.status_changed.emit("READY")
        # Simple event loop - can be enhanced with actual voice input
        while self.is_running:
            time.sleep(0.1)
    def stop(self):
class JarvisUIApp:
    """Main application with UI"""
        self.worker = JarvisWorker()
        self.ui = None
        """Start the application"""
        from PyQt5.QtWidgets import QApplication
        # Create UI
        self.ui = JarvisUI(jarvis_instance=self)
        self.ui.show()
        # Start worker thread
        self.worker.start()
        self.worker.status_changed.connect(self.ui.signal_emitter.status_changed)
        self.worker.listening.connect(self.ui.signal_emitter.listening)
        self.worker.thinking.connect(self.ui.signal_emitter.thinking)
        self.worker.speaking.connect(self.ui.signal_emitter.speaking)
def main():
    """Main entry point"""
    app = JarvisUIApp()
    app.run()
    main()
