CREATE TABLE [dbo].[playerRegister]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Identifier] VARCHAR(100) NOT NULL, 
    [UniqueName] VARCHAR(100) NOT NULL, 
    [Names] VARCHAR(1000) NULL, 
    [CricInfoId] INT NOT NULL, 
    [IsActive] BIT NOT NULL DEFAULT 0
)
