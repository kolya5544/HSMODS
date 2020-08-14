# Forgery and ModMaker

Forgery is a Heat Signature mod installer with centralised database of approved modifications. ModMaker is the tool used to create new mods, based on original and modified contents. ModMaker uses special diff filetype for mods.

## Installation

To install Forgery, [download](https://github.com/kolya5544/HSMODS/releases) or compile the latest version, and run an executable file.
To install ModMaker, make sure to have [.NET Core](https://dotnet.microsoft.com/download), then, compile or download the latest version.

## Usage
To use Forgery, run an executable file and choose a directory with game executable file. Then, choose a mod from the list on the left to install, or install a mod from a file.
[All approved mods are stored there.](https://github.com/kolya5544/HSMODS/tree/master/mods).

To use ModMaker, you have to create two directories. One should include original Heat Signature files, and the other one should include modded files. For example, if you modded Heat_Signature.exe, `original` folder should have original Heat_Signature.exe executable, and `modded` folder should have the modded Heat_Signature.exe.

Place ModMaker one directory above folders, and run it in console
```bash
> ModMaker.exe modded_folder original_folder
```

Once ModMaker is done, newmod.hsmod will appear. You can then open a pull request to add this mod to the database. All submissions are reviewed manually

## Contributing
Contribute your modifications using pull requests. Changes to the code are welcome, but should be documented. For major changes, open an issue to discuss the changes

## License
[MIT](https://choosealicense.com/licenses/mit/)
