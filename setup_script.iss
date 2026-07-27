[Setup]
AppName=Ashpro Exp
AppVersion=1.0
DefaultDirName={pf}\Ashpro Exp
DefaultGroupName=Ashpro Exp
OutputDir=Output
OutputBaseFilename=AshproExpSetup
Compression=lzma
SolidCompression=yes

; Installer icon
SetupIconFile=D:\Ashpro Business\Ashpro Exp New\ExpenseIncomeTracker.Uno\Assets\Icons\Ashpro.ico

; Optional modern wizard style
WizardStyle=modern

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "D:\Ashpro Business\Ashpro Exp New\ExpenseIncomeTracker.Uno\bin\Debug\net10.0-desktop\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]

; Start Menu shortcut
Name: "{group}\Ashpro Exp"; \
Filename: "{app}\ExpenseIncomeTracker.Uno.exe"; \
IconFilename: "{app}\app.ico"

; Desktop shortcut
Name: "{autodesktop}\Ashpro Exp"; \
Filename: "{app}\ExpenseIncomeTracker.Uno.exe"; \
Tasks: desktopicon; \
IconFilename: "{app}\app.ico"

[Run]
Filename: "{app}\ExpenseIncomeTracker.Uno.exe"; \
Description: "Launch Ashpro Exp"; \
Flags: nowait postinstall skipifsilent