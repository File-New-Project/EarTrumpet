$process = Get-Process -Name EarTrumpet -ErrorAction SilentlyContinue
if ($process) {
  $process | Stop-Process -Force
}