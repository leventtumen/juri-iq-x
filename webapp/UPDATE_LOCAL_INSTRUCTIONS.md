# How to Update Your Local Folder After Merging

## After you merge the pull request on GitHub, follow these steps:

### Option 1: Using Git Command Line
```bash
# Navigate to your project folder
cd C:\path\to\juris-frontend

# Make sure you're on the main branch
git checkout main

# Pull the latest changes from GitHub
git pull origin main
```

### Option 2: Using GitHub Desktop (if you have it installed)
1. Open GitHub Desktop
2. Select your juris-frontend repository
3. Click "Fetch origin" button
4. Click "Pull origin" to download the changes

### Option 3: Using Visual Studio Code (if you use it)
1. Open your project folder in VS Code
2. Click on the Source Control icon (left sidebar)
3. Click the "..." menu
4. Select "Pull" to download the changes

## What Happens:
- Your local C drive folder will be updated with all the changes from the pull request
- The login page will show the new AI/NLP description
- The kararlar page will have the new legal case table structure

## Important Notes:
- Always pull changes after merging on GitHub
- If you have local uncommitted changes, Git will ask you to commit or stash them first
- The changes are safe - they won't overwrite any work you haven't committed

## Need Help?
If you encounter any issues pulling the changes, let me know and I can help troubleshoot!