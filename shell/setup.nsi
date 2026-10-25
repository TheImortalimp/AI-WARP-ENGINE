Unicode true
!include "MUI2.nsh"
!ifndef VERSION
  !define VERSION "6.4.11"
!endif
!ifndef SRC
  !define SRC "publish"
!endif
Name "AI WARP ENGINE"
OutFile "AI-WARP-ENGINE-${VERSION}-x64-setup.exe"
RequestExecutionLevel user
InstallDir "$DESKTOP\AI WARP ENGINE"
BrandingText "AI WARP ENGINE - River Lyle Reuveni - imortalimp.nl"
VIProductVersion "6.4.11.0"
VIAddVersionKey "ProductName" "AI WARP ENGINE"
VIAddVersionKey "ProductVersion" "${VERSION}"
VIAddVersionKey "FileVersion" "${VERSION}"
VIAddVersionKey "FileDescription" "AI WARP ENGINE Installer - glitch visual synthesizer and audio-reactive media playground"
VIAddVersionKey "CompanyName" "River Lyle Reuveni (Imortalimp)"
VIAddVersionKey "LegalCopyright" "(c) 2026 River Lyle Reuveni - imortalimp.nl - CC0 1.0 / The Unlicense"
VIAddVersionKey "Comments" "https://imortalimp.nl - https://github.com/TheImortalimp"
Icon "app.ico"
UninstallIcon "app.ico"
!define MUI_ICON "app.ico"
!define MUI_UNICON "app.ico"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE.txt"
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\AI-Warp-Engine.exe"
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

Section "Install"
  SetOutPath "$INSTDIR"
  File /r "${SRC}\*.*"
  File "LICENSE.txt"
  WriteUninstaller "$INSTDIR\uninstall.exe"
  CreateDirectory "$SMPROGRAMS\AI WARP ENGINE"
  CreateShortcut "$SMPROGRAMS\AI WARP ENGINE\AI WARP ENGINE.lnk" "$INSTDIR\AI-Warp-Engine.exe"
  CreateShortcut "$SMPROGRAMS\AI WARP ENGINE\Uninstall.lnk" "$INSTDIR\uninstall.exe"
  CreateShortcut "$INSTDIR\AI WARP ENGINE.lnk" "$INSTDIR\AI-Warp-Engine.exe" "" "$INSTDIR\AI-Warp-Engine.exe" 0
  WriteRegStr HKCU "Software\AI-WARP-ENGINE" "InstallDir" "$INSTDIR"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\AI-WARP-ENGINE" "DisplayIcon" "$INSTDIR\AI-Warp-Engine.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\AI-WARP-ENGINE" "DisplayName" "AI WARP ENGINE"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\AI-WARP-ENGINE" "DisplayVersion" "${VERSION}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\AI-WARP-ENGINE" "Publisher" "River Lyle Reuveni (TheImortalimp)"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\AI-WARP-ENGINE" "UninstallString" "$INSTDIR\uninstall.exe"
SectionEnd

Section "Uninstall"
  RMDir /r "$INSTDIR"
  RMDir /r "$SMPROGRAMS\AI WARP ENGINE"
  DeleteRegKey HKCU "Software\AI-WARP-ENGINE"
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\AI-WARP-ENGINE"
SectionEnd
