# MultiBloxy 🎮🔓

MultiBloxy is a Windows application designed to manage and control multiple Roblox instances. It provides a convenient way to open multiple instances for different accounts and offers a variety of options to manage them efficiently.

![Total Downloads](https://img.shields.io/github/downloads/Zgoly/MultiBloxy/total?color=%231e90ff)
![Stars](https://img.shields.io/github/stars/Zgoly/MultiBloxy?color=%231e90ff)
![Forks](https://img.shields.io/github/forks/Zgoly/MultiBloxy?color=%231e90ff)

## Key Features 🌟
- **Single File Executable**: Easy to use with no installation required.
- **System Tray Integration**: Runs in the background and provides quick access through a system tray icon.
- **Localization Support**: Supports multiple languages for a better user experience.
- **Advanced Mutex Control**: Allows you to pause and resume mutex with ease.
- **Handling Roblox Already Opened**: Shows a dialog box with actions when Roblox is already opened. You can also choose to remember your action for future use.
- **Low System Resources Usage**: Uses only about 3 MB of RAM and 0% CPU (when running on an average setup).
- **Customizable Settings**: Allows you to configure couple of settings (config file is saved in the folder along with the .exe file).
- **Bloxstrap support**: Supports both Bloxstrap and the original bootstrapper.

## Getting Started 🚀

1. **Download**: Download the `MultiBloxy.exe` from the [latest release](https://github.com/Zgoly/MultiBloxy/releases/latest).
2. **Auto-Start**: To ensure MultiBloxy starts automatically with Windows, click <kbd>Win</kbd> + <kbd>R</kbd>, run the path `%ALLUSERSPROFILE%\Microsoft\Windows\Start Menu\Programs\StartUp`, and drop the `MultiBloxy.exe` file here.
3. **Run**: Launch `MultiBloxy.exe`, and it will appear in your system tray.
4. **Use**: Right-click the system tray icon to access the context menu and manage your Roblox instances.

## How It Works ⚙️
Roblox uses a singleton mutex named `ROBLOX_singletonEvent` to ensure that only one instance of the game is running at a time. MultiBloxy creates this mutex before Roblox does, allowing you to run as many instances of Roblox as you want.

## Is This a Virus? 🛡️

MultiBloxy is completely safe and not a virus. If you encounter a "Windows Protected Your PC" message, it appears because the application is unsigned, and obtaining a certificate can be costly. You can safely ignore it and run the program anyway. Here's how:

1. **Click on "More info"** in the warning message.
2. **Click on "Run anyway"** to proceed with running MultiBloxy.

For those who are still skeptical, you can compile the program yourself using [Visual Studio Community](https://visualstudio.microsoft.com/vs/). Alternatively, you can decompile the current executable file to ensure that it is completely safe.

## Is This Bannable? 🚫

MultiBloxy is not bannable as long as you do not break Roblox's rules. The tool is designed to help running multiple Roblox instances and does not interfere with the game's mechanics or provide any unfair advantages. Always ensure that your usage complies with Roblox's terms of service.

## Contributing 🤝

Contributions are welcome! If you have any suggestions, bug reports, or feature requests, please [open an issue](https://github.com/Zgoly/MultiBloxy/issues) or [submit a pull request](https://github.com/Zgoly/MultiBloxy/pulls).

## License 📜

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

Thank you for using MultiBloxy! 😊
