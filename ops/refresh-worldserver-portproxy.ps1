$ErrorActionPreference = "Stop"

$wslIp = (wsl.exe -d Ubuntu-24.04 hostname -I).Trim().Split()[0]

if ([string]::IsNullOrWhiteSpace($wslIp)) {
    throw "WSL IPv4 address was not found."
}

Write-Host "WSL IPv4: $wslIp"

netsh interface portproxy delete v4tov4 listenaddress=0.0.0.0 listenport=5080 2>$null
netsh interface portproxy add v4tov4 `
    listenaddress=0.0.0.0 `
    listenport=5080 `
    connectaddress=$wslIp `
    connectport=5080

netsh interface portproxy show all
