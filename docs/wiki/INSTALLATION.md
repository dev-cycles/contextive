# Installation

## IDE Plugins/Extensions

For the following IDEs, Contextive can be installed via their plugin/extensions marketplaces.

### Visual Studio Code

Open Visual Studio Code, launch the quick open (`Ctrl+P`) and then enter `ext install devcycles.contextive`.  OR, search `contextive` in the extensions side-bar.

Visit the [Contextive Extension](https://marketplace.visualstudio.com/items?itemName=devcycles.contextive) on the [VsCode Marketplace](https://marketplace.visualstudio.com/items?itemName=devcycles.contextive) for details.

Check the extension [README](../../src/vscode/contextive/README.md) for usage instructions.

### IntelliJ Plugin Platform

> [!WARNING]  
> This plugin is considered 'beta' status, as leverages relatively new [Language Server Protocol](https://plugins.jetbrains.com/docs/intellij/language-server-protocol.html) support from JetBrains which is still marked as unstable in the IntelliJ Platform API. Some features may not work or may not work as expected. Please [report issues](https://github.com/dev-cycles/contextive/issues/new?assignees=&labels=&projects=&template=bug_report.md&title=) in this project and we will liaise with JetBrains to resolve.
> 
> For example, Rider doesn't support C# files when opening a solution, see [#65](https://github.com/dev-cycles/contextive/issues/65)

Open your JetBrains product, e.g. IntelliJ IDEA, or JetBrains Rider, open the Settings dialog and choose "Plugins".  Search `contextive` in the Marketplace tab and click the `Install` button.

See JetBrains documentation on [installing plugins](https://www.jetbrains.com/help/idea/managing-plugins.html), and visit the [Contextive Plugin](https://plugins.jetbrains.com/plugin/23928-contextive) on the [JetBrains Marketplace](https://plugins.jetbrains.com/plugin/23928-contextive) for details.

Check the plugin [README](../../src/intellij/contextive/README.md) for usage instructions, known issues and links to related JetBrains issues with the Language Server Protocol support.

## Language Server Configurations

For the following IDEs, you will need to install the Contextive Language Server and then configure the IDE to use it.

#### IDE-specific Configuration

As discussed in the [usage guide](./USAGE.md), the default value of the `contextive.path` setting is: `.contextive/definitions.yml`.

Contextive currently relies on the configuration supplied by the IDE to determine if it should look in a different location - see each IDE's section below for details on how to change the `contextive.path` configuration setting.

You may also need to consult your IDE's documentation on how it determines the `workspace root`, as the default value is a path to the workspace root.

### Neovim

For [Neovim](https://neovim.io/), the Contextive Language Server can be installed via [Mason](https://github.com/williamboman/mason.nvim) by executing the Mason install command in Neovim.

```
:MasonInstall contextive
```

Alternatively, install manually as described [below](#installing-contextive-language-server).

The following configuration requires the `neovim/nvim-lspconfig` plugin, which can be installed and set up by following this [install guide](https://github.com/neovim/nvim-lspconfig#install).

Use lspconfig to setup and initialize the Contextive Language Server configuration. The following lua snippet needs to be included in the `init.lua` file either directly or from another lua module like `lspconfigs.lua`.

```lua
local lspconfig = require("lspconfig")

lspconfig.contextive.setup {}
```

The workspace root will be the first parent folder containing `.contextive` or `.git`.

To enable a custom path for contextive terms, include these settings in the language server setup configuration.

```lua
lspconfig.contextive.setup {
  settings = {
    contextive = {
      path = "./path/to/definitions.yml"
    }
  }
}
```

For more information on configuring Neovim with Lua modules, see: https://neovim.io/doc/user/lua-guide.html#lua-guide-config

### Helix

For [Helix](https://helix-editor.com/), install the Contextive Language Server manually as described [below](#installing-contextive-language-server).

Then add the following configuration to `~/.config/helix/languages.toml`:

```
[language-server.contextive]
command = "Contextive.LanguageServer"
```

To enable a custom path for contextive terms, include a config parameter in the language server setup configuration:

```
[language-server.contextive]
# The path can be an absolute path, or a path relative to the LSP workspace root.
# See `workspace-lsp-roots` in https://docs.helix-editor.com/master/configuration.html#editor-section for details.
config = { contextive = { path = "/path/to/definitions.yml" } }
command = "Contextive.LanguageServer"
```

Once Contextive is defined as a language sever, it can be added to specific languages, e.g.:

```
[[language]]
name = "typescript"
language-servers = [ { name = "typescript-language-server" }, { name = "contextive" } ]
```

See the [helix language configuration](https://docs.helix-editor.com/languages.html?highlight=roots#language-configuration) for more details, including the `roots` setting which may be needed for non-standard workspace roots.


See [this discussion](https://github.com/helix-editor/helix/discussions/8850) for enabling LSP servers in all files.

### Others

Official support for other IDEs coming soon!

In the meantime, if your IDE supports the Language Server Protocol (LSP) try downloading the Contextive Language Server as described [below](#installing-contextive-language-server) and consult your IDE documentation for how to configure a language server.

If you get it working, we'd love you to submit a PR to this README with instructions on how to get it working your favourite IDE!

## Installing Contextive Language Server


1. Download the appropriate zip file for your operating system and architecture:

   ```shell
   curl -L https://github.com/dev-cycles/contextive/releases/download/<version>/Contextive.LanguageServer-<os>-<arch>-<version>.zip -o Contextive.LanguageServer-<os>-<arch>-<version>.zip
   ```
  
   OR, manually download the language server asset from the [desired release](https://github.com/dev-cycles/contextive/releases)

2. Unzip the Contextive.LanguageServer and copy the file into a folder that is included in your system's PATH:

   Assuming the $HOME/bin directory has been created beforehand and is included in the system's PATH.

   ```shell
   unzip Contextive.LanguageServer-<os>-<arch>-<version>.zip -d contextive-language-server
   cp contextive-language-server/Contextive.LanguageServer $HOME/bin
   ```

3. Verify that Contextive.LanguageServer is found in the PATH. A non-zero exit code indicates that the language server was not found in the PATH:

   ```shell
   command -v Contextive.LanguageServer
   ```

   The command should return the absolute path to the binary if it's found in the system PATH.

4. For IDEs in the officially supported list, check the configuration guides above.  Otherwise, consult your IDE's Language Server documentation for details on how to configure a new language server and enable it for all files.
5. Check out our [usage guide](./USAGE.md) for details on configuring and using Contextive. 
