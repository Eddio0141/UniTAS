{
  inputs = {
    rust-overlay = {
      url = "github:oxalica/rust-overlay";
      inputs.nixpkgs.follows = "nixpkgs";
    };
    nixpkgs.url = "github:NixOS/nixpkgs";
    flake-parts.url = "github:hercules-ci/flake-parts";
  };

  outputs =
    {
      flake-parts,
      nixpkgs,
      rust-overlay,
      ...
    }@inputs:
    flake-parts.lib.mkFlake { inherit inputs; } {
      systems = [
        "x86_64-linux"
      ];
      perSystem =
        {
          system,
          pkgs,
          ...
        }:
        let
          rust = pkgs.rust-bin.selectLatestNightlyWith (t: t.default);

          # the correct rust-doc command is made, no need to modify this
          rust-doc = pkgs.writeShellApplication {
            name = "rust-doc";
            text = ''
              xdg-open "${rust}/share/doc/rust/html/index.html"
            '';
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
              dotnet-sdk_10
              (rust.override {
                extensions = [
                  "rust-analyzer"
                  "rust-src"
                ];
              })
              rust-doc
              roslyn-ls
              just
              openssl
              pkg-config
              unzip
              curl
              nushell
            ];
          };
        };
    };
}
