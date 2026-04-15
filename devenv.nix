{ pkgs, lib, config, inputs, ... }:

let
  pkgs-stable = import inputs.nixpkgs-stable { system = pkgs.stdenv.system; };
  pkgs-unstable = import inputs.nixpkgs-unstable { system = pkgs.stdenv.system; };
in
{
  languages.dotnet.enable = true;
  # TEMP: see https://github.com/NixOS/nixpkgs/issues/502224
  # Can be removed when that issue is resolved
  # This needs to be explicitly set to the stable pkg, as the stable package doesn't need rebuilding  
  languages.dotnet.package = pkgs-unstable.dotnet-sdk;
  # And this ensures it doesn't hit the ICU conflict
  # env.DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = 1;

  languages.javascript.enable = true;
  languages.javascript.npm.enable = true;
  languages.javascript.package = pkgs.nodejs-slim_22;

  languages.java.enable = true;
  
  packages = [
    pkgs-stable.fantomas
    pkgs.nixfmt-rfc-style
   ];

  tasks = {
    "startup:dotnet-tool-restore" = {
      exec = "dotnet tool restore";
      cwd = "src";
      before = [ "devenv:enterShell" ];
      execIfModified = [
        "src/.config/dotnet-tools.json"        
      ]; 
      showOutput = true;
    };
    "startup:paket-restore" = {
      exec = "dotnet paket restore";
      cwd = "src";
      before = [ "devenv:enterShell" ];
      execIfModified = [
        "**/paket.*"
      ]; 
      showOutput = true;
    };
    "startup:npm-install" = {
      exec = "npm install";
      cwd = "src/vscode/contextive";
      before = [ "devenv:enterShell" ];
      execIfModified = [
        "src/vscode/contextive/package*.json"
      ];
      showOutput = true;
    };
  };

  enterShell = ''    
  '';
}
