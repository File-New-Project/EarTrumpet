cd $PSScriptRoot
$xml = [xml](Get-Content ..\EarTrumpet.UWP\Package.appxmanifest)
$v = $xml.Package.Identity.Version
Set-Content .\Assets\DevVersion.txt $v