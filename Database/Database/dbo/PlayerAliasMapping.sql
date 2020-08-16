CREATE TABLE [dbo].[PlayerAliasMapping] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,    
    [CricInfoId]       INT            NOT NULL,
    [IsActive]         BIT            DEFAULT ((1)) NOT NULL,
    [CricsheetName]    NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_dbo.PlayerAliasMapping] PRIMARY KEY CLUSTERED ([Id] ASC)
);