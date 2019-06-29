cd $PSScriptRoot

# Goal: Keep all versions in sync with the master version.txt
$newVersion = Get-Content -raw ..\version.txt
write-host "Version: $newVersion from version.txt"

# Update Package.appxmanifest
$PackageManifestPath = "..\EarTrumpet.Package\Package.appxmanifest"
$xml = [xml](Get-Content $PackageManifestPath)
$xml.Package.Identity.Version = "$newVersion"
$xml.Save((Resolve-Path ("$PSScriptRoot\$PackageManifestPath")))

# Update AssemblyInfo.cs
$AssemblyInfoPath = ".\Properties\AssemblyInfo.cs"
$assemblyInfo = Get-Content -raw $AssemblyInfoPath
if ($assemblyInfo -match ".*?Version\(`"(.*?)`"\).*?") {
	$assemblyInfo = $assemblyInfo.Replace($Matches[1], $newVersion);
	Set-Content -Path $AssemblyInfoPath -Value $assemblyInfo.Trim();
} else {
	throw "Unexpected lack of AssemblyInfo.cs match when searching for version";
}