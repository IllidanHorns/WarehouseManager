SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.ApplicationMetrics', N'U') IS NULL
BEGIN
    PRINT 'Creating table [dbo].[ApplicationMetrics]...';

    CREATE TABLE [dbo].[ApplicationMetrics]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MetricName] NVARCHAR(255) NOT NULL,
        [Value] FLOAT NOT NULL,
        [LastUpdated] DATETIME2 NOT NULL CONSTRAINT [DF_ApplicationMetrics_LastUpdated] DEFAULT (GETDATE()),
        [Description] NVARCHAR(1000) NULL
    );

    CREATE UNIQUE INDEX [IX_ApplicationMetrics_MetricName]
        ON [dbo].[ApplicationMetrics] ([MetricName]);

    PRINT 'Table [dbo].[ApplicationMetrics] has been created.';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[ApplicationMetrics] already exists. Skipping creation.';
END



