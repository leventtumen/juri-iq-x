# GitHub Setup Guide for HukukPro

## Quick Start: Setting Up Your GitHub Repository

### Step 1: Create a New Repository on GitHub

1. Go to [GitHub](https://github.com) and log in
2. Click the "+" icon in the top right corner
3. Select "New repository"
4. Fill in the details:
   - **Repository name**: `hukukpro-frontend` (or your preferred name)
   - **Description**: "Professional legal management system frontend"
   - **Visibility**: Choose Private or Public
   - **DO NOT** initialize with README (we already have one)
5. Click "Create repository"

### Step 2: Initialize Git in Your Project

Open your terminal in the project directory and run:

```bash
# Initialize git repository
git init

# Add all files
git add .

# Create initial commit
git commit -m "Initial commit: Professional legal app redesign"

# Rename branch to main
git branch -M main

# Add your GitHub repository as remote (replace with your actual URL)
git remote add origin https://github.com/YOUR_USERNAME/hukukpro-frontend.git

# Push to GitHub
git push -u origin main
```

### Step 3: Verify Upload

1. Go to your GitHub repository URL
2. You should see all files uploaded
3. The README.md will be displayed on the repository homepage

## Working with GitHub - Common Commands

### Making Changes

```bash
# Check status of changes
git status

# Add specific files
git add filename.html

# Or add all changes
git add .

# Commit changes with message
git commit -m "Description of changes"

# Push to GitHub
git push
```

### Creating a Pull Request Workflow

```bash
# Create a new branch for your feature
git checkout -b feature/new-feature-name

# Make your changes and commit
git add .
git commit -m "Add new feature"

# Push branch to GitHub
git push -u origin feature/new-feature-name

# Then go to GitHub and create a Pull Request
```

### Pulling Latest Changes

```bash
# Get latest changes from GitHub
git pull origin main
```

## Recommended .gitignore File

Create a `.gitignore` file in your project root:

```
# Dependencies
node_modules/
.npm/
package-lock.json

# IDE
.vscode/
.idea/
*.swp
*.swo
*.sublime-project
*.sublime-workspace

# OS
.DS_Store
Thumbs.db
desktop.ini

# Logs
*.log
npm-debug.log*
yarn-debug.log*
yarn-error.log*

# Environment variables
.env
.env.local
.env.development.local
.env.test.local
.env.production.local

# Build outputs
dist/
build/
*.min.js
*.min.css

# Temporary files
tmp/
temp/
*.tmp

# Testing
coverage/
.nyc_output/

# Misc
.cache/
.parcel-cache/
```

## Branch Strategy

### Recommended Branches:

1. **main** - Production-ready code
2. **develop** - Development branch
3. **feature/*** - Feature branches
4. **hotfix/*** - Quick fixes

### Workflow:

```bash
# Start new feature
git checkout develop
git checkout -b feature/add-calendar-page

# Work on feature, commit changes
git add .
git commit -m "Add calendar page"

# Push to GitHub
git push -u origin feature/add-calendar-page

# Create Pull Request on GitHub
# After review and approval, merge to develop
# When ready for production, merge develop to main
```

## Collaboration Tips

### For Team Members:

1. **Clone the repository:**
```bash
git clone https://github.com/YOUR_USERNAME/hukukpro-frontend.git
cd hukukpro-frontend
```

2. **Create a branch for your work:**
```bash
git checkout -b feature/your-feature-name
```

3. **Make changes and commit:**
```bash
git add .
git commit -m "Your descriptive message"
git push -u origin feature/your-feature-name
```

4. **Create Pull Request on GitHub**

5. **After PR is merged, update your local:**
```bash
git checkout main
git pull origin main
```

## GitHub Pages Deployment (Optional)

To host your frontend on GitHub Pages:

1. Go to repository Settings
2. Scroll to "Pages" section
3. Under "Source", select "main" branch
4. Click "Save"
5. Your site will be available at: `https://YOUR_USERNAME.github.io/hukukpro-frontend/`

## Protecting Your Main Branch

1. Go to repository Settings
2. Click "Branches"
3. Add branch protection rule for "main"
4. Enable:
   - Require pull request reviews before merging
   - Require status checks to pass
   - Include administrators

## Useful GitHub Features

### Issues
- Track bugs and feature requests
- Assign to team members
- Use labels for organization

### Projects
- Create project boards
- Track progress with Kanban boards
- Link issues and pull requests

### Wiki
- Document your project
- Create guides and tutorials
- Share knowledge with team

## Commit Message Best Practices

Use clear, descriptive commit messages:

```bash
# Good examples:
git commit -m "Add user authentication to login page"
git commit -m "Fix responsive layout on mobile devices"
git commit -m "Update color scheme for better contrast"
git commit -m "Refactor data table component for better performance"

# Bad examples:
git commit -m "fix"
git commit -m "updates"
git commit -m "changes"
```

### Commit Message Format:

```
<type>: <subject>

<body (optional)>

<footer (optional)>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance tasks

**Example:**
```bash
git commit -m "feat: Add calendar page with event management

- Implement calendar view with month/week/day views
- Add event creation and editing functionality
- Integrate with existing data structure

Closes #123"
```

## Troubleshooting

### Problem: Push rejected
```bash
# Solution: Pull latest changes first
git pull origin main
# Resolve any conflicts
git push
```

### Problem: Merge conflicts
```bash
# Solution: Resolve conflicts manually
# 1. Open conflicted files
# 2. Look for <<<<<<< HEAD markers
# 3. Edit to keep desired changes
# 4. Remove conflict markers
# 5. Add and commit
git add .
git commit -m "Resolve merge conflicts"
git push
```

### Problem: Accidentally committed to wrong branch
```bash
# Solution: Cherry-pick to correct branch
git log  # Find commit hash
git checkout correct-branch
git cherry-pick <commit-hash>
```

## Next Steps

1. ✅ Create GitHub repository
2. ✅ Push initial code
3. ✅ Set up branch protection
4. ✅ Invite collaborators
5. ✅ Create first issue/task
6. ✅ Start working on features

---

**Need Help?**
- [GitHub Documentation](https://docs.github.com)
- [Git Cheat Sheet](https://education.github.com/git-cheat-sheet-education.pdf)
- [GitHub Learning Lab](https://lab.github.com/)