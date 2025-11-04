# HukukPro - Legal NLP Search Engine

A professional, Google-like search engine for Turkish case law and legislation. Built with Natural Language Processing capabilities to make legal documents accessible to everyone - from lawyers to students to the general public.

## 🎯 Project Overview

HukukPro is a comprehensive legal search platform that allows users to search through thousands of Turkish court decisions (Kararlar) and legislation (Mevzuatlar) using natural language queries. The system uses semantic search to understand user intent and provides relevant results with text highlighting and PDF viewing capabilities.

## ✨ Key Features

### 🔍 Smart Search
- **Natural Language Processing**: Search using everyday language, not legal jargon
- **Semantic Search**: AI understands context and intent
- **Voice Search**: Speak your query in Turkish
- **Auto-suggestions**: Real-time search suggestions as you type
- **Search History**: Track and revisit previous searches

### 📊 Advanced Filtering
- **Type Filter**: Toggle between Kararlar (Decisions) and Mevzuatlar (Legislation)
- **Date Range**: Filter by specific time periods
- **Court Type**: Filter by Yargıtay, Danıştay, Anayasa Mahkemesi, etc.
- **Case Type**: Filter by İş Hukuku, Ticaret Hukuku, Aile Hukuku, etc.
- **Relevance Scoring**: See how relevant each result is to your query

### 📄 Results & Viewing
- **List View**: Clean, organized results display
- **Text Highlighting**: Search terms highlighted in results
- **PDF Viewer**: View full case documents in-browser
- **Case Metadata**: Esas No, Karar No, Date, Court information
- **Related Cases**: AI-suggested similar cases
- **AI Summary**: Plain language explanation of complex legal text

### 👤 User Features
- **Save Searches**: Bookmark searches for later
- **Save Cases**: Bookmark important cases
- **Share**: Share cases via link or social media
- **Export**: Download cases as PDF
- **Dashboard**: View search history and saved items

## 📱 Pages

### 1. Login Page (`login.html`)
Professional authentication with:
- Split-screen design
- Email/password login
- Social login options
- Password recovery

### 2. Search Home (`search-home.html`)
Google-like search interface with:
- Large search box with voice input
- Quick filters (All, Kararlar, Mevzuatlar)
- Popular searches
- Feature highlights

### 3. Search Results (`search-results.html`)
Comprehensive results page with:
- Sidebar filters (type, date, court, case type)
- List of results with highlighting
- Relevance scores
- Pagination
- Sort options

### 4. Case Detail (`case-detail.html`)
Full case viewer with:
- Case metadata and tags
- Full case text with highlighting
- PDF viewer
- AI summary in plain language
- Related cases
- Save, share, print, download options

### 5. Dashboard (`dashboard.html`)
User dashboard with:
- Search statistics
- Recent searches
- Saved cases
- Favorites
- Quick actions

## 🎨 Design Philosophy

### For High-End Clientele
- **Professional**: Navy and gold color scheme
- **Elegant**: Playfair Display + Inter typography
- **Trustworthy**: Clean, organized layouts
- **Modern**: Smooth animations and transitions

### For Non-Technical Users
- **Simple Language**: No legal jargon required
- **Clear UI**: Intuitive navigation
- **Helpful Guides**: Tooltips and examples
- **Accessible**: Works on all devices

## 🚀 Technology Stack

- **Frontend**: HTML5, CSS3, JavaScript (ES6+)
- **Framework**: Bootstrap 5.3
- **Icons**: Font Awesome 6.4
- **Fonts**: Google Fonts (Inter, Playfair Display)
- **Features**: 
  - Web Speech API for voice search
  - Clipboard API for sharing
  - LocalStorage for history
  - Responsive design

## 📂 File Structure

```
legal-search-engine/
├── css/
│   ├── variables.css      # Design tokens
│   ├── global.css         # Base styles
│   ├── components.css     # Reusable components
│   ├── search.css         # Search pages
│   ├── results.css        # Results page
│   ├── case-detail.css    # Case detail page
│   ├── dashboard.css      # Dashboard page
│   └── login.css          # Login page
├── js/
│   ├── main.js           # Core functionality
│   ├── search.js         # Search page logic
│   ├── results.js        # Results page logic
│   ├── case-detail.js    # Case detail logic
│   └── login.js          # Login logic
├── login.html            # Login page
├── search-home.html      # Search home
├── search-results.html   # Results page
├── case-detail.html      # Case viewer
├── dashboard.html        # User dashboard
└── README.md             # This file
```

## 🎯 User Personas

### 1. Lawyers
- Need quick access to case law
- Want to find precedents
- Need to cite cases accurately

### 2. Law Students
- Learning legal concepts
- Researching for assignments
- Need simple explanations

### 3. General Public
- Understanding their rights
- Researching legal issues
- No legal background

## 🔍 Search Examples

### Natural Language Queries:
- "iş hukuku kararları 2024"
- "boşanma davası içtihatları"
- "kira sözleşmesi feshi"
- "işe iade davası emsal kararlar"
- "tazminat hesaplama yöntemi"

### Advanced Searches:
- Filter by date: "Last 1 year"
- Filter by court: "Yargıtay 9. Hukuk Dairesi"
- Filter by type: "İş Hukuku"
- Combine filters for precise results

## 📊 Features Breakdown

### Search Capabilities
✅ Natural language processing
✅ Semantic search
✅ Voice search (Turkish)
✅ Auto-suggestions
✅ Search history
✅ Popular searches

### Filtering
✅ Kararlar/Mevzuatlar toggle
✅ Date range selection
✅ Court type filter
✅ Case type filter
✅ Relevance sorting

### Results Display
✅ List view with metadata
✅ Text highlighting
✅ Relevance scores
✅ Pagination
✅ Quick actions (save, share, download)

### Case Viewing
✅ Full case text
✅ PDF viewer
✅ Text highlighting
✅ AI summary
✅ Related cases
✅ Citation format

### User Management
✅ Search history
✅ Saved searches
✅ Bookmarked cases
✅ Favorites
✅ User dashboard

## 🎨 Color Scheme

```css
Primary Navy:    #1a2332  /* Trust, authority */
Primary Dark:    #0f1419  /* Depth */
Accent Gold:     #d4af37  /* Prestige */
Accent Bronze:   #cd7f32  /* Elegance */
Success:         #27ae60  /* Positive actions */
Warning:         #f39c12  /* Caution */
Danger:          #e74c3c  /* Errors */
Info:            #3498db  /* Information */
```

## 📱 Responsive Design

- **Desktop**: Full sidebar, multi-column layouts
- **Tablet**: Collapsible sidebar, adjusted grids
- **Mobile**: Hamburger menu, single column, touch-optimized

## 🚦 Getting Started

### Local Development
```bash
# Clone repository
git clone https://github.com/huseyinarabaji-stack/juris-frontend.git

# Navigate to directory
cd juris-frontend

# Start local server
python -m http.server 8000

# Open in browser
http://localhost:8000/search-home.html
```

### Production Deployment
1. Upload all files to web server
2. Configure backend API endpoints
3. Set up SSL certificate
4. Configure domain name
5. Test all features

## 🔗 Integration Points

### Backend API (To Be Implemented)
- `/api/search` - Search endpoint
- `/api/case/:id` - Get case details
- `/api/filters` - Get filter options
- `/api/user/history` - User search history
- `/api/user/saved` - User saved items

### Database Schema (Suggested)
- `cases` - Court decisions
- `legislation` - Laws and regulations
- `users` - User accounts
- `searches` - Search history
- `bookmarks` - Saved items

## 🎯 Future Enhancements

### Phase 2
- [ ] OCR for scanned documents
- [ ] Advanced analytics dashboard
- [ ] Collaborative features
- [ ] API for developers
- [ ] Mobile apps (iOS/Android)

### Phase 3
- [ ] AI-powered legal assistant
- [ ] Document comparison
- [ ] Case prediction
- [ ] Legal document generator
- [ ] Multi-language support

## 📄 License

This project is proprietary software for HukukPro.

## 🤝 Contributing

This is a private project. For questions or contributions, contact the development team.

## 📞 Support

For technical support or questions:
- Email: support@hukukpro.com
- Documentation: docs.hukukpro.com

---

**Built with ❤️ for the Turkish legal community**