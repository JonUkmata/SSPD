IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251121184558_InitialCreate'
)
BEGIN
    CREATE TABLE [Recommendations] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [AnalysisId] uniqueidentifier NULL,
        [Type] nvarchar(max) NOT NULL,
        [Priority] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Recommendations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251121184558_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251121184558_InitialCreate', N'10.0.0');
END;

COMMIT;
GO

