{
  description = "Description for the project";

  inputs = {
    flake-parts.url = "github:hercules-ci/flake-parts";
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    nix2container.url = "github:nlewo/nix2container";
  };

  outputs =
    inputs@{ flake-parts, nix2container, ... }:
    flake-parts.lib.mkFlake { inherit inputs; } {
      imports = [
        # To import an internal flake module: ./other.nix
        # To import an external flake module:
        #   1. Add foo to inputs
        #   2. Add foo as a parameter to the outputs function
        #   3. Add here: foo.flakeModule

      ];
      systems = [
        "x86_64-linux"
        "aarch64-linux"
        "aarch64-darwin"
        "x86_64-darwin"
      ];
      perSystem =
        {
          config,
          self',
          inputs',
          pkgs,
          system,
          ...
        }:
        let
          lib = pkgs.lib;
          tmp = pkgs.runCommand "tmp" { } ''
            mkdir -p $out/tmp
          '';
        in
        {
          # Per-system attributes can be defined here. The self' and inputs'
          # module parameters provide easy access to attributes of the same
          # system.

          # Equivalent to  inputs'.nixpkgs.legacyPackages.hello;
          packages.default = nix2container.packages.${system}.nix2container.buildImage {
            name = "unity-2017-container";
            tag = "local";
            copyToRoot = [
              (pkgs.buildEnv {
                name = "root-bin";
                paths = with pkgs; [
                  just
                  coreutils
                  bash

                  glibc_multi.bin
                ];
                pathsToLink = [ "/bin" ];
              })

              (pkgs.buildEnv {
                name = "root-lib";
                paths = with pkgs; [
                  glibc_multi.out
                ];
                pathsToLink = [
                  "/lib"
                  "/lib64"
                ];
              })

              # cups
              # expat
              # alsa-lib
              # nspr
              # dbus
              # libxdamage
              # libxtst
              # nss
              # libxrandr
              # libxcursor
              # libx11
              # libglvnd
              # gdk-pixbuf
              # glib
              # gtk2
              # libGLU
              # libcap
              # libxi
              # libxrender
              # glibc_multi.static
              # fontconfig.lib
              # freetype
              # pango
              # cairo
              # libxcomposite
              # libxext
              # libxfixes

              tmp
            ];
            perms = [
              {
                path = tmp;
                regex = ".*";
                mode = "0777";
              }
            ];
            config = {
              entrypoint = [ "/bin/bash" ];
            };
          };
        };
      flake = {
        # The usual flake attributes can be defined here, including system-
        # agnostic ones like nixosModule and system-enumerating ones, although
        # those are more easily expressed in perSystem.

      };
    };
}
