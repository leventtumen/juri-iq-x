# JuriIQ Animation

## What This Is

A clean, professional loading animation for the JuriIQ Legal Intelligence Platform.

## Features

- **Network Animation**: Golden particles connected with lines
- **Code Rain Effect**: Matrix-style falling code in background
- **JuriIQ Branding**: Letter-by-letter text reveal
- **Loading Bar**: Professional loading indicator
- **Auto-redirect**: Goes to dashboard after 5 seconds
- **Skip Button**: Users can skip anytime

## Files

- `jurisiq-animation.html` - The animation (single file, no dependencies)

## How to Use

1. Copy `jurisiq-animation.html` to your project
2. Update the redirect URL (line ~200):
   ```javascript
   window.location.href = 'ana-sayfa.html'; // Change this to your page
   ```
3. Use as login success screen or app splash screen

## Customization

### Change particle count:
```javascript
for (let i = 0; i < 100; i++) { // Change 100 to your number
```

### Change connection distance:
```javascript
if (dist < 150) { // Change 150 to adjust connections
```

### Change animation duration:
```javascript
setTimeout(() => {
    window.location.href = 'ana-sayfa.html';
}, 5000); // Change 5000 (5 seconds) to your duration
```

### Change colors:
```css
/* Find these in the CSS section */
rgba(212, 175, 55, 0.8) /* Golden color - change RGB values */
```

## Preview

Open `jurisiq-animation.html` in your browser to see it in action.

## That's It!

Simple, clean, professional. No brain animations, just a network effect with code rain.