let
  nixpkgs = fetchTarball "https://github.com/NixOS/nixpkgs/tarball/nixos-25.05";
  pkgs = import nixpkgs { config = {}; overlays = []; };
in

pkgs.mkShellNoCC {
  packages = with pkgs; [
    jdk 
	# Version should match between nixos and jetbrains platform - currently Java 21
	# https://search.nixos.org/packages?channel=25.05&show=jdk&query=jdk
	# https://plugins.jetbrains.com/docs/intellij/build-number-ranges.html#platformVersions
  ];
}
