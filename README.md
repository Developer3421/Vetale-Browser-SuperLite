# Vetale Browser SuperLite

Vetale Browser SuperLite is a lightweight desktop web browser for Windows built with **WPF** and **.NET Framework 4.8**, using **CefSharp.Wpf (Chromium Embedded Framework)** as the web engine.

Designed for fast everyday browsing with a minimal UI, **DuckDuckGo Lite** search, a simple consent flow, and a safety-first navigation rule to reduce accidental redirects.

---

## Key features

### Lightweight browsing
- Lightweight WPF UI (**Windows 7–11** compatible)
- Chromium-based browsing via **CefSharp.Wpf**
- Basic navigation:
  - Back / Forward
  - Reload
  - Address bar navigation + search fallback

### DuckDuckGo Lite search
- Built-in search integration:
  - https://lite.duckduckgo.com/lite

### Popup / new window blocking
- Popups are prevented from opening as separate windows
- Navigation is kept inside the main browser window

### User Agreement flow
- First launch shows a **User Agreement** dialog
- Acceptance is stored locally so the consent buttons are not shown again

### Error handling UI
- Custom in-app error overlay (WPF UI) on load failures and HTTP errors
- Avoids silent failures or default engine error pages

---

## Safety behavior (anti-accidental redirects)

To reduce accidental navigation to suspicious or unintended domains, the address input logic follows this rule:

- If the input looks like a clear **URL/domain** → open it directly
- Otherwise → perform a **DuckDuckGo Lite** search

Additionally:
- non-web schemes are blocked
- only **http/https** are allowed for direct navigation

---

## Notes (security)

This project embeds a Chromium engine (CefSharp). Embedded browser components may have security considerations depending on version and platform limitations.

**Important:** the app is built using an embedded/older web component in some environments. Full security cannot be guaranteed. **Use at your own risk.**

---

## System requirements

- Windows 7 / 8 / 8.1 / 10 / 11
- **.NET Framework 4.8** installed
- **x64 build recommended** (CefSharp native dependencies)

---

## Build and run (Visual Studio)

1. Open the solution in **Visual Studio 2026**
2. Restore **NuGet** packages
3. Build **Release | x64**
4. Run the app

---

## External services

- DuckDuckGo Lite (search): https://lite.duckduckgo.com/lite
- Websites you open in the browser

Their own terms and privacy policies apply.

---

## User Agreement

On first launch, you must accept the included User Agreement to continue using the app.

- If accepted → the browser starts normally
- If declined → the app closes

Document:
- `VETALE_BROWSER_SUPERLITE_USER_AGREEMENT_EN.md`

---

## Contact

Developer: **vetalebrowser01@gmail.com**

---

## License
