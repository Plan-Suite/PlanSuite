$foldersToRemove = @(
    "scripts",
    "wwwroot",
    "index.html",
    "webpack.common.cjs",
    "webpack.common.cjs.map",
    "webpack.dev.cjs",
    "webpack.dev.cjs.map",
    "webpack.prod.cjs",
    "webpack.prod.cjs.map"
)

$projectDir = $args[0]
$configuration = $args[1]
$jsFolderPath = Join-Path -Path $projectDir -ChildPath "wwwroot/js"

$webpackFile = "webpack.dev.cjs";

if($configuration == "Release")
{
    $webpackFile = "webpack.prod.cjs";
}

Write-Host "Running $webpackFile..."
npx webpack --config $webpackFile

foreach ($folder in $foldersToRemove)
{
    $folderPath = Join-Path -Path $jsFolderPath -ChildPath $folder

    if (Test-Path $folderPath)
    {
        Write-Host "Removing: $folderPath"
        Remove-Item -Path $folderPath -Recurse -Force
    }
}