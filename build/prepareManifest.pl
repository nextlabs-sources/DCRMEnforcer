#!/usr/bin/perl
#
# DESCRIPTION
# This script prepare an installer assembly directory for building an installer.
#
# IMPORTANT:
#	1. "Script Files" folder must be in the same directory as *.ism file. Otherwise, you will get
#		an error message like this:
#			ISDEV : error -7132: An error occurred streaming ISSetup.dll support file 
#			c:\nightly\current\D_SiriusR2\install\oe\desktop-enforcer-5.5.0.0-10001-dev-20110321063014\
#			desktop-enforcer-5.5.0.0-10001-dev-20110321063014\Script Files\Setup.inx
#		And you will not see these messages at the beginning:
#			Compiling...
#			Setup.rul
#			c:\nightly\current\D_SiriusR2\install\oe\desktop-enforcer-5.5.0.0-10001-dev-20110321063014\script files\Setup.rul(90) 
#				: warning W7503: 'ProcessEnd' : function defined but never called
#			c:\nightly\current\D_SiriusR2\install\oe\desktop-enforcer-5.5.0.0-10001-dev-20110321063014\script files\Setup.rul(90) 
#				: warning W7503: 'ProcessRunning' : function defined but never called
#			Linking...
#			Setup.inx - 0 error(s), 2 warning(s)
#			ISDEV : warning -4371: There were warnings compiling InstallScript

use strict;
use warnings;

use Getopt::Long;
use File::Copy::Recursive qw(dircopy);
use File::Copy;
use File::stat;

print "Dynamic CRM Enforcer Version Preparation Script\n";


#
# Global variables
#

my	$buildType = "";
my	$buildNum = "";
my  $buildVersion_Major = "";
my  $buildVersion_Minor = "";
my  $buildVersion_Fix = "";


#
# Process parameters
#

# -----------------------------------------------------------------------------
# Print usage

sub printUsage
{
	print "usage: prepareManifest.pl --buildNum=<#> --buildType=<#>\n";
	print "  buildNum        A build number. Can be any numerical or string value.\n";
	print "  buildType       Specify a build type (e.g., release, dev, pcv)\n";
	print "\nEnvironment Variables:\n";
	print "  NLBUILDROOT     Source tree root (e.g., c:/nightly/current/D_SiriusR2).\n";
}

# -----------------------------------------------------------------------------
# Parse command line arguments

sub parseCommandLine()
{
	#
	# Parse arguments
	#
	
	# GetOptions() key specification:
	#	option			Given as --option of not at all (value set to 0 or 1)
	#	option!			May be given as --option or --nooption (value set to 0 or 1)
	#	option=s		Mandatory string parameter: --option=somestring
	#	option:s		Optional string parameter: --option or --option=somestring	
	#	option=i		Mandatory integer parameter: --option=35
	#	option:i		Optional integer parameter: --option or --option=35	
	#	option=f		Mandatory floating point parameter: --option=3.14
	#	option:f		Optional floating point parameter: --option or --option=3.14	

	my	$help = 0;
		
	if (!GetOptions(
			'buildNum=s' => \$buildNum,						# --buildNum
			'buildType=s' => \$buildType,					# --buildType
			'buildVersion_Major=s' => \$buildVersion_Major,	# --buildVersion_Major
			'buildVersion_Minor=s' => \$buildVersion_Minor,	# --buildVersion_Minor
			'buildVersion_Fix=s' => \$buildVersion_Fix,		# --buildVersion_Fix
			'help' => \$help								# --help
		))
	{
		exit(1);
	}

	#
	# Help
	#
	
	if ($help == 1)
	{
		&printHelp();
		exit;
	}

	#
	# Check for errors
	#
	
	if ($buildType eq '')
	{
		print "Missing build type\n";
		exit(1);
	}

	if ($buildType ne "release" && $buildType ne "dev" && $buildType ne "pcv_smdc")
	{
		print "Invalid build type $buildType (expected release, pcv or dev)\n";
		exit(1);
	}
	
	if ($buildVersion_Major eq '')
	{
		print "Missing build version major\n";
		exit(1);
	}
	
	if ($buildVersion_Minor eq '')
	{
		print "Missing build version minor\n";
		exit(1);
	}
	
	if ($buildVersion_Fix eq '')
	{
		print "Missing build version fix\n";
		exit(1);
	}
	
	if ($buildNum eq '')
	{
		print "Missing build number\n";
		exit(1);
	}
}



&parseCommandLine();

# Print parameters
print "Parameters:\n";
print "  Build Type          = $buildType\n";
print "  Build Version Major = $buildVersion_Major\n";
print "  Build Version Minor = $buildVersion_Minor\n";
print "  Build Version Fix   = $buildVersion_Fix\n";
print "  Build #             = $buildNum\n";

#
# Environment
#

my	$buildRootDir = $ENV{NLBUILDROOT};
my	$buildRootPath = $buildRootDir;

$buildRootPath =~ s/:$/:\//;

if (! defined $buildRootDir || $buildRootDir eq "")
{
	die "### ERROR: Environment variable NLBUILDROOT is missing.\n";
}


if (! -d $buildRootPath)
{
	die "### ERROR: $buildRootPath (i.e., NLBUILDROOT) does not exists.\n";
}

# Print environment
print "Environment Variables:\n";
print "  NLBUILDROOT     = $buildRootDir\n";

my	$argCount = scalar(@ARGV);

if ($argCount < 2 || $ARGV[0] eq "-h" || $ARGV[0] eq "--help")
{
	printUsage;
	
    my	$SolitionXML_2015_Managed="$buildRootDir/Install/DCRM2015/NextLabsDCRMSolution_2015_Managed/solution.xml";
    my	$SolitionXML_2015_UnManaged="$buildRootDir/Install/DCRM2015/NextLabsDCRMSolution_2015_Unmanaged/solution.xml";
    my	$SolitionXML_2016_Managed="$buildRootDir/Install/DCRM2016/NextLabsDCRMSolution_2016_Managed/solution.xml";
    my	$SolitionXML_2016_UnManaged="$buildRootDir/Install/DCRM2016/NextLabsDCRMSolution_2016_Unmanaged/solution.xml";
    my	$AssemblyInfoCSPath="$buildRootDir/prod/NextLabsCRM/Properties/AssemblyInfo.cs";
	my  $AssemblyInfoCSForConfiguationWriterPath="$buildRootDir/prod/ConfigurationWriter/Properties/AssemblyInfo.cs";
	my  $AssemblyInfoCSForSPPluginPath="$buildRootDir/prod/DCRMPluginForSharePoint/Properties/AssemblyInfo.cs";

	my $prefix = "assembly: AssemblyVersion\\(\"";
	my $newProdcutVersion = $buildVersion_Major.".".$buildVersion_Minor.".".$buildNum.".0";
	updateVersionInFile($AssemblyInfoCSPath,$newProdcutVersion, $prefix);
	
	$prefix = "<Version>";
	updateVersionInFile($SolitionXML_2015_Managed, $newProdcutVersion,$prefix);
	updateVersionInFile($SolitionXML_2015_UnManaged, $newProdcutVersion,$prefix);
	updateVersionInFile($SolitionXML_2016_Managed, $newProdcutVersion,$prefix);
	updateVersionInFile($SolitionXML_2016_UnManaged, $newProdcutVersion,$prefix);
	
	$prefix = "assembly: AssemblyFileVersion\\(\"";
	#my $newFileVersion = createVersion($AssemblyInfoCSPath,$buildNum,$prefix);
	updateVersionInFile($AssemblyInfoCSPath,$newProdcutVersion, $prefix);
	updateVersionInFile($AssemblyInfoCSForSPPluginPath,$newProdcutVersion, $prefix);
	updateVersionInFile($AssemblyInfoCSForConfiguationWriterPath,$newProdcutVersion, $prefix);
	
	exit 0;
}

sub trim 
{ 
	my $s = shift; 
	$s =~ s/^\s+|\s+$//g; 
	return $s 
}

sub createVersion
{
	my	($file, $buildNum,$prefix) = @_;
	my	$argCount = scalar(@_);
	
	if ($argCount != 3)
	{
		die "### ERROR: Wrong # of arguments (expected 3, got $argCount) to getVersion().\n";
	}
	
	if (! -e $file)
	{
		die "### ERROR: File $file does not exist. Cannot get version from file.\n";
	}
	
	if ($buildNum eq "")
	{
		die "### ERROR: Missing buildNum #. Cannot get version from file.\n";
	}
	
	open FILE, $file || die "Error opening ReadMe file $file\n";

	my $version = "";
	while (my $buf = <FILE>)
	{

		$buf =~ s/(.*)($prefix)(\d+\.\d+\.\d+\.)(\d+)(.*)/$3$buildNum/g;
		
		#if regex cannot match, $2 will be undefined
		if(defined($2)){
			$version = $buf;
			last;
		}
	}
	
	close FILE;
	
	$version = trim($version);
	if ($version eq "")
	{
		die "### ERROR: Cannot find version in file.\n";
	}

	$version;
}

sub updateVersionInFile
{
	my	($file, $newVersion,$prefix) = @_;
	
	print "INFO: Updating version in file $file.\n";
	print "  New version #      = $newVersion\n";

	# Check for errors
	my	$argCount = scalar(@_);
	
	if ($argCount != 3)
	{
		die "### ERROR: Wrong # of arguments (expected 3, got $argCount) to updateVersionInFile().\n";
	}
	
	if (! -e $file)
	{
		die "### ERROR: File $file does not exist. Cannot update file.\n";
	}
	
	if ($newVersion eq "")
	{
		die "### ERROR: Missing new version #. Cannot update version in file.\n";
	}

	#my	$VersionStr = "1.0.0." . $buildNum;
	#print "New Version: $VersionStr";
	# Read ReadMe file
	my	$data = "";

	open FILE, $file || die "Error opening ReadMe file $file\n";

	while (my $buf = <FILE>)
	{
		$buf =~ s/(.*)($prefix)(\d+\.\d+\.\d+\.\d+)(.*)/$1$2$newVersion$4/g;
		$data .= $buf;
	}

	close FILE;

	# Create temporary file
	my	$tmpFile = "${file}.tmp";
		
	# Write output file
	open FILE, ">$tmpFile" || die "Error opening output file $tmpFile\n";
	print FILE $data;
	close FILE;
	
	# Print output
	print "INFO:   New Version in File:\n";
	print "$data";		
	
	# Print differences
	#my	$result = diff($file, $tmpFile, {STYLE => "OldStyle"});
	
	#print "INFO:   File Differences:\n";
	#print $result;	
	
	# Replace file
	unlink($file) || die "### ERROR: Failed to delete existing file $file.\n";
	rename($tmpFile, $file) || die "### ERROR: Failed to rename file from $tmpFile to $file.\n";
	
	print "INFO:   Successfully wrote file $file.\n";	
}

sub updateSPOLAppNameInFile
{
	my	($file, $buildType) = @_;
	
	print "INFO: Updating SharePoint Online App Name in file $file.\n";
	print "  Build #      = $buildType\n";

	# Check for errors
	my	$argCount = scalar(@_);
	
	if ($argCount != 2)
	{
		die "### ERROR: Wrong # of arguments (expected 2, got $argCount) to updateSPOLAppNameInFile().\n";
	}
	
	if (! -e $file)
	{
		die "### ERROR: File $file does not exist. Cannot update file.\n";
	}
	
	if ($buildType eq "")
	{
		die "### ERROR: Missing buildType #. Cannot update App Name in file.\n";
	}

	my	$newAppName = "NLSPOLEventHandlerApp";
	if ($buildType eq "release")
	{
		$newAppName = "NLSPOLEventHandlerApp";
	}

	if ($buildType eq "dev")
	{
		$newAppName = "NLSPOLEventHandlerApp_Dev";
	}

	if ($buildType eq "pcv_smdc")
	{
		$newAppName = "NLSPOLEventHandlerApp_PCV";
	}

	print "New App Name: $newAppName\n";
	# Read ReadMe file
	my	$data = "";

	open FILE, $file || die "Error opening file $file\n";

	while (my $buf = <FILE>)
	{
#		print "LINE: $buf";

		$buf =~ s/NLSPOLEventHandlerApp/$newAppName/g;

		$data .= $buf;
	}

	close FILE;

	# Create temporary file
	my	$tmpFile = "${file}.tmp";
		
	# Write output file
	open FILE, ">$tmpFile" || die "Error opening output file $tmpFile\n";
	print FILE $data;
	close FILE;
	
	# Print differences
	#my	$result = diff($file, $tmpFile, {STYLE => "OldStyle"});
	
	#print "INFO:   File Differences:\n";
	#print $result;	
	
	# Replace file
	unlink($file) || die "### ERROR: Failed to delete existing file $file.\n";
	rename($tmpFile, $file) || die "### ERROR: Failed to rename file from $tmpFile to $file.\n";
	
	print "INFO:   Successfully wrote file $file.\n";	
}

sub updateSPOLAppShortNameInFile
{
	my	($file, $buildType) = @_;
	
	print "INFO: Updating SharePoint Online App Short Name in file $file.\n";
	print "  Build #      = $buildType\n";

	# Check for errors
	my	$argCount = scalar(@_);
	
	if ($argCount != 2)
	{
		die "### ERROR: Wrong # of arguments (expected 2, got $argCount) to updateSPOLAppShortNameInFile().\n";
	}
	
	if (! -e $file)
	{
		die "### ERROR: File $file does not exist. Cannot update file.\n";
	}
	
	if ($buildType eq "")
	{
		die "### ERROR: Missing buildType #. Cannot update App Short Name in file.\n";
	}

	my	$newAppName = "NextLabsEnforcerForSharePointOnline";
	my  $newAppTitle = "NextLabs Enforcer for SharePoint Online";
	if ($buildType eq "release")
	{
		$newAppName = "NextLabsEnforcerForSharePointOnline";
		$newAppTitle = "NextLabs Enforcer for SharePoint Online";
	}

	if ($buildType eq "dev")
	{
		$newAppName = "NextLabsEnforcerForSharePointOnline_Dev";
		$newAppTitle = "NextLabs Enforcer for SharePoint Online Dev";
	}

	if ($buildType eq "pcv_smdc")
	{
		$newAppName = "NextLabsEnforcerForSharePointOnline_PCV";
		$newAppTitle = "NextLabs Enforcer for SharePoint Online PCV";
	}

	print "New App Short Name: $newAppName\n";
	print "New App Short Title: $newAppTitle\n";
	# Read ReadMe file
	my	$data = "";

	open FILE, $file || die "Error opening file $file\n";

	while (my $buf = <FILE>)
	{
#		print "LINE: $buf";

		$buf =~ s/NextLabsEnforcerForSharePointOnline/$newAppName/g; # Modify the name in xml
		$buf =~ s/NextLabs Enforcer for SharePoint Online/$newAppTitle/g; # Modify the title in xml

		$data .= $buf;
	}

	close FILE;

	# Create temporary file
	my	$tmpFile = "${file}.tmp";
		
	# Write output file
	open FILE, ">$tmpFile" || die "Error opening output file $tmpFile\n";
	print FILE $data;
	close FILE;
	
	# Print differences
	#my	$result = diff($file, $tmpFile, {STYLE => "OldStyle"});
	
	#print "INFO:   File Differences:\n";
	#print $result;	
	
	# Replace file
	unlink($file) || die "### ERROR: Failed to delete existing file $file.\n";
	rename($tmpFile, $file) || die "### ERROR: Failed to rename file from $tmpFile to $file.\n";
	
	print "INFO:   Successfully wrote file $file.\n";	
}

sub updateSPOLAppIDInFile
{
	my	($file, $buildType) = @_;
	
	print "INFO: Updating SharePoint Online App ID in file $file.\n";
	print "  Build #      = $buildType\n";

	# Check for errors
	my	$argCount = scalar(@_);
	
	if ($argCount != 2)
	{
		die "### ERROR: Wrong # of arguments (expected 2, got $argCount) to updateSPOLAppIDInFile().\n";
	}
	
	if (! -e $file)
	{
		die "### ERROR: File $file does not exist. Cannot update file.\n";
	}
	
	if ($buildType eq "")
	{
		die "### ERROR: Missing buildType #. Cannot update App Name in file.\n";
	}

	my	$newAppName = "ffe5bbd4-dc9a-4c72-a050-72c412123089";
	if ($buildType eq "release")
	{
		$newAppName = "ffe5bbd4-dc9a-4c72-a050-72c412123089";
	}

	if ($buildType eq "dev")
	{
		$newAppName = "ffe5bbd4-dc9a-4c72-a050-72c412123088";
	}

	if ($buildType eq "pcv_smdc")
	{
		$newAppName = "ffe5bbd4-dc9a-4c72-a050-72c412123087";
	}

	print "New App ID: $newAppName\n";
	# Read ReadMe file
	my	$data = "";

	open FILE, $file || die "Error opening file $file\n";

	while (my $buf = <FILE>)
	{
#		print "LINE: $buf";

		$buf =~ s/ffe5bbd4-dc9a-4c72-a050-72c412123089/$newAppName/g;

		$data .= $buf;
	}

	close FILE;

	# Create temporary file
	my	$tmpFile = "${file}.tmp";
		
	# Write output file
	open FILE, ">$tmpFile" || die "Error opening output file $tmpFile\n";
	print FILE $data;
	close FILE;
	
	# Print differences
	#my	$result = diff($file, $tmpFile, {STYLE => "OldStyle"});
	
	#print "INFO:   File Differences:\n";
	#print $result;	
	
	# Replace file
	unlink($file) || die "### ERROR: Failed to delete existing file $file.\n";
	rename($tmpFile, $file) || die "### ERROR: Failed to rename file from $tmpFile to $file.\n";
	
	print "INFO:   Successfully wrote file $file.\n";	
}

#
# Construct names
#

print "INFO: Construct ISM and MSI names\n";

# Construct versions



#updateSPOLAppNameInFile($SetupRUL, $buildType);

#updateSPOLAppNameInFile($DCRMMakefile, $buildType);

#updateSPOLAppIDInFile($SolitionXML_2015, $buildType);
#updateSPOLAppIDInFile($SolitionXML_2016, $buildType);

#updateSPOLAppShortNameInFile($SolitionXML_2015, $buildType);
#updateSPOLAppShortNameInFile($SolitionXML_2016, $buildType);
