# MultiBloxy 🎮🔓

<img align="right" src="https://raw.githubusercontent.com/Zgoly/MultiBloxy/refs/heads/main/MultiBloxy/Resources/MultiBloxy.ico" />

MultiBloxy is a Windows system tray icon application designed to manage and control multiple Roblox instances. It provides a convenient way to open multiple instances for different accounts and offers a variety of options to manage them efficiently.

[![License][shield-repo-license]][repo-license]
[![Downloads][shield-repo-releases]][repo-releases]
[![Version][shield-repo-latest]][repo-latest]
[![Stars][shield-repo-stargazers]][repo-stargazers]
[![Issues][shield-repo-issues]][repo-issues]
[![Pulls][shield-repo-pulls]][repo-pulls]
[![Forks][shield-repo-forks]][repo-forks]

<a href="https://dsc.gg/zgoly">
  <img alt="Discord" src="https://invidget.switchblade.xyz/y8fBWPNJFm">
</a>

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
1. Download the `MultiBloxy.exe` from the [latest release](https://github.com/Zgoly/MultiBloxy/releases/latest).
2. To ensure MultiBloxy starts automatically with Windows, click <kbd>Win</kbd> + <kbd>R</kbd>, run the path `%ALLUSERSPROFILE%\Microsoft\Windows\Start Menu\Programs\StartUp`, and drop the `MultiBloxy.exe` file here.
3. Launch `MultiBloxy.exe`, and it will appear in your system tray.
4. Right-click the system tray icon to access the context menu and manage your Roblox instances.

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

If you want to contribute to localization, you can add new translations or improve existing ones. The localization code is located in the [Localization.cs](https://github.com/Zgoly/MultiBloxy/blob/main/MultiBloxy/Localization.cs) file. Feel free to add new languages or correct any mistakes.

## Join Our Discord Server 💬
For faster responses to your issues, problems, and other inquiries, join [our Discord server](https://dsc.gg/zgoly):

<a href="https://dsc.gg/zgoly">
  <img alt="Discord" src="https://invidget.switchblade.xyz/y8fBWPNJFm">
</a>

## License 📜
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

Thank you for using MultiBloxy! 😊

[shield-repo-license]: https://img.shields.io/github/license/Zgoly/MultiBloxy?style=flat&labelColor=c80064&color=c80064
[repo-license]: https://github.com/Zgoly/MultiBloxy/blob/main/LICENSE

[shield-repo-releases]: https://img.shields.io/github/downloads/Zgoly/MultiBloxy/total?style=flat&labelColor=c80064&color=c80064
[repo-releases]: https://github.com/Zgoly/MultiBloxy/releases

[shield-repo-latest]: https://img.shields.io/github/v/release/Zgoly/MultiBloxy?style=flat&labelColor=c80064&color=c80064
[repo-latest]: https://github.com/Zgoly/MultiBloxy/releases/latest

[shield-repo-stargazers]: https://img.shields.io/github/stars/Zgoly/MultiBloxy?style=flat&labelColor=c80064&color=c80064
[repo-stargazers]: https://github.com/Zgoly/MultiBloxy/stargazers

[shield-repo-issues]: https://img.shields.io/github/issues/Zgoly/MultiBloxy?style=flat&labelColor=c80064&color=c80064
[repo-issues]: https://github.com/Zgoly/MultiBloxy/issues

[shield-repo-pulls]: https://img.shields.io/github/issues-pr/Zgoly/MultiBloxy?style=flat&labelColor=c80064&color=c80064
[repo-pulls]: https://github.com/Zgoly/MultiBloxy/pulls

[shield-repo-forks]: https://img.shields.io/github/forks/Zgoly/MultiBloxy?style=flat&labelColor=c80064&color=c80064
[repo-forks]: https://github.com/Zgoly/MultiBloxy/network/members
