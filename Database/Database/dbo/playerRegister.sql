CREATE TABLE [dbo].[PlayerRegister]
(
	[Id]               INT            IDENTITY (1, 1) NOT NULL, 
    [Identifier] VARCHAR(100) NOT NULL, 
    [UniqueName] VARCHAR(100) NOT NULL, 
    [Names] VARCHAR(1000) NULL, 
    [CricInfoId] INT NOT NULL, 
    [IsActive] BIT NOT NULL DEFAULT 0
)
