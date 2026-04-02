🤖 Jarvis - Advanced Voice Assistant UI
Modern Futuristic Robot Interface
import threading
import time
from datetime import datetime
from PyQt5.QtWidgets import (
    QApplication, QMainWindow, QWidget, QVBoxLayout, QHBoxLayout,
    QLabel, QPushButton, QFrame, QSizePolicy
from PyQt5.QtCore import Qt, QTimer, pyqtSignal, QObject, QThread, QSize, QRect
from PyQt5.QtGui import QFont, QColor, QIcon, QPixmap, QPainter, QPen, QBrush
from PyQt5.QtCore import QSize as QtSize
class SignalEmitter(QObject):
    """Emit signals from threads"""
    status_changed = pyqtSignal(str)
    listening = pyqtSignal(bool)
    thinking = pyqtSignal(bool)
    speaking = pyqtSignal(bool)
    waveform_update = pyqtSignal(list)
class AnimatedCircle(QFrame):
    """Animated loading circle"""
    def __init__(self, parent=None, size=200, color="#00D4FF"):
        super().__init__(parent)
        self.size = size
        self.color = color
        self.angle = 0
        self.is_animating = False
        self.setFixedSize(size, size)
        self.setStyleSheet("background: transparent; border: none;")
        self.timer = QTimer()
        self.timer.timeout.connect(self.rotate)
    def start_animation(self):
        self.is_animating = True
        self.timer.start(30)
    def stop_animation(self):
        self.timer.stop()
        self.update()
    def rotate(self):
        self.angle = (self.angle + 6) % 360
    def paintEvent(self, event):
        if not self.is_animating:
        painter = QPainter(self)
        painter.setRenderHint(QPainter.Antialiasing, True)
        center = self.size // 2
        radius = self.size // 2 - 10
        # Outer circle
        pen = QPen(QColor(self.color))
        pen.setWidth(3)
        pen.setCapStyle(Qt.RoundCap)
        painter.setPen(pen)
        # Rotating arc
        from PyQt5.QtCore import QRect
        arc_rect = QRect(10, 10, self.size - 20, self.size - 20)
        painter.drawArc(arc_rect, self.angle * 16, 90 * 16)
        # Center dot
        painter.setBrush(QBrush(QColor(self.color)))
        painter.setPen(Qt.NoPen)
        painter.drawEllipse(center - 5, center - 5, 10, 10)
class WaveformVisualizer(QFrame):
    """Animated waveform visualizer"""
    def __init__(self, parent=None):
        self.bars = 20
        self.heights = [0] * self.bars
        self.target_heights = [0] * self.bars
        self.setFixedHeight(100)
        self.timer.timeout.connect(self.update_heights)
        self.timer.start(50)
    def set_waveform(self, values):
        """Update waveform from audio levels"""
        import random
        if len(values) > 0:
            self.target_heights = [max(0, min(80, v * 80)) for v in values[:self.bars]]
            self.target_heights = [random.randint(10, 80) for _ in range(self.bars)]
    def update_heights(self):
        # Smooth animation
        for i in range(self.bars):
            diff = self.target_heights[i] - self.heights[i]
            self.heights[i] += diff * 0.3
        width = self.width()
        height = self.height()
        bar_width = width / self.bars - 2
        for i, h in enumerate(self.heights):
            x = i * (bar_width + 2) + 1
            y = height - h - 10
            # Gradient color
            if h > 60:
                color = QColor("#FF003D")  # Red when high
            elif h > 40:
                color = QColor("#FF7600")  # Orange
                color = QColor("#00D4FF")  # Cyan
            painter.fillRect(int(x), int(y), int(bar_width), int(h), color)
class JarvisUI(QMainWindow):
    """Main Jarvis UI Window"""
    def __init__(self, jarvis_instance=None):
        super().__init__()
        self.jarvis = jarvis_instance
        self.signal_emitter = SignalEmitter()
        # State
        self.is_listening = False
        self.is_thinking = False
        self.is_speaking = False
        self.current_command = ""
        self.status_message = "Initializing..."
        # Setup UI
        self.setup_ui()
        self.setup_styles()
        self.start_idle_animation()
        # Connect signals
        self.signal_emitter.status_changed.connect(self.on_status_changed)
        self.signal_emitter.listening.connect(self.on_listening)
        self.signal_emitter.thinking.connect(self.on_thinking)
        self.signal_emitter.speaking.connect(self.on_speaking)
    def setup_ui(self):
        """Setup the UI"""
        self.setWindowTitle("🤖 JARVIS - Advanced Voice Assistant")
        self.setGeometry(100, 100, 1200, 800)
        self.setWindowIcon(QIcon("icon256.ico") if Path("icon256.ico").exists() else None)
        # Central widget
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        main_layout = QVBoxLayout()
        main_layout.setContentsMargins(20, 20, 20, 20)
        main_layout.setSpacing(20)
        # Header
        header = self.create_header()
        main_layout.addWidget(header)
        # Main robot visualization area
        content_layout = QHBoxLayout()
        # Left panel - Robot/Status
        left_panel = self.create_left_panel()
        content_layout.addWidget(left_panel, 3)
        # Right panel - Info/Waveform
        right_panel = self.create_right_panel()
        content_layout.addWidget(right_panel, 2)
        main_layout.addLayout(content_layout, 1)
        # Control buttons
        button_area = self.create_button_area()
        main_layout.addWidget(button_area)
        central_widget.setLayout(main_layout)
    def create_header(self):
        """Create header with title and status"""
        header = QFrame()
        header.setStyleSheet("""
            QFrame {
                background: qlineargradient(
                    x1:0, y1:0, x2:1, y2:0,
                    stop:0 #0a1929,
                    stop:1 #1a3a52
                );
                border: 2px solid #00D4FF;
                border-radius: 10px;
        """)
        header.setFixedHeight(80)
        layout = QHBoxLayout()
        layout.setContentsMargins(20, 10, 20, 10)
        layout.setSpacing(20)
        # Title
        title = QLabel("🤖 JARVIS")
        title_font = QFont("Segoe UI", 28)
        title_font.setBold(True)
        title.setFont(title_font)
        title.setStyleSheet("color: #00D4FF;")
        layout.addWidget(title)
        # Spacer
        layout.addStretch()
        # Status
        self.status_label = QLabel("System: READY")
        status_font = QFont("Segoe UI", 12)
        self.status_label.setFont(status_font)
        self.status_label.setStyleSheet("color: #00D4FF; font-weight: bold;")
        layout.addWidget(self.status_label)
        # Time
        self.time_label = QLabel()
        time_font = QFont("Segoe UI", 10)
        self.time_label.setFont(time_font)
        self.time_label.setStyleSheet("color: #00FF00;")
        layout.addWidget(self.time_label)
        header.setLayout(layout)
        # Update time
        self.timer.timeout.connect(self.update_time)
        self.timer.start(1000)
        self.update_time()
        return header
    def create_left_panel(self):
        """Create left panel with robot visualization"""
        panel = QFrame()
        panel.setStyleSheet("""
                    x1:0, y1:0, x2:1, y2:1,
                    stop:0 #0f1f3d,
                border-radius: 15px;
        layout = QVBoxLayout()
        layout.setAlignment(Qt.AlignCenter)
        # Animated circle (robot head)
        self.animated_circle = AnimatedCircle(size=250, color="#00D4FF")
        layout.addWidget(self.animated_circle, alignment=Qt.AlignCenter)
        # Current command display
        self.command_label = QLabel("Listening...")
        command_font = QFont("Consolas", 11)
        self.command_label.setFont(command_font)
        self.command_label.setStyleSheet("""
            color: #00FF00;
            background: rgba(0, 0, 0, 0.5);
            border: 1px solid #00D4FF;
            border-radius: 5px;
            padding: 10px;
            text-align: center;
        self.command_label.setWordWrap(True)
        layout.addWidget(self.command_label)
        # State indicators
        state_layout = QHBoxLayout()
        state_layout.setSpacing(15)
        state_layout.setContentsMargins(10, 10, 10, 10)
        self.listening_indicator = self.create_indicator("🎤 LISTEN", "#00FF00")
        self.thinking_indicator = self.create_indicator("⚙️ THINK", "#FFB700")
        self.speaking_indicator = self.create_indicator("🔊 SPEAK", "#FF003D")
        state_layout.addWidget(self.listening_indicator)
        state_layout.addWidget(self.thinking_indicator)
        state_layout.addWidget(self.speaking_indicator)
        state_layout.addStretch()
        layout.addLayout(state_layout)
        panel.setLayout(layout)
        return panel
    def create_indicator(self, label, color):
        """Create status indicator"""
        indicator = QFrame()
        indicator.setStyleSheet(f"""
            QFrame {{
                background: rgba(0, 0, 0, 0.7);
                border: 2px solid {color};
                border-radius: 8px;
                padding: 8px 12px;
            }}
        layout.setContentsMargins(5, 5, 5, 5)
        label_widget = QLabel(label)
        label_widget.setFont(QFont("Segoe UI", 9, QFont.Bold))
        label_widget.setStyleSheet(f"color: {color};")
        indicator_dot = QLabel("●")
        indicator_dot.setFont(QFont("Arial", 12))
        indicator_dot.setStyleSheet(f"color: {color};")
        indicator_dot.setObjectName("indicator_dot")
        layout.addWidget(label_widget)
        layout.addWidget(indicator_dot, alignment=Qt.AlignCenter)
        indicator.setLayout(layout)
        indicator.setMaximumWidth(100)
        return indicator
    def create_right_panel(self):
        """Create right panel with info and waveform"""
        layout.setContentsMargins(15, 15, 15, 15)
        layout.setSpacing(15)
        title = QLabel("AUDIO")
        title.setFont(QFont("Segoe UI", 14, QFont.Bold))
        # Waveform
        self.waveform = WaveformVisualizer()
        layout.addWidget(self.waveform)
        layout.addSpacing(20)
        # System info
        info_title = QLabel("SYSTEM")
        info_title.setFont(QFont("Segoe UI", 12, QFont.Bold))
        info_title.setStyleSheet("color: #00D4FF;")
        layout.addWidget(info_title)
        # CPU/Memory info
        self.cpu_label = QLabel("CPU: 0%")
        self.cpu_label.setFont(QFont("Consolas", 10))
        self.cpu_label.setStyleSheet("color: #00FF00;")
        layout.addWidget(self.cpu_label)
        self.memory_label = QLabel("Memory: 0 MB")
        self.memory_label.setFont(QFont("Consolas", 10))
        self.memory_label.setStyleSheet("color: #00FF00;")
        layout.addWidget(self.memory_label)
        self.uptime_label = QLabel("Uptime: 00:00:00")
        self.uptime_label.setFont(QFont("Consolas", 10))
        self.uptime_label.setStyleSheet("color: #00FF00;")
        layout.addWidget(self.uptime_label)
        # Start system monitor
        self.start_monitor()
    def create_button_area(self):
        """Create control buttons"""
        button_area = QFrame()
        button_area.setStyleSheet("""
                background: transparent;
                border: none;
        button_area.setFixedHeight(60)
        layout.setContentsMargins(0, 10, 0, 0)
        layout.setSpacing(10)
        # Start button
        self.start_btn = self.create_control_button(
            "🎙️ START",
            "#00D4FF",
            lambda: self.start_assistant()
        layout.addWidget(self.start_btn)
        # Stop button
        self.stop_btn = self.create_control_button(
            "⏹️  STOP",
            "#FF003D",
            lambda: self.stop_assistant()
        self.stop_btn.setEnabled(False)
        layout.addWidget(self.stop_btn)
        # Clear button
        self.clear_btn = self.create_control_button(
            "🗑️  CLEAR",
            "#FFB700",
            lambda: self.clear_display()
        layout.addWidget(self.clear_btn)
        # Exit button
        self.exit_btn = self.create_control_button(
            "❌ EXIT",
            lambda: self.close()
        layout.addWidget(self.exit_btn)
        button_area.setLayout(layout)
        return button_area
    def create_control_button(self, text, color, callback):
        """Create styled control button"""
        btn = QPushButton(text)
        btn.setFont(QFont("Segoe UI", 11, QFont.Bold))
        btn.setFixedSize(150, 40)
        btn.setStyleSheet(f"""
            QPushButton {{
                    x1:0, y1:0, x2:0, y2:1,
                    stop:0 {color},
                    stop:1 {self.darken_color(color)}
                color: white;
                font-weight: bold;
            QPushButton:hover {{
                background: {color};
                border: 3px solid white;
            QPushButton:pressed {{
                background: {self.darken_color(color)};
            QPushButton:disabled {{
                background: #444;
                border: 2px solid #666;
                color: #888;
        btn.clicked.connect(callback)
        return btn
    def darken_color(self, color):
        """Darken a hex color"""
        # Simple darkening
        return color.replace("D4", "80").replace("FF", "AA").replace("003D", "0019")
    def setup_styles(self):
        """Setup global styles"""
        self.setStyleSheet("""
            QMainWindow {
                    stop:0 #0a1f3d,
                    stop:1 #1a2a3a
    def start_idle_animation(self):
        """Start idle animation"""
        self.animated_circle.start_animation()
    def update_time(self):
        """Update time display"""
        current_time = datetime.now().strftime("%H:%M:%S")
        self.time_label.setText(f"🕑 {current_time}")
    def start_assistant(self):
        """Start Jarvis"""
        self.is_listening = True
        self.status_label.setText("System: LISTENING")
        self.status_label.setStyleSheet("color: #00FF00; font-weight: bold;")
        self.command_label.setText("🎤 Listening for command...")
        self.start_btn.setEnabled(False)
        self.stop_btn.setEnabled(True)
        self.listening_indicator.findChild(QLabel, "indicator_dot").setVisible(True)
        # Start waveform simulation
        self.start_waveform_animation()
    def stop_assistant(self):
        """Stop Jarvis"""
        self.status_label.setText("System: READY")
        self.command_label.setText("System standby")
        self.start_btn.setEnabled(True)
        self.listening_indicator.findChild(QLabel, "indicator_dot").setVisible(False)
        self.animated_circle.stop_animation()
    def clear_display(self):
        """Clear display"""
        self.command_label.setText("Display cleared")
        self.waveform.target_heights = [0] * self.waveform.bars
    def start_waveform_animation(self):
        """Animate waveform"""
        self.waveform.set_waveform([random.random() for _ in range(20)])
        if self.is_listening:
            QTimer().singleShot(100, self.start_waveform_animation)
    def on_status_changed(self, status):
        """Handle status change"""
        self.status_label.setText(f"System: {status}")
    def on_listening(self, state):
        """Handle listening state"""
        if state:
            self.start_assistant()
            self.stop_assistant()
    def on_thinking(self, state):
        """Handle thinking state"""
            self.is_thinking = True
            self.status_label.setText("System: PROCESSING")
            self.status_label.setStyleSheet("color: #FFB700; font-weight: bold;")
    def on_speaking(self, state):
        """Handle speaking state"""
            self.is_speaking = True
            self.status_label.setText("System: SPEAKING")
    def start_monitor(self):
        """Start system monitoring"""
        self.start_time = time.time()
        def update_stats():
                cpu = psutil.cpu_percent(interval=0.1)
                memory = psutil.virtual_memory().used / 1024 / 1024
                uptime = int(time.time() - self.start_time)
                self.cpu_label.setText(f"CPU: {cpu:.1f}%")
                self.memory_label.setText(f"Memory: {memory:.0f} MB")
                hours = uptime // 3600
                minutes = (uptime % 3600) // 60
                seconds = uptime % 60
                self.uptime_label.setText(f"Uptime: {hours:02d}:{minutes:02d}:{seconds:02d}")
            except:
                pass
                QTimer().singleShot(1000, update_stats)
        QTimer().singleShot(0, update_stats)
def run_ui(jarvis_instance=None):
    """Run the UI"""
    app = QApplication(sys.argv)
    window = JarvisUI(jarvis_instance)
    window.show()
    sys.exit(app.exec_())
    run_ui()
