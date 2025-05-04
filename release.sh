#!/bin/bash
set -e

if [ ! -f version.txt ]; then
    echo "version.txt not found"
    exit 1
fi

version=$(cat version.txt)
IFS='.' read -r major minor patch <<< "$version"
new_patch=$((patch + 1))
new_version="$major.$minor.$new_patch"

echo "$new_version" > version.txt

if [ ! -f BanGuard.cs ]; then
    echo "BanGuard.cs not found"
    exit 1
fi

sed -i "s/$version/$new_version/g" BanGuard.cs

echo "Building project..."
dotnet build

dll_path="bin/Debug/net6.0/BanGuard.dll"
if [ ! -f "$dll_path" ]; then
    echo "Build failed or $dll_path not found"
    exit 1
fi

git add version.txt BanGuard.cs
git commit -m "Release $new_version"
git tag "$new_version"
git push
git push origin "$new_version"

echo "Released version $new_version"

