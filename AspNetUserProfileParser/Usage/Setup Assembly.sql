ALTER DATABASE aspnet_userprofile_viewer SET TRUSTWORTHY ON;
GO

ALTER DATABASE aspnet_userprofile_viewer SET COMPATIBILITY_LEVEL = 130;
GO

USE aspnet_userprofile_viewer;
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

CREATE ASSEMBLY AspNetUserProfileParser
FROM 'C:\SqlserverAssembly\UserProfileBinaryPaser\AspNetUserProfileParser.dll' WITH PERMISSION_SET = EXTERNAL_ACCESS;
GO

CREATE FUNCTION ReadUserProfileFromString (
	@propertyNames NVARCHAR(MAX),
	@propertyValuesString NVARCHAR(MAX),
	@propertyValuesBytes VARBINARY(MAX)
	)
RETURNS TABLE (
	PropertyName NVARCHAR(MAX),
	PropertyValue NVARCHAR(MAX)
	)
AS
EXTERNAL NAME AspNetUserProfileParser.UserDefinedFunctions.[ReadUserProfileFromString];
GO