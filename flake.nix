{
  inputs = {
    fenix = {
      url = "github:nix-community/fenix";
      inputs.nixpkgs.follows = "nixpkgs";
    };
    rust-overlay = {
      url = "github:oxalica/rust-overlay";
      inputs.nixpkgs.follows = "nixpkgs";
    };
    nixpkgs.url = "github:NixOS/nixpkgs";
    flake-parts.url = "github:hercules-ci/flake-parts";
    naersk.url = "github:nix-community/naersk";
  };

  outputs =
    {
      flake-parts,
      nixpkgs,
      rust-overlay,
      naersk,
      fenix,
      ...
    }@inputs:
    flake-parts.lib.mkFlake { inherit inputs; } {
      systems = [
        "x86_64-linux"
      ];
      perSystem =
        {
          inputs',
          system,
          pkgs,
          ...
        }:
        let
          rust-doc = pkgs.writeShellApplication {
            name = "rust-doc";
            text = ''
              xdg-open "${inputs'.fenix.packages.stable.rust-docs}/share/doc/rust/html/index.html"
            '';
          };

          rust = pkgs.rust-bin.stable.latest.default;

          toolchain =
            with fenix.packages.${system};
            combine [
              minimal.rustc
              minimal.cargo
              targets.x86_64-pc-windows-gnu.latest.rust-std
              targets.i686-pc-windows-gnu.latest.rust-std
            ];
          naersk' = naersk.lib.${system}.override {
            cargo = toolchain;
            rustc = toolchain;
          };
        in
        {
          _module.args.pkgs = import nixpkgs {
            inherit system;
            overlays = [
              (import rust-overlay)
            ];
          };

          devShells.default = pkgs.mkShell {
            packages = with pkgs; [
              dotnet-sdk_8
              (rust.override {
                extensions = [
                  "rust-analyzer"
                  "rust-src"
                ];
              })
              rust-doc
            ];
          };

          packages = {
            # https://github.com/nix-community/naersk/tree/master/examples/cross-windows
            x86_64-pc-windows-gnu = naersk'.buildPackage {
              src = ./unitas-rs;
              strictDeps = true;

              depsBuildBuild = with pkgs; [
                pkgsCross.mingwW64.stdenv.cc
                pkgsCross.mingwW64.windows.pthreads
              ];

              nativeBuildInputs = with pkgs; [
                # We need Wine to run tests:
                wineWowPackages.stable
              ];

              doCheck = true;

              # Tells Cargo that we're building for Windows.
              # (https://doc.rust-lang.org/cargo/reference/config.html#buildtarget)
              CARGO_BUILD_TARGET = "x86_64-pc-windows-gnu";

              # Tells Cargo that it should use Wine to run tests.
              # (https://doc.rust-lang.org/cargo/reference/config.html#targettriplerunner)
              CARGO_TARGET_X86_64_PC_WINDOWS_GNU_RUNNER = pkgs.writeScript "wine-wrapper" ''
                export WINEPREFIX="$(mktemp -d)"
                exec wine64 $@
              '';
            };
            i686-pc-windows-gnu = naersk'.buildPackage {
              src = ./unitas-rs;
              strictDeps = true;

              depsBuildBuild = with pkgs; [
                pkgsCross.mingw32.stdenv.cc
                pkgsCross.mingw32.windows.pthreads
              ];

              nativeBuildInputs = with pkgs; [
                # We need Wine to run tests:
                wineWowPackages.stable
              ];

              doCheck = true;

              # Tells Cargo that we're building for Windows.
              # (https://doc.rust-lang.org/cargo/reference/config.html#buildtarget)
              CARGO_BUILD_TARGET = "i686-pc-windows-gnu";

              # Tells Cargo that it should use Wine to run tests.
              # (https://doc.rust-lang.org/cargo/reference/config.html#targettriplerunner)
              CARGO_TARGET_I686_PC_WINDOWS_GNU_RUNNER = pkgs.writeScript "wine-wrapper" ''
                export WINEPREFIX="$(mktemp -d)"
                exec wine32 $@
              '';
            };
          };
        };
    };
}
