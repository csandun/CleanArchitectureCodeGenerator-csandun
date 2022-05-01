# Code Generator for Risk Project 

Download the extension at the
[VS Gallery]https://marketplace.visualstudio.com/items?itemName=CSandun.riskcqrs)

### Introduction

A Visual Studio extension for easily create application features code  to CQRS based clean architecture  project files for usecases. Simply hit Shift+F2 to create an empty file in the
selected folder or in the same folder as the selected file.

See the [changelog](CHANGELOG.md) for updates and roadmap.

### Features

- Easily create following application features code
    - Create command
    - Update command
    - Delete command
    - Get all query
    - Get all with pagination
    - Get by id
    - Generate Dto

### CleanArchitecture for Razor Page project
The current project only applies to the following development projects.

Github :https://github.com/neozhu/RazorPageCleanArchitecture
[![Clean Architecture Solution For Razor Page Development](https://github.com/neozhu/RazorPageCleanArchitecture/blob/main/doc/screenshot/2021-08-11_19-28-59.png?raw=true)](https://www.youtube.com/watch?v=NcMd5W3C63A)

### Show the dialog

A new button is added to the context menu in Solution Explorer.

![Add new file dialog](art/menu1.png)

You can either click that button or use the keybord shortcut **Shift+F2**.

Select Entity Name from Domain Project

![Add new file dialog](art/dialog1.png)

### Create folders and namespace

Create additional folders for your file by using forward-slash to
specify the structure.

For example, by typing **scripts/test.js** in the dialog, the
folder **scripts** is created if it doesn't exist and the file
**test.js** is then placed into it.

### Generate sourcecode
![Source code for application features](art/code.png)

### Generate to-do list
![to-do list](art/task-list.png)

### code templates
You can modify these templates according to your own projects
![tempaltes](art/template.png)

## Contribute
Check out the [contribution guidelines](.github/CONTRIBUTING.md)
if you want to contribute to this project.

For cloning and building this project yourself, make sure
to install the
[Extensibility Tools 2015](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.ExtensibilityTools)
extension for Visual Studio which enables some features
used by this project.

## License
[Apache 2.0](LICENSE)
