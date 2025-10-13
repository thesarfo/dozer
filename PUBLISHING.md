# Publishing Guide for Dozer ORM

This guide explains how to package and publish Dozer ORM to NuGet.

## Prerequisites

1. **.NET SDK 8.0** or later
2. **NuGet account** - Create one at [nuget.org](https://www.nuget.org/)
3. **API Key** - Generate from your NuGet account settings

## Step 1: Update Version

Update the version in `Dozer.Core/Dozer.Core.csproj`:

```xml
<Version>1.0.0</Version>
```

Also update `CHANGELOG.md` with the new version and changes.

## Step 2: Update Package Metadata

In `Dozer.Core/Dozer.Core.csproj`, update:

```xml
<Authors>Your Name</Authors>
<Company>Your Company</Company>
<PackageProjectUrl>https://github.com/yourusername/Dozer</PackageProjectUrl>
<RepositoryUrl>https://github.com/yourusername/Dozer</RepositoryUrl>
```

## Step 3: Build the Project

```bash
dotnet build --configuration Release
```

## Step 4: Run Tests (Optional but Recommended)

```bash
dotnet test
```

## Step 5: Pack the NuGet Package

```bash
cd Dozer.Core
dotnet pack --configuration Release --output ./nupkg
```

This will create:
- `Dozer.Core.1.0.0.nupkg` - The main package
- `Dozer.Core.1.0.0.snupkg` - The symbol package (for debugging)

## Step 6: Inspect the Package (Optional)

You can use the NuGet Package Explorer to inspect your package:

```bash
# Install NuGet Package Explorer
dotnet tool install -g NuGetPackageExplorer

# Open your package
nuget-package-explorer ./nupkg/Dozer.Core.1.0.0.nupkg
```

Or manually extract and review:

```bash
# On Windows
Expand-Archive -Path ./nupkg/Dozer.Core.1.0.0.nupkg -DestinationPath ./inspect

# On Linux/Mac
unzip ./nupkg/Dozer.Core.1.0.0.nupkg -d ./inspect
```

## Step 7: Test Package Locally (Recommended)

Create a local NuGet feed to test your package:

```bash
# Create a local feed directory
mkdir ../LocalNuGetFeed

# Copy your package
copy ./nupkg/Dozer.Core.1.0.0.nupkg ../LocalNuGetFeed/

# Add local feed
dotnet nuget add source ../LocalNuGetFeed --name LocalFeed

# Test in a new project
cd ../../TestProject
dotnet new console
dotnet add package Dozer.Core --version 1.0.0
```

## Step 8: Publish to NuGet.org

### Get Your API Key

1. Go to [nuget.org](https://www.nuget.org/)
2. Sign in
3. Go to API Keys
4. Create a new API key with "Push" permission

### Push the Package

```bash
cd Dozer.Core

# Set your API key (do this once)
dotnet nuget push ./nupkg/Dozer.Core.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Or store the API key securely
dotnet nuget setapikey YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Then push without specifying the key
dotnet nuget push ./nupkg/Dozer.Core.1.0.0.nupkg --source https://api.nuget.org/v3/index.json
```

## Step 9: Verify Publication

1. Go to [nuget.org/packages/Dozer.Core](https://www.nuget.org/packages/Dozer.Core)
2. Wait 10-15 minutes for indexing
3. Test installation: `dotnet add package Dozer.Core`

## Alternative: Publish to GitHub Packages

If you want to host on GitHub instead:

```bash
# Authenticate with GitHub
dotnet nuget add source --username YOUR_GITHUB_USERNAME --password YOUR_GITHUB_TOKEN --store-password-in-clear-text --name github "https://nuget.pkg.github.com/YOUR_GITHUB_USERNAME/index.json"

# Push to GitHub Packages
dotnet nuget push ./nupkg/Dozer.Core.1.0.0.nupkg --api-key YOUR_GITHUB_TOKEN --source "github"
```

## Automation with GitHub Actions

Create `.github/workflows/publish.yml`:

```yaml
name: Publish to NuGet

on:
  push:
    tags:
      - 'v*'

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
          
      - name: Restore dependencies
        run: dotnet restore
        
      - name: Build
        run: dotnet build --configuration Release --no-restore
        
      - name: Test
        run: dotnet test --no-build --verbosity normal
        
      - name: Pack
        run: dotnet pack Dozer.Core/Dozer.Core.csproj --configuration Release --output ./nupkg
        
      - name: Publish to NuGet
        run: dotnet nuget push ./nupkg/*.nupkg --api-key ${{secrets.NUGET_API_KEY}} --source https://api.nuget.org/v3/index.json --skip-duplicate
```

Then create a git tag:

```bash
git tag v1.0.0
git push origin v1.0.0
```

## Version Numbering

Follow [Semantic Versioning](https://semver.org/):

- **MAJOR** (1.x.x) - Breaking changes
- **MINOR** (x.1.x) - New features (backward compatible)
- **PATCH** (x.x.1) - Bug fixes

## Checklist Before Publishing

- [ ] Updated version number
- [ ] Updated CHANGELOG.md
- [ ] All tests passing
- [ ] XML documentation complete
- [ ] README.md up to date
- [ ] License file included
- [ ] Package metadata correct
- [ ] Tested locally
- [ ] No sensitive information in code
- [ ] Dependencies up to date

## Common Issues

### Issue: "Package already exists"
**Solution**: Increment the version number. You cannot replace an existing version.

### Issue: "Authentication failed"
**Solution**: Regenerate your API key and try again.

### Issue: "Package validation failed"
**Solution**: Check the error message. Common issues:
- Missing required metadata
- Invalid package ID
- File path issues

### Issue: "Package doesn't appear on NuGet"
**Solution**: Wait 10-15 minutes for indexing. Check validation status in your NuGet account.

## Support

For issues or questions:
- GitHub Issues: [github.com/yourusername/Dozer/issues](https://github.com/yourusername/Dozer/issues)
- NuGet Support: [nuget.org/policies/Contact](https://www.nuget.org/policies/Contact)

