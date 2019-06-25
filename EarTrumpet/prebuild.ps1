param($version)

$PackageManifestPath = "$PSScriptRoot\..\EarTrumpet.Package\Package.appxmanifest"

$xml = [xml](Get-Content $PackageManifestPath)
$xml.Package.Identity.Version = "$version"
$xml.Save($PackageManifestPath)