#!/usr/bin/env bash
csproj_files=($(find * -name *.csproj))

#Loop throuh all project files. If a project should be a deployable project, generate dockerfile for it.
for csproj_file in "${csproj_files[@]}"
do 
    if (grep -q "Project Sdk=\"Microsoft.NET.Sdk.Web\"" $csproj_file) || (grep -q "Project Sdk=\"Microsoft.NET.Sdk.Worker\"" $csproj_file); then
        project_folder=${csproj_file##*/}
        project_folder=${project_folder%.csproj*}

        domain=${project_folder%%.*}

        cd "$project_folder"

        bash ../scripts/create_docker_file.sh

        cd ../
    fi
done