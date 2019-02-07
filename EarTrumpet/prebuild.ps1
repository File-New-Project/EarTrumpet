cd $PSScriptRoot
$xml = [xml](Get-Content ..\EarTrumpet.UWP\Package.appxmanifest)
Set-Content .\Assets\DevVersion.txt ($xml.Package.Identity.Version)