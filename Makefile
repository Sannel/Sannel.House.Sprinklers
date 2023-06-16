arm64:
	rm -rf output
	mkdir output
	dotnet publish -r linux-arm64 --sc -o output/ /p:PublishSingleFile=true src/Sannel.House.Sprinklers/Sannel.House.Sprinklers.csproj
