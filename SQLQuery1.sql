USE [MyStore];
GO

IF COL_LENGTH('dbo.Product', 'ProductStock') IS NULL
    ALTER TABLE dbo.Product ADD ProductStock int NOT NULL CONSTRAINT DF_Product_Stock DEFAULT (50);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Product_Stock')
    ALTER TABLE dbo.Product ADD CONSTRAINT CK_Product_Stock CHECK (ProductStock >= 0);
GO

IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_Product_Stock')
BEGIN
    ALTER TABLE dbo.Product DROP CONSTRAINT DF_Product_Stock;
    ALTER TABLE dbo.Product ADD CONSTRAINT DF_Product_Stock DEFAULT (0) FOR ProductStock;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_Category_CategoryName')
BEGIN
    IF EXISTS (SELECT CategoryName FROM dbo.Category GROUP BY CategoryName HAVING COUNT(*) > 1)
        PRINT N'Skipped UQ_Category_CategoryName: duplicate category names exist. Rename them, then run this script again.';
    ELSE
        ALTER TABLE dbo.Category ADD CONSTRAINT UQ_Category_CategoryName UNIQUE (CategoryName);
END
GO