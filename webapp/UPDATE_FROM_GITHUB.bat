@echo off
echo ========================================
echo  JurisIQ Frontend - Update from GitHub
echo ========================================
echo.

REM Get the current directory
set CURRENT_DIR=%cd%

echo Current directory: %CURRENT_DIR%
echo.

echo Step 1: Checking Git status...
git status
echo.

echo Step 2: Fetching latest changes from GitHub...
git fetch origin
echo.

echo Step 3: Checking current branch...
git branch
echo.

echo Step 4: Switching to main branch...
git checkout main
echo.

echo Step 5: Pulling changes from GitHub...
git pull origin main
echo.

echo ========================================
echo  Update Complete!
echo ========================================
echo.

echo Do you want to switch to the feature branch to see the new changes? (Y/N)
set /p SWITCH_BRANCH=

if /i "%SWITCH_BRANCH%"=="Y" (
    echo.
    echo Switching to feature/ui-improvements branch...
    git checkout feature/ui-improvements
    git pull origin feature/ui-improvements
    echo.
    echo You are now on the feature/ui-improvements branch!
)

echo.
echo ========================================
echo  Verification
echo ========================================
echo.
echo Please check:
echo 1. Open login.html - Should say "Yapay Zeka Destekli Hukuki Arama Platformu"
echo 2. Open kararlar.html - Should have "Taraflar, Yasa Madde, Tarih, Tur, Konu, Ozet" columns
echo.

pause