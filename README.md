<h1> <p "font-size:200px;"><img align="left" src="PrintBuddy3D/Assets/PrintBuddy3D.png" width="100">PrintBuddy3D</p> </h1>
3D printer app for remote control, tracking filament and managing assembly guides.

# Overview
## 🚀 Main features
- Receive notification on home screen.
- Controlling klipper and marlin printers.
- SSH into klipper printer.
- Webmode as fallback if widgets fails for klipper printers.
- Adding filament and automatically track it.
- Custom layout with docking system for control widgets inside the app.
![Home page](/assets/image-2.png)
![Printer Page](/assets/image-1.png)
![Printer Control Page](/assets/image-3.png)
![Filament Page](/assets/image.png)
---
# 🔨 Installation
## Windows
`Dependencies: WebView2`

## Linux
`Dependencies: libgtk-3 and libwebkit2gtk-4.1`
>[!NOTE]
>Use this app with x11. Otherwise the app will crash because of the current implement of GtkWebView. This will probably be resolved in the future.

# ⚠️ Work in progress
- This project is currently in developing. Features will be implemented will be implemented as soon as possible.
## ✅ Working features
- [x] Receive notification on home screen.
- [x] Get printer status. (online/offline/printing/...)
- [x] Add klipper printers and connect to them.
- [x] SSH into klipper printer on Windows and Linux.
  - note: you need to have installed powershell on windows or one of these on linux: gnome-terminal, gnome-console, konsole, xfce4-terminal, xterm, mate-terminal, kitty, alacritty. 
- [x] Moving axis on klipper printers.
- [x] Set temperature on klipper printers.
- [x] Send commands using console to klipper printers.
- [x] Webmode as fallback if widgets fails for klipper printers.
- [x] Adding filament and manually update it.
- [x] Docking system for control widgets inside the app.
- [x] Documentation inside app. (basic troubleshooting, etc.)
  - needs to be finished on github wiki
- [x] Connect to the marlin printers.

## ⚒️ Features in development
- [ ] Get printer info. (axis position, temperature, etc.)
- [ ] Automatic fillament tracking.
- [ ] Adding widgets for controlling printer.
- [ ] Send print jobs.

# 🛠️ Maintenance and Support
- If you encounter bugs or need help, make a request here on git at issues.

# 🌟 Acknowledgements
- Thank you for using PrintBuddy3D! We appreciate your support and hope that the application will make it easier for you to manage your 3D printers.
