# 🚀 How to Run JuriIQ Frontend Locally

## The Problem You're Facing

Visual Studio shows everything locked because this is a **web project** (HTML/CSS/JS), not a C# or .NET project. Visual Studio is designed for compiled applications, but this project just needs a web server.

---

## ✅ EASIEST METHOD: Visual Studio Code + Live Server

### Step 1: Install Visual Studio Code
1. Download from: https://code.visualstudio.com/
2. Install it (it's free and lightweight)

### Step 2: Install Live Server Extension
1. Open VS Code
2. Click the **Extensions** icon (left sidebar, looks like 4 squares)
3. Search for: **"Live Server"**
4. Click **Install** on "Live Server" by Ritwick Dey

### Step 3: Open Your Project
1. In VS Code: **File** → **Open Folder**
2. Select your `juris-frontend` folder
3. You'll see all files in the left sidebar

### Step 4: Run the Project
1. Right-click on **login.html** (or any HTML file)
2. Select **"Open with Live Server"**
3. Your browser will automatically open with the project running!
4. Any changes you make will auto-refresh in the browser

**That's it!** 🎉

---

## Alternative Methods

### Method 2: Using Python (If you have Python installed)

1. Open Command Prompt or Terminal
2. Navigate to your project folder:
   ```bash
   cd C:\path\to\juris-frontend
   ```
3. Run:
   ```bash
   python -m http.server 8000
   ```
4. Open browser to: http://localhost:8000

### Method 3: Using Node.js (If you have Node.js installed)

1. Open Command Prompt or Terminal
2. Navigate to your project folder:
   ```bash
   cd C:\path\to\juris-frontend
   ```
3. Run:
   ```bash
   npx http-server -p 8000
   ```
4. Open browser to: http://localhost:8000

### Method 4: Double-Click HTML Files (Quick Preview)

You can simply double-click any HTML file to open it in your browser:
- **login.html** - See the login page
- **kararlar.html** - See the kararlar table
- **case-detail.html** - See the case detail page
- **search-home.html** - See the search page

**Note:** Some features might not work perfectly this way (like form submissions), but you can see the design and layout.

---

## 🎯 Recommended Setup for Development

**Use VS Code + Live Server** because:
- ✅ Auto-refresh when you make changes
- ✅ Works like a real web server
- ✅ Easy to use
- ✅ Free and lightweight
- ✅ Great for HTML/CSS/JS projects

---

## 📁 Project Structure

```
juris-frontend/
├── login.html              ← Login page (START HERE)
├── search-home.html        ← Search homepage
├── search-results.html     ← Search results page
├── kararlar.html          ← Legal cases table
├── case-detail.html       ← Case detail page (UPDATED)
├── dashboard.html         ← Dashboard
├── profile.html           ← User profile
├── css/                   ← Stylesheets
│   ├── variables.css
│   ├── global.css
│   ├── components.css
│   └── ...
└── js/                    ← JavaScript files
    ├── login.js
    ├── search.js
    └── ...
```

---

## 🔍 To See Your Changes

### Login Page Changes:
1. Open **login.html** with Live Server
2. You should see: **"Yapay Zeka Destekli Hukuki Arama Platformu"**
3. You should NOT see: "Kapsamlı Dosya Yönetimi"

### Kararlar Page Changes:
1. Open **kararlar.html** with Live Server
2. Table headers should be: **Taraflar | Yasa Madde | Tarih | Tür | Konu | Özet**
3. You should see Turkish legal case examples

### Case Detail Page Changes (NEW):
1. Open **case-detail.html** with Live Server
2. Top bar should now match kararlar page style
3. Should have: Search box, notification bell, user menu with "Av. Ahmet Kaya"

---

## ❓ Why Visual Studio Doesn't Work

Visual Studio is designed for:
- C# / .NET applications
- Compiled projects
- Projects with a solution (.sln) file

Your project is:
- Pure HTML/CSS/JavaScript
- No compilation needed
- Just needs a web server to serve files

**That's why VS Code is better for this project!**

---

## 🆘 Still Having Issues?

### Can't install VS Code?
- Use Method 2 (Python) or Method 3 (Node.js)
- Or just double-click the HTML files

### Want to use Visual Studio anyway?
- You can open files in Visual Studio, but you'll need to:
  1. Install IIS Express
  2. Configure it as a web project
  3. It's much more complicated than VS Code

### Files still locked?
- That's just Visual Studio's way of showing files
- The files aren't actually locked
- Use VS Code instead for a better experience

---

## 📞 Quick Start Summary

1. **Download VS Code** (5 minutes)
2. **Install Live Server extension** (1 minute)
3. **Open your folder in VS Code** (30 seconds)
4. **Right-click login.html → Open with Live Server** (10 seconds)
5. **Done!** Your project is running on localhost! 🎉

---

**Total time to get running: Less than 10 minutes!**