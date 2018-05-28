$branch = & git rev-parse --abbrev-ref HEAD
$branch = $branch.Trim()
& "Photon.CLI\bin\Debug\PhotonCLI.exe" build run -f="Tasks\Publish_Win.json" -r="$branch"
