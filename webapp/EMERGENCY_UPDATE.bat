@echo off
echo ========================================
echo  EMERGENCY UPDATE - Fresh Clone
echo ========================================
echo.
echo WARNING: This will create a fresh copy of the repository.
echo Your current folder will be backed up first.
echo.
echo Press any key to continue or close this window to cancel...
pause >nul
echo.

REM Get parent directory
cd ..
set PARENT_DIR=%cd%

echo Step 1: Creating backup...
if exist juris-frontend-backup (
    echo Removing old backup...
    rmdir /s /q juris-frontend-backup
)
echo Copying current folder to juris-frontend-backup...
xcopy juris-frontend juris-frontend-backup /E /I /H /Y
echo Backup created successfully!
echo.

echo Step 2: Removing old repository...
rmdir /s /q juris-frontend
echo.

echo Step 3: Cloning fresh copy from GitHub...
git clone https://github.com/huseyinarabaji-stack/juris-frontend.git
echo.

echo Step 4: Entering new repository...
cd juris-frontend
echo.

echo Step 5: Checking out feature branch with updates...
git checkout feature/ui-improvements
echo.

echo ========================================
echo  Fresh Clone Complete!
echo ========================================
echo.
echo Your old files are backed up in: %PARENT_DIR%\juris-frontend-backup
echo Your new updated files are in: %PARENT_DIR%\juris-frontend
echo.
echo Please verify the changes:
echo 1. Open login.html - Should say "Yapay Zeka Destekli Hukuki Arama Platformu"
echo 2. Open kararlar.html - Should have "Taraflar, Yasa Madde, Tarih, Tur, Konu, Ozet" columns
echo.

pause