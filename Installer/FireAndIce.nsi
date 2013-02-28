;Fire and Ice Installer
;(c) RRR Software

;--------------------------------
;Includes and Initial Set-Up

  SetCompressor /SOLID lzma
 
  !define MULTIUSER_EXECUTIONLEVEL Highest
  !define MULTIUSER_MUI
  !define MULTIUSER_INSTALLMODE_COMMANDLINE
  !define MULTIUSER_INSTALLMODE_DEFAULT_REGISTRY_KEY "Software\Fire and Ice\%version"
  !define MULTIUSER_INSTALLMODE_DEFAULT_REGISTRY_VALUENAME ""
  !define MULTIUSER_INSTALLMODE_INSTDIR_REGISTRY_KEY "Software\Fire and Ice\%version"
  !define MULTIUSER_INSTALLMODE_INSTDIR_REGISTRY_VALUENAME ""
  !define MULTIUSER_INSTALLMODE_INSTDIR "Fire and Ice %version"
  !include "MultiUser.nsh"
  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Fire and Ice Installer"
  OutFile "fireandice.exe"

  ;Default installation folder
  InstallDir "$LOCALAPPDATA\Fire and Ice"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\Fire and Ice" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "Docs\License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  
  ;Store installation folder
  WriteRegStr HKCU "Software\Fire and Ice" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecDummy ${LANG_ENGLISH} "A test section."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDummy} $(DESC_SecDummy)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...

  Delete "$INSTDIR\Uninstall.exe"

  RMDir "$INSTDIR"

  DeleteRegKey /ifempty HKCU "Software\Fire and Ice"

SectionEnd