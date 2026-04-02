#!/usr/bin/env python
"""
Build Jarvis as standalone Windows EXE using PyInstaller.
This script handles all the configuration and verification needed.
import subprocess
import shutil
from pathlib import Path
class JarvisBuilder:
    def __init__(self):
        self.project_root = Path(__file__).parent
        self.dist_dir = self.project_root / "dist"
        self.build_dir = self.project_root / "build"
    def print_header(self, text):
        """Print formatted header"""
        print("\n" + "="*60)
        print(f"  🔨 {text}")
        print("="*60 + "\n")
    def print_success(self, text):
        """Print success message"""
        print(f"✅ {text}")
    def print_error(self, text):
        """Print error message"""
        print(f"❌ {text}")
        sys.exit(1)
    def print_warning(self, text):
        """Print warning message"""
        print(f"⚠️  {text}")
    def step(self, text):
        """Print step number"""
        print(f"\n📍 {text}")
    def check_python_version(self):
        """Verify Python 3.8+"""
        self.step("Checking Python version...")
        version = sys.version_info
        if version.major < 3 or (version.major == 3 and version.minor < 8):
            self.print_error(f"Python 3.8+ required. Current: {version.major}.{version.minor}")
        self.print_success(f"Python {version.major}.{version.minor}.{version.micro}")
    def check_pyinstaller(self):
        """Check if PyInstaller is installed"""
        self.step("Checking PyInstaller...")
            import PyInstaller
            self.print_success(f"PyInstaller found: {PyInstaller.__path__[0]}")
        except ImportError:
            self.print_error("PyInstaller not installed. Run: pip install pyinstaller")
    def check_modules(self):
        """Verify all required modules can be imported"""
        self.step("Verifying required modules...")
        modules = {
            'pyttsx3': 'Text-to-Speech',
            'speech_recognition': 'Speech Recognition',
            'wikipedia': 'Wikipedia API',
            'cv2': 'OpenCV',
            'pytesseract': 'Tesseract OCR',
            'bs4': 'BeautifulSoup',
            'requests': 'Requests',
        }
        for module, description in modules.items():
                __import__(module)
                self.print_success(f"{description} ({module})")
                self.print_error(f"{description} ({module}) not found. Run: pip install -r requirements.txt")
    def check_tesseract(self):
        """Verify Tesseract is installed"""
        self.step("Checking Tesseract-OCR...")
        common_paths = [
            "C:\\Program Files\\Tesseract-OCR\\tesseract.exe",
            "C:\\Program Files (x86)\\Tesseract-OCR\\tesseract.exe",
        ]
        found = False
        for path in common_paths:
            if Path(path).exists():
                self.print_success(f"Tesseract found at: {path}")
                found = True
        if not found:
            self.print_warning("Tesseract-OCR not found in standard locations.")
            self.print_warning("Please install from: https://github.com/UB-Mannheim/tesseract/wiki")
            self.print_warning("Or update tesseract path in modules/computer_vision/ocr.py")
    def check_jarvis_script(self):
        """Verify jarvis.py exists and is valid"""
        self.step("Checking Jarvis script...")
        jarvis_py = self.project_root / "Jarvis" / "jarvis.py"
        if not jarvis_py.exists():
            self.print_error(f"jarvis.py not found at {jarvis_py}")
        self.print_success(f"Found jarvis.py: {jarvis_py}")
    def check_icon(self):
        """Verify icon file exists"""
        self.step("Checking icon...")
        icon_path = self.project_root / "icon256.ico"
        if icon_path.exists():
            self.print_success(f"Icon found: {icon_path}")
            return str(icon_path)
            self.print_warning("No icon found. EXE will use default icon.")
            return None
    def clean_previous_builds(self):
        """Clean up previous build artifacts"""
        self.step("Cleaning previous builds...")
        if self.build_dir.exists():
            shutil.rmtree(self.build_dir)
            self.print_success("Removed build directory")
        if self.dist_dir.exists():
            old_exe = self.dist_dir / "Jarvis.exe"
            if old_exe.exists():
                old_exe.unlink()
                self.print_success("Removed old Jarvis.exe")
    def run_pyinstaller(self, icon_path=None):
        """Run PyInstaller with optimal configuration"""
        self.step("Building EXE with PyInstaller...")
        # Build PyInstaller command
        cmd = [
            sys.executable,
            "-m", "PyInstaller",
            "--onefile",                    # Single executable
            "--windowed",                   # No console window
            "--name=Jarvis",                # EXE name
            f"--distpath={self.dist_dir}",
            f"--workpath={self.build_dir}",
            f"--specpath={self.project_root}",
            "--add-data=modules;modules",   # Include modules
            "--add-data=icon256.ico;.",     # Include icon
            "--collect-all=cv2",            # OpenCV
            "--collect-all=pyttsx3",        # pyttsx3
            "--collect-all=speech_recognition",  # speech_recognition
        # Add icon if available
        if icon_path:
            cmd.append(f"--icon={icon_path}")
        # Add Python script
        cmd.append(str(jarvis_py))
        print(f"\nRunning command:\n{' '.join(cmd)}\n")
            result = subprocess.run(cmd, check=True)
            return result.returncode == 0
        except subprocess.CalledProcessError as e:
            self.print_error(f"PyInstaller failed with error code {e.returncode}")
    def verify_exe(self):
        """Verify the built EXE exists"""
        self.step("Verifying built EXE...")
        exe_path = self.dist_dir / "Jarvis.exe"
        if exe_path.exists():
            size_mb = exe_path.stat().st_size / (1024 * 1024)
            self.print_success(f"✅ EXE built successfully!")
            self.print_success(f"Location: {exe_path}")
            self.print_success(f"Size: {size_mb:.1f} MB")
            return True
            self.print_error(f"EXE not found at {exe_path}")
    def build(self):
        """Run the complete build process"""
        self.print_header("JARVIS DESKTOP VOICE ASSISTANT - EXE BUILDER")
            # Verification phase
            self.print_header("PHASE 1: VERIFICATION")
            self.check_python_version()
            self.check_pyinstaller()
            self.check_modules()
            self.check_tesseract()
            self.check_jarvis_script()
            icon_path = self.check_icon()
            # Preparation phase
            self.print_header("PHASE 2: PREPARATION")
            self.clean_previous_builds()
            # Build phase
            self.print_header("PHASE 3: BUILDING EXE")
            success = self.run_pyinstaller(icon_path)
            if not success:
                self.print_error("PyInstaller build failed")
            self.print_header("PHASE 4: VERIFICATION")
            self.verify_exe()
            # Success message
            self.print_header("BUILD SUCCESSFUL! 🎉")
            print("\n📝 Next steps:")
            print("  1. Test the EXE: dist/Jarvis.exe")
            print("  2. Configure amazon_service.py with your email")
            print("  3. Distribute to other computers (no Python needed!)\n")
            self.print_error(f"Build failed: {str(e)}")
if __name__ == "__main__":
    builder = JarvisBuilder()
    builder.build()
