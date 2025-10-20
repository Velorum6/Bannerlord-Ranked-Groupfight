# DoF Admin Tools - Bannerlord Ranked Groupfight Edition

> **⚠️ IMPORTANT**: This is a **modified version** of the original [DoF Admin Tools](https://gitlab.com/Krex/dofadmintools) by Krex, specifically customized for the **Bannerlord Ranked Groupfight** server by Velorum.

## 🎯 **About This Fork**

This repository contains a heavily modified version of the original DoF Admin Tools, enhanced with additional behaviors and features specifically designed for competitive ranked groupfight gameplay:

- **AutoBalanceBehavior**: Intelligent team balancing based on MMR and party groups
- **HitCounter**: Advanced hit tracking and statistics system
- **InfantryOnlyBehavior**: Enforces infantry-only restrictions for competitive play
- **Multikill**: Multikill detection and announcement system
- **HPSyncAntiCheat**: Anti-cheat HP synchronization controls
- **Party Management**: Enhanced party system for organized competitive play
- **Mirror Matchup**: Enforces mirror matchups on map changes
- **Player Statistics**: Comprehensive player stats and MMR tracking

## 🏆 **Original DoF Admin Tools**

While TaleWorlds has greatly improved the ingame admin tools with update 1.2.8, some options are still lacking. The original DAT aims to build on what TaleWorlds have given us by adding new config options and ingame chat commands for actions not covered by TaleWorlds own administration panel.

**Original Repository**: [GitLab - Krex/DoFAdminTools](https://gitlab.com/Krex/dofadmintools)

## 📦 **Installation**

### For Bannerlord Ranked Groupfight Server
This modified version is specifically designed for the Bannerlord Ranked Groupfight server. If you're running a different server, consider using the [original DoF Admin Tools](https://gitlab.com/Krex/dofadmintools) instead.

1. Download the latest release from this repository
2. Extract to your Bannerlord server's `Modules/` directory
3. Add to server startup arguments: `_MODULES_*Native*Multiplayer*DoFAdminTools*_MODULES_`
4. Configure the additional behaviors through console commands (see Features section)

> **Note**: Load order should be after `Native` and `Multiplayer` modules.

## 🚀 **Enhanced Features**

This fork includes all original DoF Admin Tools features PLUS the following additions specifically for competitive ranked gameplay:

### **New Mission Behaviors**
- **AutoBalanceBehavior**: Automatically balances teams based on MMR and party groups
- **HitCounter**: Tracks and displays hit statistics for competitive analysis
- **InfantryOnlyBehavior**: Enforces infantry-only restrictions for fair competitive play
- **Multikill**: Detects and announces multikill achievements
- **HPSyncAntiCheat**: Prevents HP synchronization exploits in competitive modes
- **Party Management**: Advanced party system for organized team play
- **Player Statistics**: Comprehensive MMR and performance tracking

### **Original DoF Admin Tools Features**

Below is a list of all features from the original DAT implementation:

- Chat Commands
  - **Admin Commands** - These commands can only be used by admins.
    - `!playerinfo PLAYERNAME`
      - Shows the PlayerId of any player whose name contains the given `PLAYERNAME`. Only available to admins.
      - Note that `PLAYERNAME` does not require an exact match. For example, by typing `!playerinfo [DoF]` you can show the PlayerId of any player with the DoF clan tag.
    - `!heal <PLAYERNAME>`
      - Heal any player whose name contains the given `PLAYERNAME`. If no name is given, all players are healed instead.
      - Healing in this case means restoring the HP of the player and their mount, restoring ammunition (arrows and bolts, not throwing weapons) and restoring the HP of the players shields.
    - `!move X Z`
      - Teleport yourself, moving in the direction you're looking at by `X` meters and up by `Z` meters. `X` and `Z` may be positive, negative or zero but must be whole numbers.
    - `!tptome <PLAYERNAME>`
      - Teleports any player whose name contains the given `PLAYERNAME` to your current position. If no `PLAYERNAME` is given, all players are teleported.
    - `!tpmeto PLAYERNAME`
      - Teleport yourself to the position of a player whose name contains the given `PLAYERNAME`. If multiple players names contain the given `PLAYERNAME`, the first one found is used, so be precise.
    - `!removehorses`
      - Remove all horses from the scene that do not currently have a rider.
      - Provided by Doseq - thank you!
    - `!slay <PLAYERNAME>`
      - Slays all alive players whose names contain the given `PLAYERNAME`, or all players if no `PLAYERNAME` is given.
    - `!extendwarmup`
      - Resets the warmup timer to its configured maximum, if it is currently active.
      - Provided by Gotha - thank you!
    - `!endwarmup`
      - Reduces the warmup timer to 30 seconds, if it is currently active.
        - This is the same functionality as found in the admin panel; it is re-added as a chat command here for quick use for those that prefer to use it this way.
      - Provided by Gotha - thank you!
  - **Public Commands** - These commands can be used by every player.
    - `!me`
      - Shows the using player their PlayerId.
    - `!help <COMMANDNAME>`
      - If `<COMMANDNAME>` is not set, shows a list of all currently available commands. Includes admin commands if the player is an admin.
      - If `<COMMANDNAME>` is set, shows information for the given command, assuming there is one matching the name.
    - `specs`
      - Prints a list of all players currently in spectator mode.
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
  - `dat_set_show_adminpanel_usage TRUE|FALSE`
    - Set whether actions taken by admins using TaleWorlds admin panel should also cause a chat message to be sent to all players, as admin actions using chat commands do.
    - Enabled by default.
  - `dat_set_and_load_banlist FILENAME`
    - Set the path to a ban list file (within your `YOURBLSERVER/Modules/Native/` folder, as with `dat_include`), then load all bans stored within the given file.
    - By default, banning someone only lasts until the server is restarted. This command allows you to persist bans across server restarts.
    - **If this command is not run, bans will not be loaded**. However, any new ban will still be written to `YOURBLSERVER/Modules/Native/banlist.txt`, should you want to use them later.
      - Similarly, if you run multiple servers using the same files, bans from one server will only be transferred to the others when they execute this command, re-reading the banlist file.
    - Anything in a line after a `#` is ignored. You can use this to store extra information on the PlayerId before it (by default, the name of the player, of the banning admin and the date of the ban are stored) or to (temporarily) exclude bans from being loaded. 
      - You can permanently remove a ban by deleting the relevant line from the banlist file. Note that a server restart is required for the unban to take effect.
  - `dat_set_prevent_unnecessary_hp_sync TRUE|FALSE` **FOR SKIRMISH/CAPTAIN/BATTLE ONLY**
    - In the game modes mentioned above, by default, players current hit points are synchronized to *all* players on the server. For competitive environments especially, this information could be used by client-side cheating software. Enabling this option will replace the default behaviour for synchronizing player hitpoints with a custom one, synchronizing the hitpoints only to teeammates and spectators (to allow for streamers to still have access to the information).
    - This option is enabled by default. 
      - For servers not running any of the game modes mentioned above, nothing will happen, even if it is enabled.
    - Provided by Gotha - thank you!
  - `dat_add_automessage MESSAGE`
    - Adds a message to the list of messages to be automatically sent to all players by the server.
    - If no messages are configured by the user, no messages will be sent.
    - You may add as many messages as you like, though each message is currently limited to 256 characters.
  - `dat_set_automessage_interval INTERVAL`
    - How often the server should send the messages configured using `dat_add_automessage` to the players, in seconds.
    - By default, messages are sent every 60 seconds.
    - You may disable the system entirely by setting a value of zero or below (or by not adding any messages).
  - `dat_set_automessage_type CHAT|ADMINCHAT|BROADCAST`
    - In what way the server should send the AutoMessages to the players.
      - `CHAT` is a white chat message
      - `ADMINCHAT` is a purple chat message
      - `BROADCAST` is a purple chat message with an extra sound notification as well as a popup in the center-top of the players screens.
    - By default, messages are sent as `CHAT` messages.
  - `dat_no_more_spam`
    - Disables the default DebugManager, preventing many messages usually spammed to the console from being printed, most notably the notifications for heartbeat messages to TaleWorlds main server.
    - Note that some mods may use the same mechanism to print their information messages, and those will be lost, too, if this option is set in the config.
    - Can not be re-enabled at runtime; if you do notice you need the debug messages, you will have to restart your server without this option in its config.

## Planned Features

Below is a list of features currently planned to be added to the module. If you have any other ideas, feel free to reach out and suggest them - or open a [Merge Request](https://gitlab.com/Krex/dofadmintools/-/merge_requests)!

- [X] Timed messages - Add options to add one or more messages to be sent to players by the server on a configurable interval. 
- [ ] Scene Scripts - Not a whole lot is possible here, but teleport doors will come.
- [ ] Further Chat Commands
  - [X] `!help`
  - [X] `!heal`
  - [X] `!extendwarmup`
  - [ ] ...
  - [X] Multiple teleport variations (to player, player to me, ...)
- [ ] Logging
- [X] A fix for TaleWorlds ban system, keeping permanent bans across server restarts
- [ ] Configuration for messages shown in chat, to allow for customization & internationalization
- [ ] ...


## 🛠️ **For Developers**

### Building from Source
1. Clone this repository
2. Set the `BLSERVER` environment variable to your Bannerlord server path:
   ```bash
   # Windows
   set BLSERVER=C:\Path\To\Bannerlord\Server
   
   # Linux/Mac  
   export BLSERVER=/path/to/bannerlord/server
   ```
3. Open in Visual Studio or JetBrains Rider
4. Build the project

### Code Structure
This fork maintains the original DoF Admin Tools architecture while adding:
- **MissionBehaviors/**: New competitive gameplay behaviors
- **Party/**: Enhanced party management system
- **PlayerStats/**: MMR and statistics tracking
- **Additional Commands**: New chat commands for competitive features

## 📄 **License & Credits**

### License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Credits & Acknowledgments
- **Original Developer**: [Krex](https://gitlab.com/Krex) - Creator of DoF Admin Tools
- **Fork Developer**: **Velorum** - Enhanced version for Bannerlord Ranked Groupfight
- **Contributors**: Gotha, Doseq, and the community for original features
- **TaleWorlds**: For the Bannerlord platform and API
- **Community**: All server admins and players who provided feedback

## 🤝 **Contributing**

This is a specialized fork for competitive gameplay. For general DoF Admin Tools contributions, please consider the [original repository](https://gitlab.com/Krex/dofadmintools).

If you have suggestions for competitive features or bug fixes specific to this fork:
1. Fork this repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## 📞 **Support**

- **Issues**: [GitHub Issues](https://github.com/Velorum6/Bannerlord-Ranked-Groupfight/issues)
- **Original DoF Tools**: [GitLab Repository](https://gitlab.com/Krex/dofadmintools)

---

**Made with ❤️ for the competitive Bannerlord community**

*This fork builds upon the excellent foundation provided by Krex's original DoF Admin Tools.*