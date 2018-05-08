USE aspnet_userprofile_viewer;
GO

ALTER DATABASE aspnet_userprofile_viewer

SET TRUSTWORTHY OFF;
GO

IF EXISTS (
		SELECT name
		FROM sysobjects
		WHERE name = 'ReadUserProfileFromString'
		)
	DROP FUNCTION ReadUserProfileFromString;
GO

IF EXISTS (
		SELECT name
		FROM sys.assemblies
		WHERE name = 'AspNetUserProfileParser'
		)
	DROP ASSEMBLY AspNetUserProfileParser;
GO


