This document provides instructions on how to build the Janus project using the dotnet command-line interface (CLI) on apt based systems like Debian and Ubuntu.

1. Introduction
The Janus project is a C# application built using the .NET 9.0 framework. To build and run this project, you will need to install the .NET 9.0 SDK.

2. Installation
2.1. .NET 9.0 SDK Installation
These instructions are for installing the .NET 9.0 SDK on Ubuntu. You can follow these steps to add the Microsoft package repository and install the SDK.

1. Add the Microsoft package signing key to your list of trusted keys and add the package repository.

Open your terminal and run the following commands:

Bash

# Get Ubuntu version
declare repo_version=$(if command -v lsb_release &> /dev/null; then lsb_release -r -s; else grep -oP 'VERSION_ID="\K[^"]+' /etc/os-release; fi)

# Download Microsoft signing key and repository
wget https://packages.microsoft.com/config/ubuntu/$repo_version/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

# Install the repository
sudo dpkg -i packages-microsoft-prod.deb

# Clean up
rm packages-microsoft-prod.deb
2. Install the .NET 9.0 SDK

Update your package lists and install the .NET SDK:

Bash

sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-9.0
2.2. A Note on .NET Framework 4.8.1
The user requested installation instructions for .NET Framework 4.8.1. It is important to note that the .NET Framework is a Windows-only framework and is not officially supported on Linux distributions.

The Janus project specifically targets net9.0-windows as seen in the Janus.csproj file. This means it is a .NET (formerly .NET Core) application, not a .NET Framework application. The installation of the .NET 9.0 SDK is the correct and only prerequisite for building this project on a supported platform.

3. Building the Project
Once you have the .NET 9.0 SDK installed, you can build the Janus project.

Navigate to the project's root directory. This is the directory that contains the Janus.sln file.

Run the dotnet build command.

Bash

dotnet build
This command will compile the entire solution and place the output in the bin/Debug/ or bin/Release directory within the project folder.