# Contributing to PrintBuddy3D

First off, thank you for considering contributing to PrintBuddy3D! It's people like you that make open-source software great. 

Whether you want to fix a bug, add a feature, or improve documentation, your help is welcome.

## 🐛 Found a Bug or 💡 Have a Feature Request?

Before writing code, please check the existing issues to see if someone has already reported the same problem or suggested the same idea.

* **If you found a bug:** Please open a new issue using our `Bug Report` template. Provide as much detail as possible, including logs and steps to reproduce.
* **If you have a feature idea:** Please open a new issue using our `Feature Request` template to discuss it before you start working on it. This ensures your hard work aligns with the project's direction.

## 💻 How to Contribute Code

If you want to contribute code (fix a bug or add a feature), please follow these steps:

### 1. Setup Your Development Environment
PrintBuddy3D is a C# project built with Avalonia UI. To run it locally, you will need:
* The latest **.NET SDK** (check `global.json` for the exact version if applicable).
* An IDE like Visual Studio, JetBrains Rider, or VS Code with C# extensions.

### 2. Fork and Branch
1. **Fork** the repository to your own GitHub account.
2. **Clone** the project to your local machine.
3. Create a **new branch** for your feature or bugfix. Use a descriptive name:
   * `git checkout -b feature/adding-new-printer-support`
   * `git checkout -b fix/temperature-reading-bug`

### 3. Make Your Changes
* Write clean, readable code.
* If you are modifying the UI, please ensure it works well within the Avalonia framework.
* Test your changes thoroughly (especially connections to Klipper/Marlin instances if you are modifying printer control logic).

### 4. Submit a Pull Request (PR)
1. Push your branch to your forked repository.
2. Open a Pull Request from your branch to the `main` branch of the official PrintBuddy3D repository.
3. In the PR description, explain clearly **what** you changed and **why**. If it fixes an open issue, link to it (e.g., `Fixes #12`).

## 📜 Code of Conduct

Please note that this project has a [Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project, you agree to abide by its terms. Be respectful and kind to others.

---
Thank you for your contribution! 🚀
