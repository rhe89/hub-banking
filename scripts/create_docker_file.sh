#!/usr/bin/env bash

nuget_access_token=$1

read_project_file() 
{
    csproj_file=$1

    while IFS= read -r line
    do
    if [[ $line == *"ProjectReference"* ]]; then

        ref=${line##*\\}
        ref=${ref##*=}
        ref=${ref%.csproj*}
        
        echo "$(read_project_file $(find ../$ref/$ref.csproj))"

        echo "$ref"
    fi
    done < "$csproj_file"
}

#Find project file in folder
csproj_file=$(find ./*.csproj)

project_name=${csproj_file##./}
project_name=${project_name%.csproj*}

# Read all project references in "project reference tree" recursively 
project_references+=("$(read_project_file "$csproj_file")")

project_references_unique=($(echo "${project_references[@]}" | tr ' ' '\n' | sort -u | tr '\n' ' '))

# Create docker file
echo FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env > Dockerfile

echo "" >> Dockerfile

echo WORKDIR /app >> Dockerfile

echo "" >> Dockerfile

 echo COPY ./"$project_name"/"$project_name".csproj ./"$project_name"/"$project_name".csproj >> Dockerfile

for project_reference in "${project_references_unique[@]}"
do 
    echo COPY ./"$project_reference"/"$project_reference".csproj ./"$project_reference"/"$project_reference".csproj >> Dockerfile
done

echo "" >> Dockerfile

echo RUN dotnet nuget add source https://nuget.pkg.github.com/rhe89/index.json -n github -u rhe89 -p "$nuget_access_token" --store-password-in-clear-text >> Dockerfile

echo "" >> Dockerfile

echo RUN dotnet restore ./"$project_name"/"$project_name".csproj -p:HideWarningsAndErrors=true -p:EmitAssetsLogMessages=false >> Dockerfile

echo "" >> Dockerfile

echo COPY /"$project_name"/ ./"$project_name"/ >> Dockerfile

for project_reference in "${project_references_unique[@]}"
do 
    echo COPY /"$project_reference"/ ./"$project_reference"/ >> Dockerfile
done

echo "" >> Dockerfile

echo RUN dotnet publish ./"$project_name"/"$project_name".csproj -c Release -o out >> Dockerfile

echo "" >> Dockerfile

echo FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 >> Dockerfile
echo WORKDIR /app >> Dockerfile
echo COPY --from=build-env /app/out . >> Dockerfile
echo ENTRYPOINT [\"dotnet\", \""$project_name".dll\"] >> Dockerfile