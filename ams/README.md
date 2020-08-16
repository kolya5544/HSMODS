# Heat Signature AMS (Advanced Mod System)

AMS is an advanced modification/addon system that allows anyone to use C++/C/C# to create a Heat Signature modification.

## Installation

FORGERY, by default, installs AMS for modifications that require AMS. You can, however, install it seperately and get an ability to run several AMS modifications at the same time. To do that, just copy the contents of `binaries` folder to the root folder of your game.

Example (pseudo-bash):

```bash
> copy /binaries/* "/steamapps/common/Heat Signature/*"
```

## Usage

You can find an example modification for Heat Signature in this folder. Every modification should contain an exported Start() method. Keep in mind to export DLL in `x86`/`x64` and NOT `Any CPU`.

To install mods for AMS, just install AMS and put mods inside `mods` folder in the root of your game.

Mod database of such modifications is located at [AMS_mods](https://github.com/kolya5544/HSMODS/ams_mods).

## Contributing

Feel free to post your AMS modifications to [AMS_mods](https://github.com/kolya5544/HSMODS/ams_mods). You are also welcome to edit an example mod, or add another example.

## License
[MIT](https://choosealicense.com/licenses/mit/)