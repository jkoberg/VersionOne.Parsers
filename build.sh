#!/usr/bin/env bash
set -ex
## x = exit immediately if a pipeline returns a non-zero status.
## e = print a trace of commands and their arguments during execution.
## See: http://www.gnu.org/software/bash/manual/html_node/The-Set-Builtin.html#The-Set-Builtin

# ----- Variables -------------------------------------------------------------
# Variables in the build.properties file will be available to Jenkins
# build steps. Variables local to this script can be defined below.
. ./build.properties


#export VERSION=`date -u +"$MAJOR.$MINOR.%Y%m%d.%H%M%S"`
#echo version: $VERSION

FSPROJNAMES="VersionOne.Parsers"


# fix for jenkins inserting the windows-style path in $WORKSPACE
cd "$WORKSPACE"
export WORKSPACE=`pwd`


# ----- Common ----------------------------------------------------------------
# Common build script creates functions and variables expected by Jenkins.
TOOLSDIR="$WORKSPACE/build_tools"
if [ -d "$TOOLSDIR" ]; then
  ## When script directory already exists, just update when there are changes.
  pushd "$TOOLSDIR"
  git fetch && git stash
  if ! git log HEAD..origin/master --oneline --quiet; then
    git pull
  fi
  popd
else
  git clone https://github.com/versionone/openAgile-build-tools.git "$TOOLSDIR"
fi
source "$TOOLSDIR/common.sh"



# ---- Produce .NET Metadata --------------------------------------------------

function create_assemblyinfo_fs() {
  cat > $1 <<EOF
module $PRODUCT_NAME.AssemblyInfo

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


for PROJNAME in $FSPROJNAMES; do
  create_assemblyinfo_fs "$WORKSPACE/$PROJNAME/AssemblyInfo.fs"
done



# ---- Clean solution ---------------------------------------------------------

MSBuild.exe $SOLUTION_FILE -m \
  -t:Clean \
  -p:Configuration="$Configuration" \
  -p:Platform="$Platform" \
  -p:Verbosity=Diagnostic




# ---- Refresh nuget packages -------------------------------------------------

unset PLATFORM
unset CONFIGURATION

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
  -p:Verbosity=Diagnostic


# ---- Execute nspec tests -------------------------------------------

"./packages/NUnit.Runners.2.6.3/tools/nunit-console.exe" \
    "./VersionOne.Parsers.Tests/bin/$Configuration/VersionOne.Parsers.Tests.dll"

