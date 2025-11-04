@echo off
echo ========================================
echo Juri-IQ Backend System Startup Script
echo ========================================
echo.

REM Set project environment variables
set PROJECT_PATH=C:\projects\project-juri-iq
set BACKEND_PORT=5000
set FRONTEND_PORT=8000

REM Check if Python is installed (required for both backend and frontend)
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.8 or higher (required for both backend and frontend)
    pause
    exit /b 1
)

REM Note: Frontend is static HTML/CSS/JavaScript, no Node.js required

echo Creating project directories...
if not exist "%PROJECT_PATH%" (
    mkdir "%PROJECT_PATH%"
)

REM Copy project files to the specified location
echo Copying project files to %PROJECT_PATH%...
xcopy /E /I /Y "%~dp0*" "%PROJECT_PATH%"

REM Change to project directory
cd /d "%PROJECT_PATH%"

echo.
echo ========================================
echo Setting up Backend Environment
echo ========================================
echo Note: Frontend is static HTML/CSS/JavaScript - no Node.js required

REM Create virtual environment if it doesn't exist
if not exist "backend\venv" (
    echo Creating Python virtual environment...
    cd backend
    python -m venv venv
    cd ..
)

REM Activate virtual environment
echo Activating virtual environment...
call backend\venv\Scripts\activate.bat

REM Install backend dependencies
echo Installing backend Python packages...
pip install -r backend\requirements.txt

REM Download spaCy model
echo Downloading spaCy language model...
python -m spacy download en_core_web_sm

REM Install antiword for .doc support (if on Windows with WSL or similar)
echo Note: For .doc file support, ensure antiword is installed
echo On Windows with WSL: sudo apt-get install antiword

echo.
echo ========================================
echo Setting up Frontend Environment
echo ========================================

REM Use Python's built-in HTTP server for static frontend
echo Python HTTP server will be used for frontend

REM Initialize document processing on first run
echo Initializing database and processing documents...
cd backend
python -c "from app import create_app; app = create_app(); from services.document_processor import DocumentProcessor; import os; processor = DocumentProcessor(); processor.process_all_documents(os.path.join(os.path.dirname(os.path.dirname(__file__)), 'sample-documents'))"
cd ..

echo.
echo ========================================
echo Starting Services
echo ========================================

REM Start backend server in background
echo Starting backend server on port %BACKEND_PORT%...
start "Backend Server" cmd /k "cd /d %PROJECT_PATH%\backend && venv\Scripts\activate.bat && python app.py"

REM Wait a moment for backend to start
timeout /t 3 /nobreak >nul

REM Start frontend server in background
echo Starting frontend server on port %FRONTEND_PORT%...
start "Frontend Server" cmd /k "cd /d %PROJECT_PATH%\webapp && python -m http.server %FRONTEND_PORT%"

echo.
echo ========================================
echo Services Started Successfully!
echo ========================================
echo.
echo Backend API: http://localhost:%BACKEND_PORT%
echo Frontend: http://localhost:%FRONTEND_PORT%
echo.
echo Admin Login:
echo   Email: admin@test.com
echo   Password: Pass!2345
echo.
echo To stop the services, close the respective command windows.
echo.
echo Press any key to open the frontend in your default browser...
pause >nul

REM Open frontend in browser
start http://localhost:%FRONTEND_PORT%

echo.
echo Startup complete! The Juri-IQ system is now running.
echo.