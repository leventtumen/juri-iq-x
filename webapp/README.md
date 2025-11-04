# JuriIQ - Turkish Legal Search Engine

A modern, Google-style search engine for Turkish case law and legislation. Built with Bootstrap 5, featuring natural language processing, semantic search, and AI-powered summaries designed for high-end legal professionals.

## 🎨 Design Features

### Professional Color Scheme
- **Primary Colors**: Deep navy (#1a2332) and charcoal (#0f1419) for trust and authority
- **Accent Colors**: Gold (#d4af37) and bronze (#cd7f32) for prestige and elegance
- **Status Colors**: Success (green), Warning (orange), Danger (red), Info (blue)

### Typography
- **Primary Font**: Inter - Clean, modern sans-serif for body text
- **Secondary Font**: Playfair Display - Elegant serif for headings
- **Responsive**: Scales appropriately across all devices

### UI Components
- **Sidebar Navigation**: Fixed sidebar with smooth transitions
- **Data Tables**: Sortable, filterable tables with pagination
- **Cards**: Professional cards with hover effects and shadows
- **Forms**: Clean form inputs with validation styling
- **Buttons**: Multiple button styles (primary, accent, outline)
- **Status Badges**: Color-coded status indicators

## 📱 Responsive Design

The application is fully responsive and optimized for:
- **Desktop**: 1920px, 1366px, 1024px
- **Tablet**: 768px, 1024px
- **Mobile**: 375px, 414px, and all mobile devices

## 🚀 Pages Included

1. **login.html** - Professional login page with gradient background
2. **index.html** - Dashboard with stats, recent cases, and calendar
3. **kararlar.html** - Decisions/cases data table with sorting and filtering
4. **profile.html** - User profile page with editable information

## 📁 File Structure

```
legal-app/
├── css/
│   ├── variables.css      # CSS custom properties and design tokens
│   ├── global.css         # Global styles and utilities
│   ├── components.css     # Reusable component styles
│   └── login.css          # Login page specific styles
├── js/
│   ├── main.js           # Main JavaScript functionality
│   └── login.js          # Login page JavaScript
├── index.html            # Dashboard page
├── login.html            # Login page
├── kararlar.html         # Decisions/cases page
├── profile.html          # Profile page
└── README.md             # This file
```

## 🛠️ Technologies Used

- **HTML5**: Semantic markup
- **CSS3**: Modern CSS with custom properties
- **Bootstrap 5.3**: Responsive grid and utilities
- **JavaScript (ES6+)**: Interactive functionality
- **Font Awesome 6.4**: Professional icons
- **Google Fonts**: Inter and Playfair Display

## 🎯 Key Features

### Navigation
- Fixed sidebar with smooth animations
- Mobile-responsive hamburger menu
- Active state indicators
- Breadcrumb navigation

### Data Management
- Sortable table columns
- Search functionality
- Pagination
- Status badges
- Action buttons

### User Experience
- Smooth transitions and animations
- Loading states
- Form validation
- Responsive design
- Touch-friendly on mobile

### Security
- Password visibility toggle
- Remember me functionality
- Secure form handling

## 🚦 Getting Started

### Option 1: Direct File Opening
1. Download all files maintaining the folder structure
2. Open `login.html` in your web browser
3. Navigate through the application

### Option 2: Local Server (Recommended)
```bash
# Using Python 3
python -m http.server 8000

# Using Node.js
npx http-server

# Then open http://localhost:8000 in your browser
```

### Option 3: Bootstrap Studio
1. Import the HTML files into Bootstrap Studio
2. The CSS and JS files will be automatically linked
3. Edit and customize as needed

## 🎨 Customization

### Colors
Edit `css/variables.css` to change the color scheme:
```css
:root {
  --primary-navy: #1a2332;
  --accent-gold: #d4af37;
  /* ... other variables */
}
```

### Typography
Change fonts in `css/variables.css`:
```css
:root {
  --font-primary: 'Your Font', sans-serif;
  --font-secondary: 'Your Heading Font', serif;
}
```

### Components
Modify component styles in `css/components.css`

## 📋 Turkish UI Labels

The interface uses Turkish language throughout:
- **Ana Sayfa**: Home/Dashboard
- **Kararlar**: Decisions/Cases
- **Mevzuatlar**: Legislation
- **Dosyalar**: Files
- **Müşteriler**: Clients
- **Takvim**: Calendar
- **Raporlar**: Reports
- **Ayarlar**: Settings
- **Profil**: Profile
- **Çıkış Yap**: Logout

## 🔒 Security Notes

This is a frontend template. For production use:
1. Implement proper backend authentication
2. Add CSRF protection
3. Use HTTPS
4. Implement proper session management
5. Add input sanitization
6. Use environment variables for sensitive data

## 📱 Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)
- Mobile browsers (iOS Safari, Chrome Mobile)

## 🤝 Contributing

This is a template project. Feel free to:
1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is provided as-is for use in your legal application.

## 🆘 Support

For questions or issues:
1. Check the code comments
2. Review the README
3. Inspect browser console for errors
4. Verify file paths are correct

## 🎉 Credits

- **Design**: Professional legal theme
- **Icons**: Font Awesome
- **Fonts**: Google Fonts (Inter, Playfair Display)
- **Framework**: Bootstrap 5

---

**Note**: This is a frontend template. Backend functionality needs to be implemented separately for a complete application.