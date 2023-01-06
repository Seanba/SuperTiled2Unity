# Powershell script that zips ST2U into a package to be installed by Unity Package Manager

try
{
    Push-Location $PSScriptRoot
    
    Write-Host Packaging Super Tiled2Unity

}
finally
{
    Pop-Location
}