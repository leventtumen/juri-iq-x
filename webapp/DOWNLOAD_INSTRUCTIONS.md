# 📥 How to Download and Update Your Files

## 🚀 QUICKEST METHOD: Direct Download from GitHub

### Option 1: Download Individual Files
1. **Login Page:** https://raw.githubusercontent.com/huseyinarabaji-stack/juris-frontend/feature/ui-improvements/login.html
   - Right-click → Save As → Replace your local `login.html`

2. **Kararlar Page:** https://raw.githubusercontent.com/huseyinarabaji-stack/juris-frontend/feature/ui-improvements/kararlar.html
   - Right-click → Save As → Replace your local `kararlar.html`

### Option 2: Download Entire Branch as ZIP
1. Go to: https://github.com/huseyinarabaji-stack/juris-frontend/tree/feature/ui-improvements
2. Click the green **"Code"** button
3. Click **"Download ZIP"**
4. Extract the ZIP file
5. Copy the files to your local `juris-frontend` folder

---

## 🔧 METHOD 2: Using Batch Scripts (Windows)

I've created automated scripts for you:

### A. Normal Update (UPDATE_FROM_GITHUB.bat)
1. Navigate to your `juris-frontend` folder
2. Double-click **UPDATE_FROM_GITHUB.bat**
3. The script will automatically pull changes from GitHub
4. Follow the on-screen prompts

### B. Emergency Fresh Clone (EMERGENCY_UPDATE.bat)
If normal update doesn't work:
1. Navigate to your `juris-frontend` folder
2. Double-click **EMERGENCY_UPDATE.bat**
3. This will:
   - Backup your current folder
   - Delete the old folder
   - Clone a fresh copy from GitHub
   - Switch to the updated branch

---

## 💻 METHOD 3: Git Bash Commands

Open Git Bash in your `juris-frontend` folder and run:

```bash
# Basic update
git fetch origin
git checkout main
git pull origin main

# Or get the feature branch with all updates
git checkout feature/ui-improvements
git pull origin feature/ui-improvements
```

**If that fails, try:**
```bash
# Stash your changes first
git stash
git pull origin main
git stash pop
```

**If everything fails:**
```bash
# Hard reset (WARNING: Discards local changes)
git fetch origin
git reset --hard origin/main
```

---

## 🎨 METHOD 4: Visual Studio Code

### Quick Steps:
1. Open VS Code
2. Open your `juris-frontend` folder
3. Click **Source Control** icon (left sidebar)
4. Click the **"..."** menu
5. Select **"Pull"**

### Detailed Guide:
See **VISUAL_STUDIO_SETUP.md** for complete VS Code instructions

---

## 📦 METHOD 5: GitHub Desktop

1. Open GitHub Desktop
2. Select your `juris-frontend` repository
3. Click **"Fetch origin"**
4. Click **"Pull origin"**
5. If it asks which branch, select **"feature/ui-improvements"**

---

## ✅ Verify the Changes

After updating, check these files:

### 1. login.html
- ✅ Should say: **"Yapay Zeka Destekli Hukuki Arama Platformu"**
- ✅ Should say: **"Doğal dil işleme teknolojisi ile..."**
- ❌ Should NOT have: "Dosyalarınızı, müşterilerinizi..."
- ❌ Should NOT have: "Kapsamlı Dosya Yönetimi"

### 2. kararlar.html
- ✅ Table headers should be: **Taraflar | Yasa Madde | Tarih | Tür | Konu | Özet**
- ✅ Should have Turkish legal case examples (Ahmet Yılmaz vs. ABC Şirketi, etc.)
- ❌ Should NOT have: İsim, Pozisyon, Ofis, Yaş, Maaş

---

## 🆘 Still Having Problems?

### Common Issues:

**"Permission denied" or "Authentication failed"**
```bash
gh auth login
# Follow the prompts to re-authenticate
```

**"Your local changes would be overwritten"**
```bash
git stash
git pull origin main
```

**"Detached HEAD" or weird branch state**
```bash
git checkout main
git pull origin main
```

**Git Bash/GitHub Desktop not working at all**
- Use **Option 1** (Direct Download) - It's the most reliable!

---

## 📋 All Available Files

I've created these helper files for you:
1. **DOWNLOAD_INSTRUCTIONS.md** (this file)
2. **DOWNLOAD_AND_UPDATE_GUIDE.md** (detailed troubleshooting)
3. **GIT_BASH_COMMANDS.txt** (copy-paste commands)
4. **VISUAL_STUDIO_SETUP.md** (VS Code guide)
5. **UPDATE_FROM_GITHUB.bat** (automated update script)
6. **EMERGENCY_UPDATE.bat** (fresh clone script)
7. **UPDATE_LOCAL_INSTRUCTIONS.md** (general instructions)
8. **UPDATED_LOGIN.html** (backup copy of updated login page)
9. **UPDATED_KARARLAR.html** (backup copy of updated kararlar page)

---

## 🔗 Important Links

- **Pull Request:** https://github.com/huseyinarabaji-stack/juris-frontend/pull/2
- **View Changes:** https://github.com/huseyinarabaji-stack/juris-frontend/pull/2/files
- **Feature Branch:** https://github.com/huseyinarabaji-stack/juris-frontend/tree/feature/ui-improvements
- **Download ZIP:** https://github.com/huseyinarabaji-stack/juris-frontend/archive/refs/heads/feature/ui-improvements.zip

---

## 💡 Recommended Approach

**If you're not comfortable with Git:**
1. Use **Option 1** - Direct Download (easiest and most reliable)
2. Just download the two files and replace them in your folder

**If you want to learn Git:**
1. Try **METHOD 2** - Use the batch scripts
2. Or follow **METHOD 3** - Git Bash commands

**If you use VS Code:**
1. Follow **METHOD 4** - VS Code has great Git integration

---

Need more help? Let me know which method you tried and what error you're seeing!