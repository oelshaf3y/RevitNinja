# Revit Ninja

**Revit Ninja** is a powerful addin designed to enhance workflow efficiency in Autodesk Revit by providing advanced view management, reinforcement selection, and parameter-based element isolation. The addin includes the following key features:

## Features

### View State Management
- **View State Addin**: Save the visibility state of elements in a view by storing their IDs in a parameter for future restoration.
- **Restore State**: Display only the elements whose IDs are stored in the view state parameter.
- **Reset Sheet**: Restore view states across all views in a sheet or across multiple sheets.
- **Isolate Elements**: Hide all other elements and focus only on selected elements within the current view.
- **Hide Unhosted**: Hide all Rebar elements which their host is permenantly hidden within the currnet view.


### Reinforcement Tools
- **Select by Host**: Select all reinforcement elements hosted by the chosen element.
- **Find RFT**: Search for rebar by partition and number.
- **Toggle Rebar On/Off**: Toggle the visibility of rebar elements in the current view.
- **Find Rebar**: Find rebar by partition and rebar number.

### Align Tools
- **Algin Tags**: Align Tags by other Tag or allign with a single point (Vertically/Horizontally).
- **Algin Elements**: Align Elements by other Element or allign with a single point (Vertically/Horizontally).
- **Mid Between 2 Pts**: Align Element between other two elements.

### General Tools
- **Select by Parameter**: Select elements based on a specified parameter and its value.
- **Delete CAD**: Delete all DWG imports and links in the project.
- **Not on Sheets**: Delete all views which are not placed on sheets.
- **Rotate Locally**: Rotates all selected element from their Center Point.

## Installation

1. Download the latest release from the [Releases](https://github.com/oelshaf3y/RevitNinja/releases) page.
2. Setup and enjoy.

## Usage

1. **View State Addin**: Use this tool to save current element visibility states in a view, storing element IDs in a parameter for future restoration.
2. **Restore State**: Restore the visibility of elements in a view based on previously saved IDs.
3. **Reset Sheet**: Restore view states across all views within a selected sheet or all sheets.
4. **Isolate Elements**: Select elements and isolate them in the current view.
5. **Toggle Rebar On/Off**: Quickly hide or display rebar elements in the view.
6. **Select by Host**: Choose a host element to automatically select all hosted rebar.
7. **Find RFT**: Locate rebar by entering its partition and number.
8. **Select by Parameter**: Use parameter filtering to select elements based on a specific parameter and value.

## Contributing

Contributions, issues, and feature requests are welcome! Feel free to check out the [issues page](https://github.com/oelshaf3y/RevitNinja/issues) for any open issues or create a new issue to request a feature.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.MD) file for details.
Credit to [icons8.com](https://icons8.com) for the icons.
