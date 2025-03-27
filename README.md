# QR Scanner Service

This README file provides a comprehensive guide for developers, including :
installation, usage, configuration, and development instructions. 

## Description
QR Scanner Service is a Windows application that reads data from a QR scanner connected 
via a serial port and writes the data to an Excel file. 
The application supports multiple workflows and can be minimized to the system tray.

## Table of Contents
- [Description](#description)
- [Table of Contents](#table-of-contents)
- [Installation](#installation)
- [Usage](#usage)
- [Features](#features)
- [Configuration](#configuration)
- [Development](#development)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Installation
1. Clone the repository:

git clone https://github.com/tusharbhut011/QR_Scanner.git

2. Open the solution in Visual Studio 2022.
3. Build the solution to restore the NuGet packages and compile the project.

## Usage
1. Connect your QR scanner to a serial port on your computer.
2. Run the application.
3. Select the appropriate COM port and baud rate.
4. Click "Start Service" to begin scanning.
5. The scanned data will be written to the specified Excel file.

## Features
- Read data from a QR scanner via a serial port.
- Write scanned data to an Excel file.
- Support for multiple workflows.
- Minimize to system tray with context menu options.
- Language support for English and German.

## Configuration
- **COM Port**: Select the COM port to which the QR scanner is connected.
- **Baud Rate**: Enter the baud rate for the serial communication.
- **Excel File**: Specify the path to the Excel file where the scanned data will be written.
- **Language**: Select the language for the user interface (English or German).
- **Start with Windows**: Enable or disable the option to start the application with Windows.

## Development
### Prerequisites
- Visual Studio 2022
- .NET Framework (version specified in the project)
- Excel Interop libraries

### Project Structure
- `QRScannerService_GUI`: Contains the Windows Forms application.
- `QRScannerService_Core`: Contains core interfaces and models.

### Key Classes and Interfaces
- `MainForm`: The main form of the application.
- `ISerialPortService`: Interface for serial port communication.
- `IExcelService`: Interface for Excel file operations.
- `IWorkflowService`: Interface for managing workflows.
- `WorkflowConfig`: Model representing a workflow configuration.

### Running the Application
1. Open the solution in Visual Studio.
2. Set `QRScannerService_GUI` as the startup project.
3. Press `F5` to build and run the application.

### Debugging
- Use Visual Studio's built-in debugging tools to set breakpoints and inspect variables.
- Check the Output window for any debug messages.

