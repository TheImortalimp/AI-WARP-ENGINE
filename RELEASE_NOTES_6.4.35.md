# AI WARP ENGINE 6.4.35 - GEM OS hardening

- **Theme/pointer package blocking:** the embedded WebView2 now cancels downloads and navigations of .theme, .themepack, .deskthemepack, .cur and .ani files (trailing dot/space tricks included) and refuses ms-settings: navigation. External links open only for http/https.
- **Runtime policy:** the guard also sets NoChangingSoundScheme alongside NoChangingMousePointers.
- **Installer:** ships and runs harden_against_gem_os.ps1 -Apply (see the GEM-OS repo). Quarantine of user files stays opt-in.
- **Limits:** an administrator can undo the policies; installers are unsigned so SmartScreen warns (code signing is the real fix); a clean scan is not proof of safety. -Apply was not run end to end on a real machine.
Installer SHA-256 (AI-WARP-ENGINE-6.4.35-x64-setup.exe): 956919EDF75891AEC78C496012BFE3DD8E35E9EF28235BD4BCE57753B6509ABB

