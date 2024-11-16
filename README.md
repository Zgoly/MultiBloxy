# roblox-multilauncher

## Disclaimer

This project is a **fork** of the original [MultiBloxy](https://github.com/Zgoly/MultiBloxy). The code provided here is offered under the **MIT License** unless otherwise stated. By using this app, you agree to the terms of the license. For more information on the license and usage, refer to the [LICENSE](LICENSE) file in the repository.

This fork includes enhancements, bug fixes, and optimizations for better performance and compatibility.

## Features

- **Multi-Account Support**: Run multiple Roblox accounts simultaneously with ease.
- **System Tray Integration**: Minimize the app to the system tray for quick access and background operation.
- **Customization Options**: Easily adjust settings via the system tray menu (right-click on the icon).
- **Lightweight**: Designed to run efficiently with minimal resource consumption.

## Prerequisites

- **Windows 7 (SP1), 8.x (8, 8.1), 10, 11** (Tested on Windows 11)
- **.NET SDK 9** (or higher)  
  Install via the following command:
  ```powershell
  winget install Microsoft.DotNet.SDK.9
  ```
- **.NET Framework 4.8 Developer Pack**  
  Download from the official .NET website:  
  [Download .NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-developer-pack-offline-installer)

## Building the App

### 1. Clone the Repository
Clone the repository to your local machine:

```bash
git clone https://github.com/Xelvanta/roblox-multilauncher <your/installation/directory>
cd <your/installation/directory>
```

### 2. Install Dependencies
Ensure that you have the necessary .NET SDK and Framework installed. If they are missing, follow the installation instructions in the **Prerequisites** section.

### 3. Build the App
To build the app, run the following command in the root directory of the project:

```bash
dotnet build
```

This will compile the application and place the output in the `bin\Debug` or `bin\Release` folder, depending on the build configuration.

## Running the App

### 1. Run the App from the Command Line
After building the app, run it using the following command:

```bash
dotnet run
```

Alternatively, navigate to the output directory (`bin\Debug` or `bin\Release`) and run the executable directly:

```bash
cd bin\Debug
roblox-multilauncher.exe
```

### 2. Access the App via the System Tray
Once the app is running, it will minimize to the **system tray**. You will find an icon for **roblox-multilauncher** there. 

- **Right-click** on the icon to access the app's menu.

## License

This project is a **fork** of the original [MultiBloxy](https://github.com/Zgoly/MultiBloxy) and is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Troubleshooting

- **Missing .NET SDK or Framework**: Ensure that both the .NET SDK 9 (or higher) and the .NET Framework 4.8 Developer Pack are installed.
- **Build Errors**: If you encounter build errors, verify that your environment meets all prerequisites. You may need to clean and rebuild the project using:
  ```bash
  dotnet clean
  dotnet build
  ```

---

By **Xelvanta Group Systems**  
For support or inquiries, please contact us at [enquiry.information@proton.me](mailto:enquiry.information@proton.me).  
GitHub: [https://github.com/Xelvanta](https://github.com/Xelvanta)