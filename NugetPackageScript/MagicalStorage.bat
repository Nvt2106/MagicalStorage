cd ..
rd NugetPackageTemp /S /Q
md NugetPackageTemp\lib\net45
copy .\MagicalStorage.Core\bin\Debug\MagicalStorage.Core.dll .\NugetPackageTemp\lib\net45
copy .\MagicalStorage.Core\bin\Debug\MagicalStorage.Core.pdb .\NugetPackageTemp\lib\net45
copy .\MagicalStorage.Repository\bin\Debug\MagicalStorage.Repository.dll .\NugetPackageTemp\lib\net45
copy .\MagicalStorage.Repository\bin\Debug\MagicalStorage.Repository.pdb .\NugetPackageTemp\lib\net45
copy .\NugetPackageScript\MagicalStorage.nuspec .\NugetPackageTemp

cd NugetPackageTemp
nuget pack MagicalStorage.nuspec -Version %1

rem copy MagicalStorage.%1.nupkg ..\

nuget push MagicalStorage.%1.nupkg

cd ..
rd NugetPackageTemp /S /Q

nuget delete MagicalStorage %2 -NonInteractive

pause