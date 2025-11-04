# Complete Guide to Download and Update Your Local Files

## Problem
You're unable to pull changes from GitHub using Git Bash or GitHub Desktop.

## Solution: Multiple Methods to Get the Updated Files

---

## Method 1: Direct Download from GitHub (EASIEST)

### Step 1: Download the Changed Files Directly
1. Go to: https://github.com/huseyinarabaji-stack/juris-frontend/pull/2/files
2. Click on each modified file:
   - **login.html**
   - **kararlar.html**
3. Click the "..." menu (three dots) on the right side
4. Select "Download file"
5. Replace the files in your local `C:\path\to\juris-frontend` folder

### Step 2: Or Download the Entire Branch as ZIP
1. Go to: https://github.com/huseyinarabaji-stack/juris-frontend/tree/feature/ui-improvements
2. Click the green "Code" button
3. Select "Download ZIP"
4. Extract and replace files in your local folder

---

## Method 2: Fix Git Bash Pull Issues

### Common Issues and Fixes:

#### Issue 1: "Your local changes would be overwritten"
```bash
# Save your current changes
git stash

# Pull the changes
git checkout main
git pull origin main

# Restore your changes if needed
git stash pop
```

#### Issue 2: "Permission denied" or Authentication Error
```bash
# Re-authenticate with GitHub
gh auth login

# Then try pulling again
git pull origin main
```

#### Issue 3: Detached HEAD or Wrong Branch
```bash
# Check current branch
git branch

# Switch to main branch
git checkout main

# Pull changes
git pull origin main
```

#### Issue 4: Merge Conflicts
```bash
# Reset to remote version (WARNING: This discards local changes)
git fetch origin
git reset --hard origin/main
```

---

## Method 3: Fresh Clone (RECOMMENDED if nothing else works)

### Step 1: Backup Your Current Work
```bash
# Copy your entire project folder to a backup location
# Example: Copy C:\juris-frontend to C:\juris-frontend-backup
```

### Step 2: Delete and Re-clone
```bash
# Navigate to parent directory
cd C:\

# Remove old folder (make sure you backed up first!)
rm -rf juris-frontend

# Clone fresh copy
gh repo clone huseyinarabaji-stack/juris-frontend

# Or use git clone
git clone https://github.com/huseyinarabaji-stack/juris-frontend.git
```

### Step 3: Get the Updated Branch
```bash
cd juris-frontend
git checkout feature/ui-improvements
```

---

## Method 4: Using Visual Studio Code

### Step 1: Open Project in VS Code
1. Open Visual Studio Code
2. File → Open Folder → Select your juris-frontend folder

### Step 2: Pull Changes
1. Click the Source Control icon (left sidebar, looks like a branch)
2. Click the "..." menu at the top
3. Select "Pull"
4. If it asks for branch, select "main" or "feature/ui-improvements"

### Step 3: If Pull Fails in VS Code
1. Open Terminal in VS Code (Terminal → New Terminal)
2. Run:
```bash
git fetch origin
git checkout main
git pull origin main
```

---

## Method 5: Manual File Update (If All Else Fails)

I've created downloadable versions of the changed files below. You can manually copy-paste the content:

### Files to Update:
1. **login.html** - See UPDATED_LOGIN.html in this repository
2. **kararlar.html** - See UPDATED_KARARLAR.html in this repository

---

## After Getting the Updates

### Verify the Changes:
1. Open `login.html` in a browser
2. Check that it says: **"Yapay Zeka Destekli Hukuki Arama Platformu"**
3. Open `kararlar.html` in a browser
4. Check that the table has: **Taraflar, Yasa Madde, Tarih, Tür, Konu, Özet**

---

## Still Having Issues?

### Check Git Configuration:
```bash
git config --list
git remote -v
```

### Re-link to GitHub:
```bash
git remote set-url origin https://github.com/huseyinarabaji-stack/juris-frontend.git
```

### Check GitHub Authentication:
```bash
gh auth status
```

---

## Need More Help?

Let me know which method you tried and what error message you're seeing, and I'll provide specific troubleshooting steps!