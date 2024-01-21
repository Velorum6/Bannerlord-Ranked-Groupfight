# DoF Admin Tools (DAT) for Bannerlord Servers

While TaleWorlds has greatly improved the ingame admin tools with update 1.2.8, some options are still lacking. 

DAT aims to build on what TaleWorlds have given us by adding some new config options as well as ingame chat commands for actions not covered by TaleWorlds own administration panel.

## Installation
1. Download the latest version ("MODULE RELEASE") from the [Releases](https://gitlab.com/Krex/dofadmintools/-/releases)-page.
2. Unzip into `YOURBLSERVER/Modules/`.
3. Add to startup arguments for your server: `_MODULES_*Native*Multiplayer*DoFAdminTools*_MODULES_`.
    - Make sure to add after `Native` and `Multiplayer`. Load order with other modules should not matter.

## Features

Below is a list of all features currently implemented in DAT.

- Chat Commands
  - `!me`
    - Shows the using player their PlayerId.
  - `!playerinfo PLAYERNAME`
    - Shows the PlayerId of any player whose name contains the given `PLAYERNAME`. Only available to admins.
    - Note that `PLAYERNAME` does not require an exact match. For example, by typing `!playerinfo [DoF]` you can show the PlayerId of any player with the DoF clan tag.
- New Configuration Options / Console Commands
  - `dat_add_admin ADMINID` - Add a player id to the list of admins. When a player joins the server and their id is on the list, they can use the ingame admin panel and admin chat commands.
    - The player id can be obtained by running `!me` (by the player themselves) or `!playerinfo PLAYERNAME` (by an admin) ingame.
    - This not only saves your admins the hassle of typing in the admin password, it also makes it unnecessary for them to even have it if they do not absolutely need to use the web panel.
  - `dat_include FILENAME` - Load the file with the given name and parse all of its lines as console commands.
    - Note that only files stored either directly in or in a sub-directory of `YOURBLSERVER/Modules/Native/` may be included this way.
    - **EXAMPLE:** If you host multiple servers using the same files, this allows you to store shared configuration in a shared file. For example, if you have multiple servers with the same set of admins, you could store all of the respective `dat_add_admin X` commands in a file called `SharedAdmins.txt`, then load it in your server configs by using `dat_include SharedAdmins.txt`. This way, if you have to add or remove an admin, you only have to do it in one place, reducing the chance of missing something somewhere.
  - `dat_set_command_prefix PREFIX` - Set the prefix for chat commands to the given character or character sequence. 
    - Default is `!`.
    - Note that `/` is reserved for chat channels by TaleWorlds; it can not be used here.
  - `dat_set_show_joinleave_messages TRUE|FALSE`
    - Set whether to show a message in chat when a player joins or leaves the server. Options are `TRUE` or `FALSE`.

## Planned Features

Below is a list of features currently planned to be added to the module. If you have any other ideas, feel free to reach out and suggest them - or open a [Merge Request](https://gitlab.com/Krex/dofadmintools/-/merge_requests)!

- [ ] Timed messages - Add options to add one or more messages to be sent to players by the server on a configurable interval. 
- [ ] Scene Scripts - Not a whole lot is possible here, but teleport doors will come.
- [ ] Further Chat Commands
  - [ ] `!help`
  - [ ] `!heal`
  - [ ] `!extendwarmup`
  - [ ] ...
  - [ ] Multiple teleport variations (to player, player to me, ...)
- [ ] Logging
- [ ] A fix for TaleWorlds ban system, keeping permanent bans across server restarts
- [ ] Configuration for messages shown in chat, to allow for customization & internationalization
- [ ] ...


## For Developers
The following information is mainly intended for those interested in building the tools from source themselves, contributing to their further development or building upon them.

### Building from source
1. Download or Clone this repository
2. Set the `BLSERVER` environment variable to the path of your local server files installation, e.g. `D:\SteamLibrary\steamapps\common\Mount & Blade II Dedicated Server`.
   - You may need to restart your PC for msbuild to pick up on the newly set environment variable.
3. Open in your favorite IDE (personally using Rider, Visual Studio should work as well)
4. Build.

Please note that currently, the build does not assemble a full, ready-to-use-module. Copying together the `SubModule.xml` as well as the `DoFAdminTools.dll` file into the correct folders is currently still a manual process. This will be fixed soonTM.

### Basic guide
TODO: Add a basic guide for adding new chat/console commands and anything else relevant.

### License
All code in this repository is licensed under the MIT License. See the [LICENSE](https://gitlab.com/Krex/dofadmintools/-/blob/master/LICENSE) file for the full license text.

### Contributing
As per the license, you are free to build on the code provided here pretty much as you see fit. That said, if you do add something cool, please consider opening a Merge Request for it [here](https://gitlab.com/Krex/dofadmintools/-/merge_requests)!

If you do open a merge request, please keep in mind:
- Please give your merge request a proper title and a (short) description
- No use of Harmony unless 100% necessary - preferably never. 
  - Reflection is fine, though please try and keep it to a minimum
- If possible, make things configurable via console commands :)