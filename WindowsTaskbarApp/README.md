# Windows Taskbar App

This project is a Windows application that runs in the taskbar and provides multiple configuration pages. Each configuration page contains text fields for user input and a save button. The application is designed to be user-friendly and allows for easy modification of the labels for each configuration form.

## Project Structure

```
WindowsTaskbarApp
├── Forms
│   ├── MainForm.cs
│   ├── ConfigurationForm1.cs
│   ├── ConfigurationForm2.cs
├── Program.cs
├── App.config
├── WindowsTaskbarApp.csproj
└── README.md
```

## Features

- **Taskbar Integration**: The application runs in the taskbar, providing easy access to configuration forms.
- **Multiple Configuration Forms**: The application includes multiple forms (ConfigurationForm1 and ConfigurationForm2) that can be used to gather user input.
- **Customizable Labels**: Each configuration form has a default label ("Label 1", "Label 2") that can be changed as needed.

## Getting Started

### Prerequisites

- .NET SDK installed on your machine.
- A compatible IDE or text editor for C# development.

### Building the Application

1. Clone the repository or download the project files.
2. Open a terminal and navigate to the project directory.
3. Run the following command to build the application:

   ```
   dotnet build
   ```

### Running the Application

After building the application, you can run it using the following command:

```
dotnet run
```

This will start the application, and you should see the taskbar icon. You can click on the icon to access the configuration forms.

## Modifying Labels

To change the labels of the configuration forms, open the respective form files (`ConfigurationForm1.cs` and `ConfigurationForm2.cs`) and modify the label text as desired.

## License

This project is licensed under the MIT License - see the LICENSE file for details.