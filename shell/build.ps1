param([string]$FFmpegRoot = $env:AIWARP_FFMPEG_ROOT)
$ErrorActionPreference = 'Stop'
if (-not $FFmpegRoot) {
    $executable = (Get-Command ffmpeg.exe -ErrorAction Stop).Source
    $FFmpegRoot = Split-Path (Split-Path $executable -Parent) -Parent
}
$FFmpegRoot = (Resolve-Path $FFmpegRoot).Path
foreach ($file in @('bin\ffmpeg.exe', 'LICENSE', 'README.txt')) {
    if (-not (Test-Path (Join-Path $FFmpegRoot $file) -PathType Leaf)) {
        throw "FFmpeg distribution is missing $file. Supply a complete distribution, including license and build/source notices."
    }
}
& dotnet publish (Join-Path $PSScriptRoot 'AiWarpEngine.csproj') -c Release -r win-x64 --self-contained true -o (Join-Path $PSScriptRoot 'publish') "-p:FFmpegRoot=$FFmpegRoot"
if ($LASTEXITCODE -ne 0) { throw 'Windows app publish failed.' }
