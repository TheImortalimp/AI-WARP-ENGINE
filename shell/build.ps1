param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'publish-native-6.4.38.1')
)
$ErrorActionPreference = 'Stop'
$output = $OutputDirectory
if (Test-Path $output) {
    throw "Build output already exists: $output. Use a new clean output directory or explicitly remove only this edition's generated output."
}
& dotnet publish (Join-Path $PSScriptRoot 'AiWarpEngine.csproj') -c Release -r win-x64 --self-contained true -o $output
if ($LASTEXITCODE -ne 0) { throw 'Windows native app publish failed.' }
$dependencies = Get-Content -LiteralPath (Join-Path $output 'AI-Warp-Engine.deps.json') -Raw | ConvertFrom-Json
$nuget = Join-Path $env:USERPROFILE '.nuget\packages'
Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($package in @(
    @{ Name = 'Microsoft.NETCore.App.Runtime.win-x64'; Entries = @('LICENSE.TXT', 'THIRD-PARTY-NOTICES.TXT') },
    @{ Name = 'Microsoft.WindowsDesktop.App.Runtime.win-x64'; Entries = @('LICENSE') }
)) {
    $key = $dependencies.libraries.PSObject.Properties.Name |
        Where-Object { $_.StartsWith('runtimepack.' + $package.Name + '/') } | Select-Object -First 1
    if (-not $key) { throw "Runtime package version missing: $($package.Name)" }
    $version = ($key -split '/')[-1]
    $name = $package.Name.ToLowerInvariant()
    $archive = Join-Path $nuget "$name\$version\$name.$version.nupkg"
    $zip = [System.IO.Compression.ZipFile]::OpenRead($archive)
    try {
        foreach ($entryName in $package.Entries) {
            $entry = $zip.GetEntry($entryName)
            if (-not $entry) { throw "Runtime notice missing: $entryName in $archive" }
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry,
                (Join-Path $output ($package.Name + '-' + $entryName + '.txt')), $true)
        }
    } finally { $zip.Dispose() }
}
$webview = Join-Path $nuget 'microsoft.web.webview2\1.0.2792.45'
Copy-Item -LiteralPath (Join-Path $webview 'LICENSE.txt') -Destination (Join-Path $output 'WEBVIEW2-LICENSE.txt')
Copy-Item -LiteralPath (Join-Path $webview 'NOTICE.txt') -Destination (Join-Path $output 'WEBVIEW2-NOTICE.txt')
$forbidden = Get-ChildItem -LiteralPath $output -Recurse -File |
    Where-Object { $_.Name -match 'ffmpeg|ffprobe|ffplay' }
if ($forbidden) { throw 'Native package unexpectedly contains FFmpeg files.' }
