
SELECT TOP 100 *
FROM (
	SELECT T1.UserId,
		T1.LastUpdatedDate,
		(
			CASE 
				WHEN T1.[key] IS NULL
					THEN T1.PropertyName
				ELSE T1.[key]
				END
			) ProertyName,
		(
			CASE 
				WHEN T1.[key] IS NULL
					THEN T1.PropertyValue
				ELSE T1.value
				END
			) ProertyValue
	FROM (
		SELECT *
		FROM aspnet_Profile P
		CROSS APPLY ReadUserProfileFromString(P.PropertyNames, P.PropertyValuesString, CAST(P.PropertyValuesBinary AS VARBINARY(MAX))) UP
		OUTER APPLY (
			SELECT *
			FROM OPENJSON(UP.PropertyValue) OJ1
			WHERE ISJSON(UP.PropertyValue) = 1
			) OJ
		) T1
	) T2
	;
