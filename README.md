# FastFileSearch

FastFileSearch is a .NET application designed to quickly search for files in a specified directory. This README will guide you through the installation of the .NET SDK and Desktop Runtime, as well as running the project.

## Prerequisites

Before you can run the project, you need to install the .NET SDK and Desktop Runtime on your machine.

### Installing .NET SDK

1. **Download the .NET SDK:**
   - Download the installer for .NET SDK 9.0.302:
     - https://drive.google.com/file/d/1BbaSQAQfpQRovGtR9IWxHJ5V3mHe4J2N/view?usp=sharing
     - [Download dotnet-sdk-9.0.302-win-x64.exe]

2. **Install the .NET SDK:**
   - Run the installer you downloaded (`dotnet-sdk-9.0.302-win-x64.exe`).
   - Follow the installation instructions.

### Installing .NET Desktop Runtime

1. **Download the .NET Desktop Runtime:**
   - Download the installer for .NET 8.0 Desktop Runtime:
     - [Download windowsdesktop-runtime-8.0.18-win-x64.exe]
     - (https://drive.google.com/file/d/1ofrPzqkclObm6rgtCFw-kdIaF-5lqV20/view?usp=sharing

2. **Install the .NET Desktop Runtime:**
   - Run the installer you downloaded (`windowsdesktop-runtime-8.0.18-win-x64.exe`).
   - Follow the installation instructions.

3. **Verify the Installation:**
   - Open a new Command Prompt or PowerShell window.
   - Run the following command to check the installed version:
     ```bash
     dotnet --version
     ```
   - If installed correctly, this will display `9.0.302` (or similar) indicating the version of the .NET SDK.

## Running the Project

1. **Navigate to the Project Directory:**
   - Open a Command Prompt or PowerShell window.
   - Change the directory to the `FastFileSearch` folder:
     ```bash
     cd path\to\FastFileSearch
     ```
   - Replace `path\to\FastFileSearch` with the actual path to your project folder.

2. **Run the Application:**
   - Execute the following command to run the project:
     ```bash
     dotnet run
     ```

3. **Usage:**
   - Follow the on-screen instructions to search for files in the specified directory.

## Additional Information

- Ensure that you have the necessary permissions to run the application.
- For further details on using the application, refer to the inline help or documentation within the code.

## Troubleshooting

If you encounter issues running the project, please check the following:

- Confirm that the .NET SDK and Desktop Runtime are installed correctly.
- Make sure you are in the correct project directory.
- Check for any error messages in the terminal for clues on what might be wrong.
