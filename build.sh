#!/usr/bin/env bash
set -ex
## x = exit immediately if a pipeline returns a non-zero status.
## e = print a trace of commands and their arguments during execution.
## See: http://www.gnu.org/software/bash/manual/html_node/The-Set-Builtin.html#The-Set-Builtin

# ----- Variables -------------------------------------------------------------
# Variables in the build.properties file will be available to Jenkins
# build steps. Variables local to this script can be defined below.
. ./build.properties

# fix for jenkins inserting the windows-style path in $WORKSPACE
cd "$WORKSPACE"
export WORKSPACE=`pwd`



# ----- Common ----------------------------------------------------------------
# Common build script creates functions and variables expected by Jenkins.
if [ -d $WORKSPACE/../build-tools ]; then
  ## When script directory already exists, just update when there are changes.
  cd $WORKSPACE/../build-tools
  git fetch && git stash
  if ! git log HEAD..origin/master --oneline --quiet; then
    git pull
  fi
  cd $WORKSPACE
else
  git clone https://github.com/versionone/openAgile-build-tools.git $WORKSPACE/../build-tools
fi
source ../build-tools/common.sh



# ---- Produce .NET Metadata --------------------------------------------------

function create_assemblyinfo_fs() {
  cat > $1 <<EOF
module AssemblyInfo

open System.Reflection

[<assembly: AssemblyTitle("$PRODUCT_TITLE")>]
[<assembly: AssemblyConfiguration("$Configuration")>]
[<assembly: AssemblyCompany("$ORGANIZATION_NAME")>]
[<assembly: AssemblyProduct("$PRODUCT_NAME")>]
[<assembly: AssemblyCopyright("Copyright $COPYRIGHT_RANGE, VersionOne, Inc. Please see the LICENSE.MD file.")>]
[<assembly: AssemblyVersion("$VERSION_NUMBER.0")>]
[<assembly: AssemblyFileVersion("$VERSION_NUMBER.$BUILD_NUMBER")>]

ignore ()

EOF
}


COMPONENTS="VersionOne.Parsers"
for COMPONENT_NAME in $COMPONENTS; do
  create_assemblyinfo_fs "$WORKSPACE/$COMPONENT_NAME/AssemblyInfo.fs"
done



# ---- Clean solution ---------------------------------------------------------

MSBuild.exe $SOLUTION_FILE -m \
  -t:Clean \
  -p:Configuration="$Configuration" \
  -p:Platform="$Platform" \
  -p:Verbosity=Diagnostic




# ---- Refresh nuget packages -------------------------------------------------

nuget_packages_refresh



# ---- Build solution using msbuild -------------------------------------------

WIN_SIGNING_KEY="`winpath "$SIGNING_KEY"`"
echo $Platform
MSBuild.exe $SOLUTION_FILE \
  -p:SignAssembly=false \
  -p:AssemblyOriginatorKeyFile=$WIN_SIGNING_KEY \
  -p:RequireRestoreConsent=false \
  -p:Configuration="$Configuration" \
  -p:Platform="$Platform" \
  -p:Verbosity=Diagnostic \
  -p:VisualStudioVersion=11.0


# ---- Execute nspec tests -------------------------------------------

# ./tests.sh


