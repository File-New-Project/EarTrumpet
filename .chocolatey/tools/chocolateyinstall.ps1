$ErrorActionPreference = 'Stop'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$installPath = Join-Path $toolsDir 'EarTrumpet'
$exePath = Join-Path $toolsDir 'EarTrumpet\EarTrumpet.exe'
$zipPath = Join-Path $toolsDir 'Release.zip'

Install-ChocolateyZipPackage -PackageName "$packageName" `
                             -Url "$zipPath" `
                             -UnzipLocation "$installPath"
                             
Write-Output "Adding shortcut to Start Menu"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\EarTrumpet.lnk" -TargetPath $exePath

Write-Output "Adding shortcut to Startup"
Install-ChocolateyShortcut -ShortcutFilePath "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\EarTrumpet.lnk" -TargetPath $exePath
