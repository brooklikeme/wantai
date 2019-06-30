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
IF NOT EXISTS(SELECT * from syscolumns where id=object_id('[dbo].[ExperimentsInfo]') and name='MixTimes') BEGIN
ALTER TABLE [dbo].ExperimentsInfo ADD MixTimes smallint NULL
END
GO
IF NOT EXISTS(SELECT * from syscolumns where id=object_id('[dbo].[[PoolingRulesConfiguration]]') and name='GroupColor') BEGIN
ALTER TABLE [dbo].PoolingRulesConfiguration ADD GroupColor VARCHAR(255) NULL
END
GO
ALTER VIEW [dbo].[View_Tubes_PCRPlatePosition]
AS
SELECT DISTINCT 
                      dbo.Tubes.TubeID, dbo.Tubes.BarCode, dbo.Tubes.Position, dbo.Tubes.Grid, dbo.Tubes.TubeType, dbo.Tubes.ExperimentID, dbo.Tubes.Volume, 
                      dbo.PoolingRulesConfiguration.PoolingRulesName, dbo.PoolingRulesConfiguration.TubeNumber, dbo.RotationInfo.RotationID, 
                      dbo.Plates.BarCode AS PCRPlateBarcode, dbo.Plates.PlateName AS PCRPlateName, dbo.PCRPlatePosition.Position AS PCRPosition, 
                      dbo.PCRPlatePosition.TestName, dbo.Plates.PlateID AS PCRPlateID, dbo.DWPlatePosition.Position AS DWPosition, 
                      Plates_1.BarCode AS DWPlateBarCode, Plates_1.PlateName AS DWPlateName, dbo.Tubes.TubePosBarCode, dbo.TubeGroup.BatchType
FROM         dbo.TubeGroup INNER JOIN
                      dbo.Tubes INNER JOIN
                      dbo.Tube_PlatePosition ON dbo.Tubes.TubeID = dbo.Tube_PlatePosition.TubeID ON 
                      dbo.TubeGroup.TubeGroupID = dbo.Tubes.TubeGroupID INNER JOIN
                      dbo.PoolingRulesConfiguration ON dbo.TubeGroup.PoolingRulesID = dbo.PoolingRulesConfiguration.PoolingRulesID INNER JOIN
                      dbo.DWPlatePosition ON dbo.Tube_PlatePosition.DWPlatePositionID = dbo.DWPlatePosition.DWPlatePositionID INNER JOIN
                      dbo.PCRPlatePosition INNER JOIN
                      dbo.PCRPlatePosition_DWPlatePosition ON 
                      dbo.PCRPlatePosition.PCRPlatePositionID = dbo.PCRPlatePosition_DWPlatePosition.PCRPlatePositionID INNER JOIN
                      dbo.Plates ON dbo.PCRPlatePosition.PlateID = dbo.Plates.PlateID ON 
                      dbo.DWPlatePosition.DWPlatePositionID = dbo.PCRPlatePosition_DWPlatePosition.DWPlatePositionID INNER JOIN
                      dbo.RotationInfo ON dbo.RotationInfo.TubesBatchID = dbo.TubeGroup.TubesBatchID INNER JOIN
                      dbo.Plates AS Plates_1 ON dbo.DWPlatePosition.PlateID = Plates_1.PlateID
GO
UPDATE [dbo].[ReagentAndSuppliesConfiguration] SET [CalculationFormula] = N'(HBV>0?(HBV+11):0)*29.125*[3]' WHERE [ItemID] = N'2e02f3f4-97df-40cd-b2aa-16a36d3f4da7'
UPDATE [dbo].[PoolingRulesConfiguration] SET [PoolingRulesName] = N'µ¥¼ì' WHERE [PoolingRulesID] = N'f219f108-105c-454a-ada2-3da765f5e96b'
GO
DELETE from [dbo].[RoleInfo]
INSERT [dbo].[RoleInfo] ([RoleID], [RoleName], [RoleModules], [RoleLevel]) VALUES (N'ce42a5ca-a334-4edc-8d05-75ce49d3947b', N'¹¤³ÌÊ¦', N'<Authority><MenuAuthority><Menu menuName="TabMainMenu" access="1" /><Menu menuName="btnNewExperiment" access="1" /><Menu menuName="CloseExperiment_button" access="1" /><Menu menuName="CloseLamp_button" access="1" /><Menu menuName="QueryExperiment_Button" access="1" /><Menu menuName="TabSettings" access="1" /><Menu menuName="PoolingRule_Button" access="1" /><Menu menuName="TestingItem_Button" access="1" /><Menu menuName="LiquidConfig_Button" access="1" /><Menu menuName="ReagentSuppliesConfig_Button" access="1" /><Menu menuName="Operation_Button" access="1" /><Menu menuName="TabUserManagement" access="1" /><Menu menuName="CreateUser_Button" access="1" /><Menu menuName="EditUser_Button" access="1" /><Menu menuName="EditPassword_Button" access="1" /><Menu menuName="OpenEVOware_Button" access="1" /><Menu menuName="TecanMaintainDay_Button" access="1" /><Menu menuName="TecanMaintainWeek_Button" access="1" /><Menu menuName="TecanMaintainMonth_Button" access="1" /><Menu menuName="TabHelp" access="1" /></MenuAuthority><DataAuthority><authority dataName="ExperimentHistory" access="All" /><authority dataName="UserInfo" access="All" /><authority dataName="LogInfo" access="All" /></DataAuthority></Authority>', 3)
INSERT [dbo].[RoleInfo] ([RoleID], [RoleName], [RoleModules], [RoleLevel]) VALUES (N'fb639be0-50a7-43d7-99eb-bf49d81f799b', N'²Ù×÷Ô±', N'<Authority><MenuAuthority><Menu menuName="TabMainMenu" access="1" /><Menu menuName="btnNewExperiment" access="1" /><Menu menuName="CloseExperiment_button" access="1" /><Menu menuName="CloseLamp_button" access="1" /><Menu menuName="QueryExperiment_Button" access="1" /><Menu menuName="TabSettings" access="0" /><Menu menuName="PoolingRule_Button" access="1" /><Menu menuName="TestingItem_Button" access="1" /><Menu menuName="LiquidConfig_Button" access="1" /><Menu menuName="ReagentSuppliesConfig_Button" access="1" /><Menu menuName="Operation_Button" access="1" /><Menu menuName="TabUserManagement" access="1" /><Menu menuName="CreateUser_Button" access="0" /><Menu menuName="EditUser_Button" access="0" /><Menu menuName="EditPassword_Button" access="1" /><Menu menuName="OpenEVOware_Button" access="0" /><Menu menuName="TecanMaintainDay_Button" access="1" /><Menu menuName="TecanMaintainWeek_Button" access="1" /><Menu menuName="TecanMaintainMonth_Button" access="1" /><Menu menuName="TabHelp" access="1" /></MenuAuthority><DataAuthority><authority dataName="ExperimentHistory" access="Self" /><authority dataName="UserInfo" access="Self" /><authority dataName="LogInfo" access="Self" /></DataAuthority></Authority>', 1)
INSERT [dbo].[RoleInfo] ([RoleID], [RoleName], [RoleModules], [RoleLevel]) VALUES (N'844d89d3-4c4a-4db8-8d10-c4abe7f856b2', N'¹ÜÀíÔ±', N'<Authority><MenuAuthority><Menu menuName="TabMainMenu" access="1" /><Menu menuName="btnNewExperiment" access="1" /><Menu menuName="CloseExperiment_button" access="1" /><Menu menuName="CloseLamp_button" access="1" /><Menu menuName="QueryExperiment_Button" access="1" /><Menu menuName="TabSettings" access="0" /><Menu menuName="PoolingRule_Button" access="1" /><Menu menuName="TestingItem_Button" access="1" /><Menu menuName="LiquidConfig_Button" access="1" /><Menu menuName="ReagentSuppliesConfig_Button" access="1" /><Menu menuName="Operation_Button" access="1" /><Menu menuName="TabUserManagement" access="1" /><Menu menuName="CreateUser_Button" access="1" /><Menu menuName="EditUser_Button" access="1" /><Menu menuName="EditPassword_Button" access="1" /><Menu menuName="OpenEVOware_Button" access="0" /><Menu menuName="TecanMaintainDay_Button" access="1" /><Menu menuName="TecanMaintainWeek_Button" access="1" /><Menu menuName="TecanMaintainMonth_Button" access="1" /><Menu menuName="TabHelp" access="1" /></MenuAuthority><DataAuthority><authority dataName="ExperimentHistory" access="All" /><authority dataName="UserInfo" access="All" /><authority dataName="LogInfo" access="All" /></DataAuthority></Authority>', 2)
INSERT [dbo].[RoleInfo] ([RoleID], [RoleName], [RoleModules], [RoleLevel]) VALUES (N'ce42a5ca-a334-4edc-8d05-75ce49d39473', N'Ô¶³Ì²Ù×÷Ô±', N'<Authority><MenuAuthority><Menu menuName="TabMainMenu" access="1" /><Menu menuName="btnNewExperiment" access="0" /><Menu menuName="CloseExperiment_button" access="0" /><Menu menuName="CloseLamp_button" access="0" /><Menu menuName="QueryExperiment_Button" access="1" /><Menu menuName="TabSettings" access="1" /><Menu menuName="PoolingRule_Button" access="1" /><Menu menuName="TestingItem_Button" access="1" /><Menu menuName="LiquidConfig_Button" access="1" /><Menu menuName="ReagentSuppliesConfig_Button" access="1" /><Menu menuName="Operation_Button" access="1" /><Menu menuName="TabUserManagement" access="0" /><Menu menuName="CreateUser_Button" access="0" /><Menu menuName="EditUser_Button" access="0" /><Menu menuName="EditPassword_Button" access="1" /><Menu menuName="OpenEVOware_Button" access="0" /><Menu menuName="TecanMaintainDay_Button" access="0" /><Menu menuName="TecanMaintainWeek_Button" access="0" /><Menu menuName="TecanMaintainMonth_Button" access="0" /><Menu menuName="TabHelp" access="1" /></MenuAuthority><DataAuthority><authority dataName="ExperimentHistory" access="All" /><authority dataName="UserInfo" access="All" /><authority dataName="LogInfo" access="All" /></DataAuthority></Authority>', 0)
GO
DELETE from [dbo].[SystemFluidConfiguration]
INSERT INTO [dbo].[SystemFluidConfiguration] ([ItemID], [Position], [Grid], [ItemType], [Volume], [BatchType]) VALUES (N'a20a19e2-3695-11e1-a940-0019d147c478', 1, 1, 3, NULL, '1')
INSERT INTO [dbo].[SystemFluidConfiguration] ([ItemID], [Position], [Grid], [ItemType], [Volume], [BatchType]) VALUES (N'77d0a7f3-3697-11e1-a940-0019d147c478', 2, 1, 2, NULL, '1')
INSERT INTO [dbo].[SystemFluidConfiguration] ([ItemID], [Position], [Grid], [ItemType], [Volume], [BatchType]) VALUES (N'65ef3d88-ff2d-11e5-b655-001c4266b053', 3, 1, 4, NULL, '1')
INSERT INTO [dbo].[SystemFluidConfiguration] ([ItemID], [Position], [Grid], [ItemType], [Volume], [BatchType]) VALUES (N'65ef3d89-ff2d-11e5-b655-001c4266b053', 4, 1, 4, NULL, '1')
INSERT INTO [dbo].[SystemFluidConfiguration] ([ItemID], [Position], [Grid], [ItemType], [Volume], [BatchType]) VALUES (N'65ef3d8a-ff2d-11e5-b655-001c4266b053', 5, 1, 4, NULL, '1')
INSERT INTO [dbo].[SystemFluidConfiguration] ([ItemID], [Position], [Grid], [ItemType], [Volume], [BatchType]) VALUES (N'65ef3d8b-ff2d-11e5-b655-001c4266b053', 6, 1, 4, NULL, '1')
INSERT INTO [dbo].[SystemFluidConfiguration] ([ItemID], [Position], [Grid], [ItemType], [Volume], [BatchType]) VALUES (N'65ef3d8c-ff2d-11e5-b655-001c4266b053', 7, 1, 4, NULL, '1')
INSERT INTO [dbo].[SystemFluidConfiguration] ([ItemID], [Position], [Grid], [ItemType], [Volume], [BatchType]) VALUES (N'65ef3d8d-ff2d-11e5-b655-001c4266b053', 8, 1, 4, NULL, '1')
GO
IF NOT EXISTS(SELECT * from sysobjects where id=object_id('[dbo].[SystemConfiguration]')) BEGIN
/****** Object:  Table [dbo].[SystemConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[SystemConfiguration] (
    [ItemID] uniqueidentifier  NOT NULL,
    [ItemValue] nvarchar(255)  NULL,
    [ItemName] varchar(40)  NOT NULL,
    [ItemCode] nvarchar(40)  NOT NULL,
    [WorkDeskType] varchar(10)  NULL,
 CONSTRAINT [PK_SystemConfiguration] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
SET ANSI_PADDING OFF
INSERT [dbo].[SystemConfiguration] ([ItemID], [ItemName], [ItemCode], [ItemValue], [WorkDeskType]) VALUES (N'e96e9da8-f35b-11e6-9b01-001c4266b053', N'ÒÇÆ÷ÐÍºÅ', N'InstrumentType', N'1.0', '200')
INSERT [dbo].[SystemConfiguration] ([ItemID], [ItemName], [ItemCode], [ItemValue], [WorkDeskType]) VALUES (N'e96e9dab-f35b-11e6-9b01-001c4266b053', N'ÒÇÆ÷±àÂë', N'InstrumentNumber', N'001', '200')
INSERT [dbo].[SystemConfiguration] ([ItemID], [ItemName], [ItemCode], [ItemValue], [WorkDeskType]) VALUES (N'e96e9dbc-f35b-11e6-9b01-001c4266b053', N'ÒÇÆ÷ÐÍºÅ', N'InstrumentType', N'2.0', '150')
INSERT [dbo].[SystemConfiguration] ([ItemID], [ItemName], [ItemCode], [ItemValue], [WorkDeskType]) VALUES (N'e96e9dc0-f35b-11e6-9b01-001c4266b053', N'ÒÇÆ÷±àÂë', N'InstrumentNumber', N'001', '150')
INSERT [dbo].[SystemConfiguration] ([ItemID], [ItemName], [ItemCode], [ItemValue], [WorkDeskType]) VALUES (N'e96e9dc4-f35b-11e6-9b01-001c4266b053', N'ÒÇÆ÷ÐÍºÅ', N'InstrumentType', N'3.0', '100')
INSERT [dbo].[SystemConfiguration] ([ItemID], [ItemName], [ItemCode], [ItemValue], [WorkDeskType]) VALUES (N'e96e9dc8-f35b-11e6-9b01-001c4266b053', N'ÒÇÆ÷±àÂë', N'InstrumentNumber', N'001', '100')
END
GO
IF NOT EXISTS(SELECT * from sysobjects where id=object_id('[dbo].[ReportConfiguration]')) BEGIN
/****** Object:  Table [dbo].[ReportConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[ReportConfiguration] (
    [ItemID] uniqueidentifier  NOT NULL,
    [DisplayName] varchar(255)  NOT NULL,
    [CalculationFormula] varchar(255)  NULL,
    [Position] int  NOT NULL,
    [ActiveStatus] bit  NOT NULL,
    [WorkDeskType] varchar(10)  NULL,
CONSTRAINT [PK_ReportConfiguration] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
SET ANSI_PADDING OFF
END
GO