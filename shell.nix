let
  nixpkgs = fetchTarball "https://github.com/NixOS/nixpkgs/tarball/nixos-24.05";
  pkgs = import nixpkgs { config = {}; overlays = []; };
in

pkgs.mkShellNoCC {
  packages = with pkgs; [
    dotnet-sdk_8
  ];
  shellHook = ''
    pushd src
     dotnet tool restore
     dotnet paket install
    popd

    pushd src/vscode/contextive
     npm install
    popd

    export CONTEXTIVE_DEBUG=1
  '';
}
