Unicode true
!include "MUI2.nsh"
!ifndef VERSION
  !define VERSION "6.4.38.21"
!endif
!ifndef SRC
  !define SRC "publish-native-6.4.38.21"
!endif
Name "AI WARP ENGINE - 6.4.38.21 ULTIMATE"
OutFile "AI-WARP-ENGINE-${VERSION}-x64-setup.exe"
RequestExecutionLevel admin
InstallDir "$DESKTOP\AI WARP ENGINE"
BrandingText "AI WARP ENGINE - River Lyle Reuveni - imortalimp.nl"
VIProductVersion "${VERSION}"
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

!define MUI_WELCOMEPAGE_TITLE "AI WARP ENGINE ${VERSION} Setup"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE.txt"
!insertmacro MUI_PAGE_LICENSE "${SRC}\Microsoft.NETCore.App.Runtime.win-x64-LICENSE.TXT.txt"
!insertmacro MUI_PAGE_LICENSE "WINDOWS-SDK-LICENSE.rtf"
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\AI-Warp-Engine.exe"
!define MUI_FINISHPAGE_TITLE "AI WARP ENGINE ${VERSION} Installed"
!define MUI_FINISHPAGE_TEXT "Setup is complete.$\r$\n$\r$\nClick Finish to close Setup."
!define MUI_FINISHPAGE_RUN_TEXT "Launch AI WARP ENGINE"
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

Section "Install"
  SetOutPath "$INSTDIR"
  File /r "${SRC}\*.*"
  File "LICENSE.txt"
  File "cursor_lockdown.ps1"
  File "harden_against_gem_os.ps1"
  DetailPrint "Locking mouse pointers to the Windows system set..."
  nsExec::ExecToLog '"$SYSDIR\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "$INSTDIR\cursor_lockdown.ps1"'
  Pop $0
    DetailPrint "Applying GEM OS theme/cursor hardening..."
    nsExec::ExecToLog '"$SYSDIR\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "$INSTDIR\harden_against_gem_os.ps1" -Apply'
    Pop $0
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
