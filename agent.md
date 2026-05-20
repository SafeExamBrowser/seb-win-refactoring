# Agent Activity Log

## Goal

Rebrand the Safe Exam Browser (SEB) Windows application to **Xolock** by **Xobin Technologies Pvt. Ltd.**

## Environment

- Workspace: `D:\seb-win-refactoring`
- Solution: `SafeExamBrowser.sln`
- Build target: `Debug|x64`
- Source logo: `D:\seb-win-refactoring\XobinLogo.png`

---

## STEP 1 — Branding Discovery (COMPLETED)

Scanned the entire repository and cataloged all branding locations:
- 49 AssemblyInfo.cs files
- 14 I18n XML resource files (en, de, fr, es, it, nl, ro, tr, zh, ja, ru, sv, et, id)
- 10 .resx resource files
- 163 XAML UI files
- 11 WiX installer files (.wxs/.wxi/.wxl)
- 18 image/icon assets (.ico, .png, .bmp)
- C# code-behind files with branding strings
- Service installer with display name
- License.rtf in installer resources

## STEP 2 — Brand Assets Generated (COMPLETED)

All 18 image/icon assets generated from XobinLogo.png:

| Asset | Path | Size |
|-------|------|------|
| Main app ICO | SafeExamBrowser.Client\SafeExamBrowser.ico | 370 KB |
| Runtime ICO | SafeExamBrowser.Runtime\SafeExamBrowser.ico | 370 KB |
| Service ICO | SafeExamBrowser.Service\SafeExamBrowser.ico | 370 KB |
| Desktop UI ICO | SafeExamBrowser.UserInterface.Desktop\Images\SafeExamBrowser.ico | 370 KB |
| Desktop Log ICO | SafeExamBrowser.UserInterface.Desktop\Images\LogNotification.ico | 93 KB |
| Mobile UI ICO | SafeExamBrowser.UserInterface.Mobile\Images\SafeExamBrowser.ico | 370 KB |
| Mobile Log ICO | SafeExamBrowser.UserInterface.Mobile\Images\LogNotification.ico | 93 KB |
| Reset ICO | SafeExamBrowser.ResetUtility\ResetUtility.ico | 59 KB |
| Config Tool ICO | SebWindowsConfig\ConfigurationTool.ico | 53 KB |
| Setup App ICO | Setup\Resources\Application.ico | 370 KB |
| Setup Config ICO | Setup\Resources\ConfigurationFile.ico | 370 KB |
| Setup Tool ICO | Setup\Resources\ConfigurationTool.ico | 53 KB |
| Setup Reset ICO | Setup\Resources\ResetUtility.ico | 59 KB |
| Desktop Splash | SafeExamBrowser.UserInterface.Desktop\Images\SplashScreen.png | 40 KB |
| Mobile Splash | SafeExamBrowser.UserInterface.Mobile\Images\SplashScreen.png | 40 KB |
| Installer Logo | SetupBundle\Resources\Logo.png | 9 KB |
| Installer Banner | Setup\Resources\Banner.bmp | 86 KB |
| Installer Dialog | Setup\Resources\Dialog.bmp | 462 KB |

## STEP 3 — Application Metadata Updated (COMPLETED)

47 AssemblyInfo.cs files updated:
- `AssemblyDescription`: "Safe Exam Browser" → "Xolock"
- `AssemblyCompany`: "ETH Zürich" → "Xobin Technologies Pvt. Ltd."
- `AssemblyCopyright`: "Copyright © 2026 ETH Zürich, IT Services" → "Copyright © 2026 Xobin Technologies Pvt. Ltd."
- `AssemblyTitle` (Runtime): "Safe Exam Browser" → "Xolock"
- `AssemblyProduct` (Runtime): "Safe Exam Browser" → "Xolock"
- `AssemblyTitle` (Config): "SEB Configuration Tool" → "Xolock Configuration Tool"

## STEP 4 — UI Branding Updated (COMPLETED)

### XAML Files
- RuntimeWindow title: "Safe Exam Browser" → "Xolock" (Desktop + Mobile)
- LockScreen heading: "SEB LOCKED" → "XOLOCK LOCKED" (Desktop + Mobile)

### I18n XML Resources (14 files)
All user-facing "SEB" and "Safe Exam Browser" strings replaced with "Xolock" across all 14 language files.
Preserved: SEB-Server, SEB Verificator (separate products)

### .resx Files (5 files)
- SEBUIStrings.resx + de.resx: all product name strings updated
- SebPasswordDialogForm.resx + de.resx: dialog title updated
- SebWindowsConfigForm.resx: tooltip descriptions updated

### WinForms Designer Files
- SebWindowsConfigForm.Designer.cs: form title, labels, tooltips, dialog titles updated
- AdditionalResources.Designer.cs: tooltip updated

### C# Code Files
- SebWindowsConfigForm.cs: status bar text, file dialog filter
- Installer.cs: Service DisplayName → "Xolock Service"
- Various code-behind files: user-facing string literals updated

## STEP 5 — Icons & Images Replaced (COMPLETED)

All 18 image assets replaced with Xobin-branded versions (see Step 2).

## STEP 6 — Installer Branding Updated (COMPLETED)

- `Setup\Product.wxs`: Product Name, Manufacturer, Feature Title, Exit dialog text
- `Setup\Shortcuts.wxs`: "Safe Exam Browser" → "Xolock", "SEB Configuration Tool" → "Xolock Configuration Tool", "SEB Reset Utility" → "Xolock Reset Utility"
- `Setup\Directories.wxs`: Install folder "SafeExamBrowser" → "Xolock", Start menu folder
- `SetupBundle\Bundle.wxs`: Bundle Name
- `Setup\Resources\License.rtf`: Product name and company references

## STEP 7 — Taskbar & System Tray Branding (COMPLETED)

- Taskbar window icon: replaced with Xobin logo ICO
- RuntimeWindow title: "Xolock"
- Service DisplayName: "Xolock Service"
- All window icons use the new SafeExamBrowser.ico (Xobin-branded)

## STEP 8 — Application Blacklist Additions (COMPLETED)

25 new entries added to `SafeExamBrowser.Configuration\ConfigurationData\DataValues.cs`:

| Application | Executable |
|-------------|-----------|
| Quiz Solver AI | QuizSolverAI.exe |
| QuizSolve | QuizSolve.exe |
| Test Bro | TestBro.exe |
| Apex Vision AI | ApexVisionAI.exe |
| Mindko | Mindko.exe |
| College Tools | CollegeTools.exe |
| Answer.AI | AnswerAI.exe |
| Quizzard | Quizzard.exe |
| QuizWiz | QuizWiz.exe |
| QuestionAI | QuestionAI.exe |
| Merlin | Merlin.exe |
| Sider | Sider.exe |
| SnapGPT | SnapGPT.exe |
| Homework Helper | HomeworkHelper.exe |
| Quiz Wizard | QuizWizard.exe |
| AnswersAI | AnswersAI.exe |
| Study Genie | StudyGenie.exe |
| Study Point AI | StudyPointAI.exe |
| Quizgecko | Quizgecko.exe |
| Quiz Genius AI | QuizGeniusAI.exe |
| Perplexity | Perplexity.exe |
| Microsoft Copilot | Copilot.exe |
| AI Homework Helper | AIHomeworkHelper.exe |
| Canvas Quiz Solver | CanvasQuizSolver.exe |
| ChatGPT | ChatGPT.exe |

All entries have `AutoTerminate = true`.

## STEP 9 — Build Verification (COMPLETED)

Two successful builds (exit code 0):
- Initial build after all branding changes
- Final rebuild after I18n cleanup

Only pre-existing warnings (MSB3884 ruleset + integrity module).
No compile errors. No missing resources.

## STEP 10 — Final Validation

### Remaining "SEB" References (Intentional)

These are categorized and intentionally preserved:

**Internal References (Namespaces/APIs):**
- `SafeExamBrowser.*` namespaces — deeply coupled, unsafe to rename
- `nameof(SafeExamBrowser)` — used for service name, mutex names
- `AppConfig.BASE_ADDRESS` = `net.pipe://localhost/safeexambrowser`
- `AppConfig.CLIENT_MUTEX_NAME` = `safe_exam_browser_client_mutex`
- Solution/project file names (SafeExamBrowser.sln, etc.)

**Compatibility Requirements:**
- `seb://` and `sebs://` URI schemes — protocol handlers
- `.seb` file extension — configuration file format
- `SebClientSettings.seb` — default config file name
- `application/seb` MIME type

**Third-Party Product References:**
- "SEB-Server" — separate server product, kept as-is
- "SEB Verificator" — separate verification app, kept as-is
- `safeexambrowser.org` URLs — kept as-is

**Variable/Control Names (Code Identifiers):**
- `lblSEBPassword`, `txtSEBPassword` — WinForms control names
- `openFileDialogSebConfigFile` — WinForms control name
- Various `Seb*` prefixed variable/class names

### Files Not Renamed (Safe Decision)
- Solution file: `SafeExamBrowser.sln`
- Project folders: `SafeExamBrowser.*`
- Output executables: `SafeExamBrowser.exe`, `SafeExamBrowser.Client.exe`
- These are internal identifiers and renaming would break many cross-references

---

## Previous Session Notes

- Debug integrity bypass in `SafeExamBrowser.Runtime\Operations\Bootstrap\ApplicationIntegrityOperation.cs` is preserved
- The proper production fix is to provide the native integrity module `C:\SEB\seb_x64.dll`

---

## STEP 11 — Applicant Landing Page and Force Quit (COMPLETED)

Implemented a browser-first applicant landing flow before assessment URLs load:
- Added `SafeExamBrowser.Browser\Content\LandingPageFactory.cs` to generate a local landing page asking for full name and email.
- Updated browser startup URL generation so the selected start URL, including command-line assessment links such as Xobin invite URLs, is wrapped by the landing page first.
- On form submit, applicant details are saved temporarily to `applicant.json` in the app temp directory and the browser then navigates to the original assessment URL unchanged.
- Expanded the landing page with assessment ID and invite token fields that build `http://xobinteam.xobin.com/wc/assessment/<assessment_id>?inviteToken=<token>`.
- Added a direct invite link field; if populated, it takes priority and redirects the candidate to that specific invite URL.
- Updated request filtering so the generated local `data:` landing page is not blocked while preserving filtering for the final assessment URL.
- Changed the default start URL from `https://www.safeexambrowser.org/start` to `https://xobinteam.xobin.com/`, so default launches show the Xolock applicant landing flow instead of the Safe Exam Browser page.
- Updated `SebWindowsConfig\SEBSettings.cs` so newly generated/default configuration files also use `https://xobinteam.xobin.com/` instead of the SEB unconfigured page.
- Added runtime normalization in `SafeExamBrowser.Configuration\ConfigurationData\DataProcessor.cs` so old `safeexambrowser.org/start` configuration values are replaced with the Xolock landing start target.

Force quit check:
- Existing force quit shortcut is `Ctrl+Q`, implemented by `SafeExamBrowser.UserInterface.Shared\Activators\TerminationActivator.cs`.
- Ensured termination remains enabled during configuration processing so the `Ctrl+Q` shortcut stays available even when loaded settings disable normal termination.

Verification:
- IDE lints were clean for all changed files.
- Full build could not be run in this shell because neither `msbuild` nor `dotnet` is available on PATH, and Visual Studio MSBuild was not found via `vswhere`.

## STEP 12 — Configuration Tool Launcher (COMPLETED)

- Built and launched the configuration tool from `SebWindowsConfig\bin\x64\Debug\SEBConfigTool.exe`.
- Added `sebconfig.bat` at the repository root to start the SEB/Xolock configuration tool.
- The launcher checks whether `SEBConfigTool.exe` exists and asks to build `SebWindowsConfig` in `Debug|x64` if missing.

## STEP 13 — Landing Invite Configuration File (COMPLETED)

- Added `xolock-landing-invite.seb` at the repository root.
- The configuration uses the Xobin invite URL as `startURL`, so the Xolock landing page opens first and redirects to the invite link after applicant details are submitted.
- Enabled fullscreen browser mode and create-new-desktop kiosk mode.
- Kept `allowQuit` enabled so the existing `Ctrl+Q` force-quit shortcut remains available.
- Updated `start.bat` to convert an existing `.seb` argument to an absolute path before launching, because the runtime ignores relative command-line configuration paths.
- Reworked `xolock-landing-invite.seb` to start on `https://xobinteam.xobin.com/` in windowed mode so the candidate enters assessment ID and invite token before navigation.
- Added `configstart.bat` to launch `SafeExamBrowser.exe` directly with `xolock-landing-invite.seb`.
- Updated `xolock-landing-invite.seb` to start directly at `https://xobinteam.xobin.com/wc/assessment/19TSU98PJSJC?inviteToken=A6F1D020EC27AB8BGKGL66C4499FA9BD1679B5DE96324B829AE68693DDB888BA0A1B1BA939`.
- Adjusted landing-page wrapping so Xobin assessment invite URLs open directly while neutral Xobin URLs still show the applicant landing form.
- Reverted `xolock-landing-invite.seb` back to neutral startup URL `https://xobinteam.xobin.com/` with `browserViewMode=0`, `createNewDesktop=false`, and `killExplorerShell=false` so startup remains windowed and non-kiosk before the candidate enters tokens.

## STEP 14 — Landing Page Removed (COMPLETED)

- Removed the generated applicant landing page from the browser project.
- Removed browser-side applicant data handling and temporary `applicant.json` persistence.
- Restored browser startup to load the configured `startURL` directly.
- Updated `xolock-landing-invite.seb` to open the Xobin assessment invite link directly while keeping `browserViewMode=0`, `createNewDesktop=false`, and `killExplorerShell=false`.
- Updated `xolock-landing-invite.seb` to complete fullscreen locked mode: `browserViewMode=1`, `createNewDesktop=true`, `showTaskBar=false`, and `mainBrowserWindowWidth/Height=100%`.

## STEP 15 — Windowed Landing Page With Assessment Redirect (COMPLETED)

- Added standalone landing page assets at the repository root:
  - `xolock-landing.html`
  - `xolock-landing.css`
  - `xolock-landing.js`
- The landing form collects candidate name, email, assessment ID, and invite token.
- The candidate can alternatively paste a complete link in the required format:
  `https://xobinteam.xobin.com/wc/assessment/<assessment_id>?inviteToken=<invite_token>`.
- The JavaScript validates the link format, stores the entered candidate/assessment data in browser `localStorage`, and redirects to the resolved Xobin assessment URL.
- Updated `xolock-landing-invite.seb` so `startURL` opens `file:///D:/seb-win-refactoring/xolock-landing.html`.
- Kept startup in non-kiosk browser-window mode with `browserViewMode=0`, `createNewDesktop=false`, `showTaskBar=true`, and a `900x760` main browser window so the candidate sees the landing form on the normal desktop.
- Added `EnterFullscreenMode()` to the browser window UI contract and implementations.
- Updated browser navigation handling so the main browser window switches to fullscreen kiosk-style mode when it navigates to a valid Xobin assessment URL with an `inviteToken`.

## STEP 16 — Landing Clipboard and Browser Window Icon (COMPLETED)

- Updated `xolock-landing-invite.seb` to allow the normal system clipboard by setting `enablePrivateClipboard=false` and `clipboardPolicy=0`, so candidates can copy/paste invite links or tokens from their email while the landing form is in non-kiosk mode.
- Updated clipboard monitoring so the system clipboard is not cleared on startup or shutdown when the active policy is `Allow`.
- Added `XobinLogo.png` as a Desktop and Mobile UI resource.
- Updated the Desktop and Mobile browser window XAML title-bar icon from `SafeExamBrowser.ico` to `XobinLogo.png`, replacing the visible SEB logo beside the window buttons.

## STEP 17 — Post-Landing Kiosk Transition (COMPLETED)

- Strengthened the existing Xobin assessment URL detection in the browser window flow: after the landing form redirects to `https://xobinteam.xobin.com/wc/assessment/...?...inviteToken=...`, the main browser window enters fullscreen mode automatically.
- Updated Desktop and Mobile fullscreen handling to behave like kiosk mode after the redirect by covering the full primary screen, removing the window frame, hiding the taskbar entry, keeping the window topmost, and collapsing the browser toolbar.
- Preserved the non-kiosk windowed landing form startup in `xolock-landing-invite.seb`; kiosk-style fullscreen now applies only after the invite URL is reached.
