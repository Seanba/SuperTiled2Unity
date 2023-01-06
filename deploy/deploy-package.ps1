# Powershell script that zips ST2U into a package to be installed by Unity Package Manager

try
{
    Push-Location $PSScriptRoot

    # Get the current version of Super Tiled2Unity
    $package_dir = '../SuperTiled2Unity/Packages/com.seanba.super-tiled2unity'
    $package_json = "$package_dir/package.json"
    $version = (Get-Content $package_json | ConvertFrom-Json).version
    $output = "super-tiled2unity.v$version.zip"

    Write-Host Packaging Super Tiled2Unity version $version
    Write-Host $output

    $compress = @{
        Path = $package_dir
        CompressionLevel = "Fastest"
        DestinationPath = $output
    }

    Compress-Archive @compress -Force
    Write-Host "Done compressing '$output'"
}
finally
{
    Pop-Location
}