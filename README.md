# Paper Tanks
This is a recreation of the PaperTanks Typescript Implementated Game
A simple game where you controll a tank on a page and fight enemies as well as players in multiplayer mode
with Randomised maps/levels

## Features
```
____________________
X = DONE
W = IN PROGRESS
  = NOT STARTED
____________________
[X] Campaign Mode
[W] Multiplayer Mode
[X] Level Editor
[X] Settings (Toggle Audio) 
```
## Launching Client
Run the main exectuable
### such as
```
PaperTanksV2-Client\PaperTanksV2-Client\bin\Debug\netcoreapp3.0\PaperTanksV2-Client.exe
```

## Launching Server
Run the main exectuable with the command argument of --server
### such as
```
PaperTanksV2-Client\PaperTanksV2-Client\bin\Debug\netcoreapp3.0\PaperTanksV2-Client.exe --server
```



## Levels
There are 2 level folders within the `resources` folder
- `level` - These are levels which are used for Campaign Mode (which is what the level editor loads and saves to)
- `multiplayer-level` - These are levels which are used for Multiplayer Mode, You can copy campaign levels to mutliplayer level folder
**Note: Levels are stored as a GUID (which preserves their creation order) and are always unique allowing for levels to be shared**