# Revit Ninja

**Revit Ninja** is a productivity-focused addin for Autodesk Revit, designed to streamline daily BIM tasks such as view state management, reinforcement selection, alignment, model cleanup, coordination issue tracking, and more. It offers a modular and evolving toolkit built to support BIM engineers and developers across design and coordination workflows.

---

## Features
### 🔄 Automatic Updates
Revit Ninja checks for updates automatically, ensuring you're always using the latest version.

### View State Management
- **Save View State**: Save the visibility state of elements directly within the view, without relying on shared parameters.
- **Restore View State**: Restore the visibility of previously saved elements for a given view.
- **Reset Sheet**: Restore saved view states for all views on the active sheet or across all sheets.

### Reinforcement Tools
- **Select by Host**: Select all rebar elements hosted by a selected structural element.
- **Find RFT**: Locate rebar by entering its partition and bar number.
- **Toggle Rebar On/Off**: Toggle the visibility of the rebar category in the active view.

### General Tools
- **Select by Parameter**: Select elements based on a specific parameter name and value.
- **Submit BIM Model**: Clean up the model before submission by:
  - Removing all views not placed on sheets (WIP views)
  - Deleting all DWG imports or links
  - Removing all Revit links

### Coordination Tools
- **Revizto Issues**: Import coordination issues from a Revizto report. Each issue is represented by a 3D marker (ball) in the model containing its metadata.

### Builder Work
- **Provision for Void**: Create solid placeholder openings at MEP-structure penetration locations to reserve space for builder work.

### Surveying Tools
- **Points Coordinates**:
  - Place points in plan and extract their shared coordinates.
  - Import/export point coordinates to/from Excel.
  - Generate tables in Revit containing the point data.

### UI Enhancement
- **Color Tabs**: Assign a unique tab color for each project to visually distinguish between open projects.

---

## Installation

1. Download the latest release from the [Releases](https://github.com/oelshaf3y/RevitNinja/releases) page.
2. Extract the contents to your Revit Addins folder (e.g., `%APPDATA%\Autodesk\Revit\Addins\20XX\`).
3. Launch Revit. The Revit Ninja tab will appear in the ribbon.

---

## Usage

- Use **Save/Restore View State** to store and return to specific view visibility configurations.
- Apply **Reset Sheet** to standardize views across sheets with saved visibility settings.
- Use **Select by Host** and **Find RFT** to quickly locate and manage rebar.
- Apply **Submit BIM Model** before submission to clean up your file.
- Use **Points Coordinates** to manage survey data and create layout tables.
- Import **Revizto Issues** to visualize and resolve clashes directly within Revit.
- Use **Color Tabs** to improve navigation when working on multiple projects.

---

## Contributing

Contributions, issues, and feature requests are welcome! Please check the [issues page](https://github.com/oelshaf3y/RevitNinja/issues) to report bugs or suggest new features.

---

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE.MD) file for details.

Credits to [icons8.com](https://icons8.com) for UI icons.
