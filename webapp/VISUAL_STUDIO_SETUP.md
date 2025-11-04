# Visual Studio Code Setup and Git Pull Guide

## Method 1: Using VS Code's Built-in Git Features

### Step 1: Open Your Project in VS Code
1. Open Visual Studio Code
2. Click **File** → **Open Folder**
3. Navigate to your `juris-frontend` folder
4. Click **Select Folder**

### Step 2: Check Git Status
1. Look at the bottom-left corner of VS Code
2. You should see the current branch name (e.g., "main")
3. Click on the branch name to see all branches

### Step 3: Pull Changes Using VS Code UI
1. Click the **Source Control** icon in the left sidebar (looks like a branch/fork icon)
2. Click the **"..."** (three dots) menu at the top of the Source Control panel
3. Select **Pull** from the dropdown menu
4. VS Code will pull the latest changes from GitHub

### Step 4: If Pull Fails - Use VS Code Terminal
1. Open Terminal in VS Code: **Terminal** → **New Terminal** (or press Ctrl+`)
2. Run these commands:
```bash
git fetch origin
git checkout main
git pull origin main
```

### Step 5: Switch to Feature Branch (to see the changes)
In the VS Code terminal, run:
```bash
git checkout feature/ui-improvements
```

Or use the UI:
1. Click the branch name in the bottom-left corner
2. Select **feature/ui-improvements** from the list

---

## Method 2: Clone Fresh Repository in VS Code

### Step 1: Clone Repository
1. Open VS Code
2. Press **Ctrl+Shift+P** (Command Palette)
3. Type: **Git: Clone**
4. Enter: `https://github.com/huseyinarabaji-stack/juris-frontend.git`
5. Choose a folder location
6. Click **Open** when prompted

### Step 2: Switch to Feature Branch
1. Click the branch name in bottom-left (should say "main")
2. Select **feature/ui-improvements**

---

## Method 3: Using VS Code's Git Graph Extension (RECOMMENDED)

### Step 1: Install Git Graph Extension
1. Click the **Extensions** icon in left sidebar (or press Ctrl+Shift+X)
2. Search for: **Git Graph**
3. Click **Install** on "Git Graph" by mhutchie

### Step 2: View and Pull Changes
1. Click **Source Control** icon in left sidebar
2. Click **Git Graph** button at the top
3. You'll see a visual representation of all branches and commits
4. Right-click on **origin/main** or **origin/feature/ui-improvements**
5. Select **Pull**

---

## Method 4: Configure VS Code for GitHub Authentication

### Step 1: Sign in to GitHub in VS Code
1. Click the **Accounts** icon in the bottom-left corner (person icon)
2. Click **Sign in to Sync Settings**
3. Choose **Sign in with GitHub**
4. Follow the browser prompts to authenticate

### Step 2: Try Pulling Again
Once authenticated, try pulling changes again using any of the methods above.

---

## Method 5: Manual File Replacement in VS Code

If Git pull still doesn't work, you can manually update files:

### Step 1: Download Updated Files
1. Go to: https://github.com/huseyinarabaji-stack/juris-frontend/tree/feature/ui-improvements
2. Click on **login.html**
3. Click the **Raw** button
4. Press **Ctrl+A** to select all, then **Ctrl+C** to copy
5. In VS Code, open your local **login.html**
6. Press **Ctrl+A** to select all, then **Ctrl+V** to paste
7. Save the file (**Ctrl+S**)
8. Repeat for **kararlar.html**

---

## Running the Project in VS Code

### Method 1: Using Live Server Extension
1. Install **Live Server** extension by Ritwick Dey
2. Right-click on **index.html** or **login.html**
3. Select **Open with Live Server**
4. Your browser will open with the project running

### Method 2: Using Python HTTP Server
In VS Code terminal:
```bash
python -m http.server 8000
```
Then open browser to: http://localhost:8000

### Method 3: Using Node.js HTTP Server
In VS Code terminal:
```bash
npx http-server -p 8000
```
Then open browser to: http://localhost:8000

---

## Verify the Changes

After pulling/updating, check:

### 1. Open login.html
- Should see: **"Yapay Zeka Destekli Hukuki Arama Platformu"**
- Should NOT see: "Dosyalarınızı, müşterilerinizi..."
- Should NOT see: "Kapsamlı Dosya Yönetimi"

### 2. Open kararlar.html
- Table headers should be: **Taraflar, Yasa Madde, Tarih, Tür, Konu, Özet**
- Should see Turkish legal case examples

---

## Troubleshooting in VS Code

### Issue: "Git not found"
1. Install Git from: https://git-scm.com/download/win
2. Restart VS Code
3. Try again

### Issue: Authentication Failed
1. Open VS Code Terminal
2. Run: `gh auth login`
3. Follow the prompts
4. Try pulling again

### Issue: Merge Conflicts
1. VS Code will highlight conflicts in red/green
2. Click **Accept Current Change** or **Accept Incoming Change**
3. Save the file
4. In Source Control panel, click **Commit** then **Sync**

### Issue: Detached HEAD
In VS Code terminal:
```bash
git checkout main
git pull origin main
```

---

## VS Code Git Settings (Optional)

### Enable Auto-Fetch
1. **File** → **Preferences** → **Settings**
2. Search for: **git.autofetch**
3. Check the box to enable
4. VS Code will automatically check for updates

### Show Git Output
1. **View** → **Output**
2. Select **Git** from the dropdown
3. You'll see detailed Git operation logs

---

## Still Having Issues?

### Check Git Status in VS Code
1. Open Terminal in VS Code
2. Run:
```bash
git status
git remote -v
git branch -a
```

### Reset to Remote Version (Last Resort)
In VS Code terminal:
```bash
git fetch origin
git reset --hard origin/main
```
⚠️ WARNING: This will discard all local changes!

---

## Quick Reference: VS Code Git Shortcuts

- **Ctrl+Shift+G** - Open Source Control
- **Ctrl+`** - Open Terminal
- **Ctrl+Shift+P** - Command Palette (type "Git" to see all Git commands)
- **F5** - Start debugging (if configured)

---

Need more help? Let me know what error you're seeing in VS Code!