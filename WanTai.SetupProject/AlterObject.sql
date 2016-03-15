SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF EXISTS(SELECT * from syscolumns where id=object_id('[dbo].[RotationInfo]') and name='BatchType') BEGIN
ALTER TABLE [dbo].[RotationInfo] DROP COLUMN BatchType
END
GO
IF NOT EXISTS(SELECT * from syscolumns where id=object_id('[dbo].[TubeGroup]') and name='BatchType') BEGIN
ALTER TABLE [dbo].[TubeGroup] ADD BatchType VARCHAR(10) NULL
END
GO