﻿IF NOT EXISTS (SELECT column_name 
			   FROM INFORMATION_SCHEMA.columns 
			   WHERE table_name = 'Preek' 
			   AND column_name = 'SourceFileName')
BEGIN
	-- Add source file name column.
	ALTER TABLE dbo.Preek ADD
		SourceFileName varchar(MAX) NULL
END