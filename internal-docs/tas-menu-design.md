# Main menu
- Keep things simple on the front
- Group all features in a tab, like TAS playback features in `TAS` tab, config in `Config` tab, etc
  - The tabs will go on the left / top of the menu
- Make it configurable wherever it can be

## Class structure
- `IMainMenuTab` is a tab entry for the main menu
---
- `MainMenu` implements things needed to render the base of the main GUI
- `TASTab` contains GUI stuff for playing a TAS

# Concept art thing
![main menu](TAS%20main%20menu.png)