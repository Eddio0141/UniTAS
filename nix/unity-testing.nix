{ pkgs }:
pkgs.buildFHSEnv {
  name = "unity-testing-fhs-env";

  multiPkgs =
    pkgs: with pkgs; [
      cups
      expat
      alsa-lib
      nspr
      dbus
      libxdamage
      libxtst
      nss
      libxrandr
      libxcursor
      libx11
      libglvnd
      gdk-pixbuf
      glib
      gtk2
      libGLU
      gnome2.GConf
      libcap
      libxi
      libxrender
      glibc_multi.static
    ];
}
