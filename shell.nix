let
  nixpkgs = fetchTarball "https://github.com/NixOS/nixpkgs/tarball/nixos-25.05";
  pkgs = import nixpkgs { config = {}; overlays = []; };
in

pkgs.mkShellNoCC {
  packages = with pkgs; [
    dotnet-sdk_8
    fantomas
  ];

  DOTNET_ROOT = "${pkgs.dotnet-sdk_8}";

  shellHook = ''
    pushd src
     dotnet tool restore
     dotnet paket install
    popd

    pushd src/vscode/contextive
     npm install
    popd
  '';
}
