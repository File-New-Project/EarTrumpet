cd $PSScriptRoot

function UpdateManifest { param($path, $version)
	$xml = [xml](Get-Content $path)
	$xml.Package.Identity.Version = "$version"
	$xml.Save((Resolve-Path ("$PSScriptRoot\$path")))
}

$PackageManifestPath = "..\EarTrumpet.Package\Package.appxmanifest"
$AssemblyInfoPath = ".\Properties\AssemblyInfo.cs"

$newVersion = Get-Content -raw ..\version.txt

UpdateManifest -path $PackageManifestPath -version $newVersion

$assemblyInfo = Get-Content -raw $AssemblyInfoPath

if ($assemblyInfo -match ".*?Version\(`"(.*?)`"\).*?") {
	$assemblyInfo = $assemblyInfo.Replace($Matches[1], $newVersion);
	Set-Content -Path $AssemblyInfoPath -Value $assemblyInfo.Trim();
} else {
	throw "Unexpected lack of AssemblyInfo.cs match when searching for version";
}

Set-Content .\Assets\DevVersion.txt $newVersion

write-host "Version: $newVersion from version.txt"