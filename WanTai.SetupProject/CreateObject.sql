/****** Object:  Table [dbo].[TestingItemConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TestingItemConfiguration](
	[TestingItemID] [uniqueidentifier] NOT NULL,
	[TestingItemName] [varchar](255) NOT NULL,
	[TestingItemColor] [varchar](255) NULL,
	[DisplaySequence] [smallint] NULL,
	[WorkListFileName] [varchar](255) NULL,
	[ActiveStatus] [bit] NOT NULL,
 CONSTRAINT [PK_TestingItemConfiguration] PRIMARY KEY CLUSTERED 
(
	[TestingItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SystemFluidConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SystemFluidConfiguration](
	[ItemID] [uniqueidentifier] NOT NULL,
	[Position] [int] NULL,
	[Grid] [int] NULL,
	[ItemType] [smallint] NULL,
	[Volume] [float] NULL,
	[BatchType] [varchar] (10) NULL,
 CONSTRAINT [PK_SystemFluidConfiguration] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SampleTracking]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SampleTracking](
	[ItemID] [uniqueidentifier] NOT NULL,
	[RotationID] [uniqueidentifier] NULL,
	[ExperimentsID] [uniqueidentifier] NULL,
	[FileName] [varchar](255) NULL,
	[OperationSequence] [int] NULL,
	[OperationID] [uniqueidentifier] NULL,
	[RackID] [varchar](255) NULL,
	[CavityID] [varchar](255) NULL,
	[Position] [varchar](255) NULL,
	[SampleID] [varchar](255) NULL,
	[CONCENTRATION] [float] NULL,
	[VOLUME] [float] NULL,
	[SampleErrors] [varchar](255) NULL,
	[IsKeeping] [bit] NULL,
	[CreateTime] [datetime] NULL,
 CONSTRAINT [PK_SampleTracking] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ReagentAndSuppliesConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ReagentAndSuppliesConfiguration](
	[ItemID] [uniqueidentifier] NOT NULL,
	[EnglishName] [varchar](255) NOT NULL,
	[DisplayName] [varchar](255) NOT NULL,
	[Position] [int] NOT NULL,
	[Grid] [int] NOT NULL,
	[Unit] [varchar](255) NULL,
	[ItemType] [smallint] NULL,
	[CalculationFormula] [varchar](255) NULL,
	[WorkDeskType] [varchar] (10) NULL,
	[ContainerName] [varchar](255) NULL
) ON [PRIMARY]
SET ANSI_PADDING OFF
ALTER TABLE [dbo].[ReagentAndSuppliesConfiguration] ADD [BarcodePrefix] [varchar](255) NULL
ALTER TABLE [dbo].[ReagentAndSuppliesConfiguration] ADD [Color] [varchar](255) NULL
ALTER TABLE [dbo].[ReagentAndSuppliesConfiguration] ADD [ActiveStatus] [bit] NOT NULL
ALTER TABLE [dbo].[ReagentAndSuppliesConfiguration] ADD [SimpleTrackingVolumn] [float] NULL
ALTER TABLE [dbo].[ReagentAndSuppliesConfiguration] ADD  CONSTRAINT [PK_ReagentAndSuppliesConfiguration] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PoolingRulesConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PoolingRulesConfiguration](
	[PoolingRulesID] [uniqueidentifier] NOT NULL,
	[PoolingRulesName] [varchar](255) NOT NULL,
	[TubeNumber] [int] NOT NULL,
	[ActiveStatus] [bit] NOT NULL,
 CONSTRAINT [PK_PoolingRulesConfiguration] PRIMARY KEY CLUSTERED 
(
	[PoolingRulesID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Plates]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Plates](
	[PlateID] [uniqueidentifier] NOT NULL,
	[BarCode] [varchar](255) NULL,
	[PlateName] [varchar](255) NULL,
	[RotationID] [uniqueidentifier] NULL,
	[PlateType] [smallint] NULL,
	[ExperimentID] [uniqueidentifier] NULL,
	[TubesBatchID] [uniqueidentifier] NULL,
	[PCRContent] [xml] NULL,
 CONSTRAINT [PK_Plates] PRIMARY KEY CLUSTERED 
(
	[PlateID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PCRTestResult]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PCRTestResult](
	[ItemID] [uniqueidentifier] NOT NULL,
	[RotationID] [uniqueidentifier] NOT NULL,
	[Position] [int] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[Result] [varchar](2000) NULL,
	[ExperimentID] [uniqueidentifier] NULL,
	[BarCode] [varchar](255) NULL,
	[PCRContent] [xml] NULL,
	[PlateID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PCRTestResult] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[OperationConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[OperationConfiguration](
	[OperationID] [uniqueidentifier] NOT NULL,
	[OperationName] [varchar](255) NOT NULL,
	[OperationType] [smallint] NOT NULL,
	[OperationSequence] [smallint] NOT NULL,
	[SubOperationIDs] [varchar](225) NULL,
	[DisplayFlag] [bit] NULL,
	[ScriptFileName] [varchar](255) NULL,
	[RunTime] [int] NULL,
	[StartOperationFileName] [varchar](255) NULL,
	[EndOperationFileName] [varchar](255) NULL,
	[ActiveStatus] [bit] NOT NULL,
 CONSTRAINT [PK_OperationConfiguration] PRIMARY KEY CLUSTERED 
(
	[OperationID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LogInfo]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LogInfo](
	[LogID] [uniqueidentifier] NOT NULL,
	[LogContent] [varchar](2000) NULL,
	[LogLevel] [smallint] NULL,
	[CreaterTime] [datetime] NULL,
	[LoginName] [varchar](255) NULL,
	[Module] [varchar](255) NULL,
	[ExperimentID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_LogInfo] PRIMARY KEY CLUSTERED 
(
	[LogID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExperimentsInfo]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExperimentsInfo](
	[ExperimentID] [uniqueidentifier] NOT NULL,
	[ExperimentName] [varchar](255) NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NULL,
	[LoginName] [varchar](255) NOT NULL,
	[Remark] [varchar](2000) NULL,
	[State] [smallint] NOT NULL,
 CONSTRAINT [PK_ExperimentsInfo] PRIMARY KEY CLUSTERED 
(
	[ExperimentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  StoredProcedure [dbo].[ChangeCharacterToPositionNumber1]    Script Date: 02/10/2012 15:24:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[ChangeCharacterToPositionNumber1] 
(
	-- Add the parameters for the function here
	@CharacterPosition varchar(255)
)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @OrgCharacterPosition varchar(255)
	DECLARE @oneLineCellCount int
	DECLARE @lineNumber int
	DECLARE @beginCharacter varchar(255)
	DECLARE @number int	
	declare @positionNUmber int	

	set @OrgCharacterPosition = @CharacterPosition   
	set @oneLineCellCount = 8
    set @number = 0
    set @beginCharacter = substring(@OrgCharacterPosition, 1, 1)    
    set @lineNumber = cast(substring(@OrgCharacterPosition, 2, len(@OrgCharacterPosition)) AS int)
    
    IF len(@beginCharacter)=1
		Begin		
			IF @beginCharacter = 'A'                
				set @number = 1
			ELSE IF @beginCharacter = 'B'
				set @number = 2
			ELSE IF @beginCharacter = 'C'
				set @number = 3
			ELSE IF @beginCharacter='D'
				set @number = 4
			ELSE IF @beginCharacter='E'
				set @number = 5
			ELSE IF @beginCharacter='F'
				set @number = 6
			ELSE IF @beginCharacter='G'
				set @number = 7
			ELSE IF @beginCharacter='H'
				set @number = 8
		END 

    set @positionNUmber = (@lineNumber-1)*@oneLineCellCount + @number
    print @positionNUmber
    
    
END
GO
/****** Object:  UserDefinedFunction [dbo].[ChangeCharacterToPositionNumber]    Script Date: 02/10/2012 15:24:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ChangeCharacterToPositionNumber]
(
	-- Add the parameters for the function here
	@CharacterPosition varchar(255)
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @OrgCharacterPosition varchar(255)
	DECLARE @oneLineCellCount int
	DECLARE @lineNumber int
	DECLARE @beginCharacter varchar(255)
	DECLARE @number int	

	set @OrgCharacterPosition = @CharacterPosition   
	set @oneLineCellCount = 8
    set @number = 0
    set @beginCharacter = substring(@OrgCharacterPosition, 1, 1)    
    set @lineNumber = cast(substring(@OrgCharacterPosition, 2, len(@OrgCharacterPosition)) AS int)
    
    IF len(@beginCharacter)=1
		Begin		
			IF @beginCharacter = 'A'                
				set @number = 1
			ELSE IF @beginCharacter = 'B'
				set @number = 2
			ELSE IF @beginCharacter = 'C'
				set @number = 3
			ELSE IF @beginCharacter='D'
				set @number = 4
			ELSE IF @beginCharacter='E'
				set @number = 5
			ELSE IF @beginCharacter='F'
				set @number = 6
			ELSE IF @beginCharacter='G'
				set @number = 7
			ELSE IF @beginCharacter='H'
				set @number = 8
		END 

    return (@lineNumber-1)*@oneLineCellCount + @number
    
    
END
GO
/****** Object:  Table [dbo].[Carrier]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING OFF
GO
CREATE TABLE [dbo].[Carrier](
	[CarrierID] [uniqueidentifier] NOT NULL,
	[CarrierName] [varchar](255) NOT NULL,
	[Position] [float] NOT NULL,
	[Grid] [float] NOT NULL,
	[Width] [float] NOT NULL,
	[Heigh] [float] NOT NULL,
	[Color] [varchar](255) NOT NULL,
	[Type] [smallint] NOT NULL,
	[WorkDeskType] [varchar] (10) NULL,
PRIMARY KEY CLUSTERED 
(
	[CarrierID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[RoleInfo]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RoleInfo](
	[RoleID] [uniqueidentifier] NOT NULL,
	[RoleName] [varchar](50) NOT NULL,
	[RoleModules] [xml] NOT NULL,
	[RoleLevel] [smallint] NOT NULL,
 CONSTRAINT [PK_RoleInfo] PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[UserInfo]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[UserInfo](
	[UserID] [uniqueidentifier] NOT NULL,
	[LoginName] [varchar](255) NOT NULL,
	[LoginPassWord] [varchar](255) NOT NULL,
	[RoleName] [varchar](50) NULL,
	[CreateTime] [datetime] NOT NULL,
	[CreateName] [varchar](255) NULL,
	[UpdateTime] [datetime] NULL,
 CONSTRAINT [PK_UserInfo] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TubesBatch]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TubesBatch](
	[TubesBatchID] [uniqueidentifier] NOT NULL,
	[TubesBatchName] [varchar](255) NOT NULL,
	[ExperimentID] [uniqueidentifier] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_TubesBatch] PRIMARY KEY CLUSTERED 
(
	[TubesBatchID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DWPlatePosition]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DWPlatePosition](
	[DWPlatePositionID] [uniqueidentifier] NOT NULL,
	[Position] [int] NOT NULL,
	[TubeGroupID] [uniqueidentifier] NULL,
	[PlateID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_DWPlatePosition] PRIMARY KEY CLUSTERED 
(
	[DWPlatePositionID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[DeleteUserInfo]    Script Date: 02/10/2012 15:24:10 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE procedure [dbo].[DeleteUserInfo]  
@userID uniqueIdentifier  
as  
begin  
--declare @userName varchar(255)  
--select @userName=loginname from UserInfo where UserID=@userID  
  
--select ExperimentID   
--INTO #experimentID  
--from ExperimentsInfo WHERE LoginName=@userName  
  
SET NOCOUNT ON  
  
DELETE FROM UserInfo WHERE UserID=@userID  
--UPDATE UserInfo Set CreateName='' WHERE CreateName=@userName  
  
--DELETE FROM PCRTestResult WHERE ExperimentID in (select experimentID from #experimentID)  
     
--DELETE FROM ReagentsAndSuppliesConsumption WHERE ExperimentID in (select experimentID from #experimentID)  
--DELETE FROM ReagentAndSupplies WHERE ExperimentID in (select experimentID from #experimentID)  
     
--DELETE FROM PCRPlatePosition_DWPlatePosition WHERE PCRPlatePositionID IN (SELECT PCRPlatePositionID FROM PCRPlatePosition WHERE PlateID IN (SELECT PlateID FROM Plates WHERE ExperimentID in (select experimentID from #experimentID)))     
--DELETE FROM PCRPlatePosition WHERE PlateID IN (SELECT PlateID FROM Plates WHERE ExperimentID in (select experimentID from #experimentID) )  
--DELETE FROM Tube_PlatePosition WHERE DWPlatePositionID IN (SELECT DWPlatePositionID FROM DWPlatePosition WHERE PlateID IN (SELECT PlateID FROM Plates WHERE ExperimentID in (select experimentID from #experimentID)))    
--DELETE FROM DWPlatePosition WHERE PlateID IN (SELECT PlateID FROM Plates WHERE ExperimentID in (select experimentID from #experimentID))  
--DELETE FROM Plates WHERE ExperimentID in (select experimentID from #experimentID)  
     
--DELETE FROM RotationOperate WHERE ExperimentID in (select experimentID from #experimentID)  
--DELETE FROM RotationInfo WHERE ExperimentID in (select experimentID from #experimentID)  
     
--DELETE FROM Tubes WHERE ExperimentID in (select experimentID from #experimentID)  
--DELETE FROM TestItem_TubeGroup WHERE TubeGroupID IN (SELECT TubeGroupID FROM TubeGroup WHERE ExperimentID in (select experimentID from #experimentID))    
--DELETE FROM TubeGroup WHERE ExperimentID in (select experimentID from #experimentID)  
--DELETE FROM TubesBatch WHERE ExperimentID in (select experimentID from #experimentID)  
     
--DELETE FROM ExperimentsInfo WHERE LoginName=@userName  
  
--drop table #experimentID  
end
GO
/****** Object:  Table [dbo].[ReagentAndSupplies]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ReagentAndSupplies](
	[ItemID] [uniqueidentifier] NOT NULL,
	[BarCode] [varchar](255) NULL,
	[ItemType] [smallint] NULL,
	[ExperimentID] [uniqueidentifier] NULL,
	[ConfigurationItemID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_ReagentAndSupplies] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PCRPlatePosition]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PCRPlatePosition](
	[PCRPlatePositionID] [uniqueidentifier] NOT NULL,
	[Position] [int] NULL,
	[PlateID] [uniqueidentifier] NULL,
	[TestName] [varchar](255) NULL,
 CONSTRAINT [PK_PlatePosition] PRIMARY KEY CLUSTERED 
(
	[PCRPlatePositionID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[RotationOperate]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RotationOperate](
	[RotationOperateID] [uniqueidentifier] NOT NULL,
	[OperationID] [uniqueidentifier] NOT NULL,
	[RotationID] [uniqueidentifier] NOT NULL,
	[State] [smallint] NOT NULL,
	[ExperimentID] [uniqueidentifier] NOT NULL,
	[ErrorLog] [varchar](2000) NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
 CONSTRAINT [PK_RotationOperate] PRIMARY KEY CLUSTERED 
(
	[RotationOperateID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[RotationInfo]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RotationInfo](
	[RotationID] [uniqueidentifier] NOT NULL,
	[ExperimentID] [uniqueidentifier] NOT NULL,
	[TubesBatchID] [uniqueidentifier] NULL,
	[RotationName] [varchar](255) NULL,
	[State] [smallint] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[OperationID] [uniqueidentifier] NOT NULL,
	[OperationName] [varchar](255) NOT NULL,
	[RotationSequence] [smallint] NULL,
 CONSTRAINT [PK_RotationInfo] PRIMARY KEY CLUSTERED 
(
	[RotationID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PCRPlatePosition_DWPlatePosition]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PCRPlatePosition_DWPlatePosition](
	[PCRPlatePositionID] [uniqueidentifier] NOT NULL,
	[DWPlatePositionID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PlatePositionInDWPlatePosition] PRIMARY KEY CLUSTERED 
(
	[PCRPlatePositionID] ASC,
	[DWPlatePositionID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TubeGroup]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TubeGroup](
	[TubeGroupID] [uniqueidentifier] NOT NULL,
	[PoolingRulesID] [uniqueidentifier] NOT NULL,
	[ExperimentID] [uniqueidentifier] NOT NULL,
	[TubesBatchID] [uniqueidentifier] NULL,
	[CreateTime] [datetime] NOT NULL,
	[BatchType] [varchar] (10) NULL,
 CONSTRAINT [PK_TubeGroup] PRIMARY KEY CLUSTERED 
(
	[TubeGroupID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReagentsAndSuppliesConsumption]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReagentsAndSuppliesConsumption](
	[ItemID] [uniqueidentifier] NOT NULL,
	[Volume] [float] NULL,
	[UpdateTime] [datetime] NULL,
	[ExperimentID] [uniqueidentifier] NOT NULL,
	[VolumeType] [smallint] NULL,
	[ReagentAndSupplieID] [uniqueidentifier] NULL,
	[RotationID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_ReagentsAndSuppliesConsumption] PRIMARY KEY CLUSTERED 
(
	[ItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TestItem_TubeGroup]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestItem_TubeGroup](
	[TubeGroupID] [uniqueidentifier] NOT NULL,
	[TestingItemID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_TestItemInTubeGroup] PRIMARY KEY CLUSTERED 
(
	[TubeGroupID] ASC,
	[TestingItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tubes]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Tubes](
	[TubeID] [uniqueidentifier] NOT NULL,
	[TubeGroupID] [uniqueidentifier] NOT NULL,
	[BarCode] [varchar](255) NOT NULL,
	[TubePosBarCode] [varchar](255) NULL,
	[Position] [int] NOT NULL,
	[Grid] [int] NOT NULL,
	[TubeType] [smallint] NOT NULL,
	[ExperimentID] [uniqueidentifier] NOT NULL,
	[Volume] [float] NULL,
 CONSTRAINT [PK_Tubes] PRIMARY KEY CLUSTERED 
(
	[TubeID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Tube_PlatePosition]    Script Date: 02/10/2012 15:24:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tube_PlatePosition](
	[TubeID] [uniqueidentifier] NOT NULL,
	[DWPlatePositionID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_TubeInPlatePosition] PRIMARY KEY CLUSTERED 
(
	[TubeID] ASC,
	[DWPlatePositionID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[View_Tubes_PCRPlatePosition]    Script Date: 02/10/2012 15:24:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_Tubes_PCRPlatePosition]
AS
SELECT DISTINCT 
                      dbo.Tubes.TubeID, dbo.Tubes.BarCode, dbo.Tubes.Position, dbo.Tubes.Grid, dbo.Tubes.TubeType, dbo.Tubes.ExperimentID, dbo.Tubes.Volume, 
                      dbo.PoolingRulesConfiguration.PoolingRulesName, dbo.PoolingRulesConfiguration.TubeNumber, dbo.RotationInfo.RotationID, 
                      dbo.Plates.BarCode AS PCRPlateBarcode, dbo.Plates.PlateName AS PCRPlateName, dbo.PCRPlatePosition.Position AS PCRPosition, 
                      dbo.PCRPlatePosition.TestName, dbo.Plates.PlateID AS PCRPlateID, dbo.DWPlatePosition.Position AS DWPosition, 
                      Plates_1.BarCode AS DWPlateBarCode, Plates_1.PlateName AS DWPlateName, dbo.Tubes.TubePosBarCode
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
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[50] 4[25] 2[21] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = -61
         Left = -39
      End
      Begin Tables = 
         Begin Table = "Tubes"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 198
               Right = 203
            End
            DisplayFlags = 280
            TopColumn = 1
         End
         Begin Table = "Tube_PlatePosition"
            Begin Extent = 
               Top = 7
               Left = 260
               Bottom = 98
               Right = 432
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "PCRPlatePosition_DWPlatePosition"
            Begin Extent = 
               Top = 23
               Left = 466
               Bottom = 114
               Right = 641
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "PCRPlatePosition"
            Begin Extent = 
               Top = 6
               Left = 664
               Bottom = 127
               Right = 839
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Plates"
            Begin Extent = 
               Top = 235
               Left = 648
               Bottom = 356
               Right = 802
            End
            DisplayFlags = 280
            TopColumn = 1
         End
         Begin Table = "TubeGroup"
            Begin Extent = 
               Top = 169
               Left = 230
               Bottom = 322
               Right = 395
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "PoolingRulesConfiguration"
            Begin Extent = 
               Top = 322
               Left = 32
               Bottom = 428
             ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Tubes_PCRPlatePosition'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'  Right = 202
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "DWPlatePosition"
            Begin Extent = 
               Top = 258
               Left = 442
               Bottom = 379
               Right = 614
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "RotationInfo"
            Begin Extent = 
               Top = 397
               Left = 237
               Bottom = 551
               Right = 408
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Plates_1"
            Begin Extent = 
               Top = 412
               Left = 480
               Bottom = 533
               Right = 634
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 2370
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Tubes_PCRPlatePosition'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Tubes_PCRPlatePosition'
GO
/****** Object:  StoredProcedure [dbo].[Delete_Experiment]    Script Date: 02/10/2012 15:24:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ellen
-- Create date: 2011-10-19
-- Description:	Delete Experiment record
-- =============================================
CREATE PROCEDURE [dbo].[Delete_Experiment] 
	-- Add the parameters for the stored procedure here
	@experimentId uniqueIdentifier
AS
BEGIN
	
	SET NOCOUNT ON;
	DELETE FROM PCRTestResult WHERE ExperimentID = @experimentId
	
	DELETE FROM ReagentsAndSuppliesConsumption WHERE ExperimentID = @experimentId
	DELETE FROM ReagentAndSupplies WHERE ExperimentID = @experimentId
	
	DELETE FROM PCRPlatePosition_DWPlatePosition WHERE PCRPlatePositionID IN (SELECT PCRPlatePositionID FROM PCRPlatePosition WHERE PlateID IN (SELECT PlateID FROM Plates WHERE ExperimentID = @experimentId)) 
	DELETE FROM PCRPlatePosition WHERE PlateID IN (SELECT PlateID FROM Plates WHERE ExperimentID = @experimentId)
	DELETE FROM Tube_PlatePosition WHERE DWPlatePositionID IN (SELECT DWPlatePositionID FROM DWPlatePosition WHERE PlateID IN (SELECT PlateID FROM Plates WHERE ExperimentID=@experimentId))
	DELETE FROM DWPlatePosition WHERE PlateID IN (SELECT PlateID FROM Plates WHERE ExperimentID=@experimentId)
	DELETE FROM Plates WHERE ExperimentID=@experimentId
	
	DELETE FROM RotationOperate WHERE ExperimentID=@experimentId
	DELETE FROM RotationInfo WHERE ExperimentID=@experimentId
	
	DELETE FROM Tubes WHERE ExperimentID=@experimentId
	DELETE FROM TestItem_TubeGroup WHERE TubeGroupID IN (SELECT TubeGroupID FROM TubeGroup WHERE ExperimentID=@experimentId)
	DELETE FROM TubeGroup WHERE ExperimentID=@experimentId
	DELETE FROM TubesBatch WHERE ExperimentID=@experimentId	
	
    DELETE FROM ExperimentsInfo WHERE ExperimentID = @experimentId
    
END
GO
/****** Object:  Default [DF_ExperimentsInfo_StartTime]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[ExperimentsInfo] ADD  CONSTRAINT [DF_ExperimentsInfo_StartTime]  DEFAULT (getdate()) FOR [StartTime]
GO
/****** Object:  Default [DF_LogInfo_CreaterTime]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[LogInfo] ADD  CONSTRAINT [DF_LogInfo_CreaterTime]  DEFAULT (getdate()) FOR [CreaterTime]
GO
/****** Object:  Default [DF_OperationConfiguration_RunTime]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[OperationConfiguration] ADD  CONSTRAINT [DF_OperationConfiguration_RunTime]  DEFAULT ((0)) FOR [RunTime]
GO
/****** Object:  Default [DF_OperationConfiguration_ActiveStatus]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[OperationConfiguration] ADD  CONSTRAINT [DF_OperationConfiguration_ActiveStatus]  DEFAULT ((1)) FOR [ActiveStatus]
GO
/****** Object:  Default [DF_PCRTestResult_CreateTime]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[PCRTestResult] ADD  CONSTRAINT [DF_PCRTestResult_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_PoolingRulesConfiguration_ActiveStatus]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[PoolingRulesConfiguration] ADD  CONSTRAINT [DF_PoolingRulesConfiguration_ActiveStatus]  DEFAULT ((1)) FOR [ActiveStatus]
GO
/****** Object:  Default [DF_ReagentAndSuppliesConfiguration_ActiveStatus]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[ReagentAndSuppliesConfiguration] ADD  CONSTRAINT [DF_ReagentAndSuppliesConfiguration_ActiveStatus]  DEFAULT ((1)) FOR [ActiveStatus]
GO
/****** Object:  Default [DF_RotationInfo_CreateTime]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[RotationInfo] ADD  CONSTRAINT [DF_RotationInfo_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_RotationOperate_StartTime]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[RotationOperate] ADD  CONSTRAINT [DF_RotationOperate_StartTime]  DEFAULT (getdate()) FOR [StartTime]
GO
/****** Object:  Default [DF_TestingItemConfiguration_ActiveStatus]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[TestingItemConfiguration] ADD  CONSTRAINT [DF_TestingItemConfiguration_ActiveStatus]  DEFAULT ((1)) FOR [ActiveStatus]
GO
/****** Object:  Default [DF_TubeGroup_CreateTime]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[TubeGroup] ADD  CONSTRAINT [DF_TubeGroup_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_TubesBatch_CreateTime]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[TubesBatch] ADD  CONSTRAINT [DF_TubesBatch_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_Table_1_CreateName]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[UserInfo] ADD  CONSTRAINT [DF_Table_1_CreateName]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  ForeignKey [FK_DWPlatePosition_Plates]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[DWPlatePosition]  WITH CHECK ADD  CONSTRAINT [FK_DWPlatePosition_Plates] FOREIGN KEY([PlateID])
REFERENCES [dbo].[Plates] ([PlateID])
GO
ALTER TABLE [dbo].[DWPlatePosition] CHECK CONSTRAINT [FK_DWPlatePosition_Plates]
GO
/****** Object:  ForeignKey [FK_PCRPlatePosition_Plates]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[PCRPlatePosition]  WITH CHECK ADD  CONSTRAINT [FK_PCRPlatePosition_Plates] FOREIGN KEY([PlateID])
REFERENCES [dbo].[Plates] ([PlateID])
GO
ALTER TABLE [dbo].[PCRPlatePosition] CHECK CONSTRAINT [FK_PCRPlatePosition_Plates]
GO
/****** Object:  ForeignKey [FK_PCRPlatePosition_DWPlatePosition_DWPlatePosition]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[PCRPlatePosition_DWPlatePosition]  WITH CHECK ADD  CONSTRAINT [FK_PCRPlatePosition_DWPlatePosition_DWPlatePosition] FOREIGN KEY([DWPlatePositionID])
REFERENCES [dbo].[DWPlatePosition] ([DWPlatePositionID])
GO
ALTER TABLE [dbo].[PCRPlatePosition_DWPlatePosition] CHECK CONSTRAINT [FK_PCRPlatePosition_DWPlatePosition_DWPlatePosition]
GO
/****** Object:  ForeignKey [FK_PCRPlatePosition_DWPlatePosition_PCRPlatePosition]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[PCRPlatePosition_DWPlatePosition]  WITH CHECK ADD  CONSTRAINT [FK_PCRPlatePosition_DWPlatePosition_PCRPlatePosition] FOREIGN KEY([PCRPlatePositionID])
REFERENCES [dbo].[PCRPlatePosition] ([PCRPlatePositionID])
GO
ALTER TABLE [dbo].[PCRPlatePosition_DWPlatePosition] CHECK CONSTRAINT [FK_PCRPlatePosition_DWPlatePosition_PCRPlatePosition]
GO
/****** Object:  ForeignKey [FK_ReagentAndSupplies_ReagentAndSuppliesConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[ReagentAndSupplies]  WITH CHECK ADD  CONSTRAINT [FK_ReagentAndSupplies_ReagentAndSuppliesConfiguration] FOREIGN KEY([ConfigurationItemID])
REFERENCES [dbo].[ReagentAndSuppliesConfiguration] ([ItemID])
GO
ALTER TABLE [dbo].[ReagentAndSupplies] CHECK CONSTRAINT [FK_ReagentAndSupplies_ReagentAndSuppliesConfiguration]
GO
/****** Object:  ForeignKey [FK_ReagentsAndSuppliesConsumption_ReagentAndSupplies]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[ReagentsAndSuppliesConsumption]  WITH CHECK ADD  CONSTRAINT [FK_ReagentsAndSuppliesConsumption_ReagentAndSupplies] FOREIGN KEY([ReagentAndSupplieID])
REFERENCES [dbo].[ReagentAndSupplies] ([ItemID])
GO
ALTER TABLE [dbo].[ReagentsAndSuppliesConsumption] CHECK CONSTRAINT [FK_ReagentsAndSuppliesConsumption_ReagentAndSupplies]
GO
/****** Object:  ForeignKey [FK_RotationInfo_TubesBatch]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[RotationInfo]  WITH CHECK ADD  CONSTRAINT [FK_RotationInfo_TubesBatch] FOREIGN KEY([TubesBatchID])
REFERENCES [dbo].[TubesBatch] ([TubesBatchID])
GO
ALTER TABLE [dbo].[RotationInfo] CHECK CONSTRAINT [FK_RotationInfo_TubesBatch]
GO
/****** Object:  ForeignKey [FK_RotationOperate_OperationConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[RotationOperate]  WITH CHECK ADD  CONSTRAINT [FK_RotationOperate_OperationConfiguration] FOREIGN KEY([OperationID])
REFERENCES [dbo].[OperationConfiguration] ([OperationID])
GO
ALTER TABLE [dbo].[RotationOperate] CHECK CONSTRAINT [FK_RotationOperate_OperationConfiguration]
GO
/****** Object:  ForeignKey [FK_TestItem_TubeGroup_TestingItemConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[TestItem_TubeGroup]  WITH CHECK ADD  CONSTRAINT [FK_TestItem_TubeGroup_TestingItemConfiguration] FOREIGN KEY([TestingItemID])
REFERENCES [dbo].[TestingItemConfiguration] ([TestingItemID])
GO
ALTER TABLE [dbo].[TestItem_TubeGroup] CHECK CONSTRAINT [FK_TestItem_TubeGroup_TestingItemConfiguration]
GO
/****** Object:  ForeignKey [FK_TestItemInTubeGroup_TubeGroup]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[TestItem_TubeGroup]  WITH CHECK ADD  CONSTRAINT [FK_TestItemInTubeGroup_TubeGroup] FOREIGN KEY([TubeGroupID])
REFERENCES [dbo].[TubeGroup] ([TubeGroupID])
GO
ALTER TABLE [dbo].[TestItem_TubeGroup] CHECK CONSTRAINT [FK_TestItemInTubeGroup_TubeGroup]
GO
/****** Object:  ForeignKey [FK_Tube_PlatePosition_DWPlatePosition]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[Tube_PlatePosition]  WITH CHECK ADD  CONSTRAINT [FK_Tube_PlatePosition_DWPlatePosition] FOREIGN KEY([DWPlatePositionID])
REFERENCES [dbo].[DWPlatePosition] ([DWPlatePositionID])
GO
ALTER TABLE [dbo].[Tube_PlatePosition] CHECK CONSTRAINT [FK_Tube_PlatePosition_DWPlatePosition]
GO
/****** Object:  ForeignKey [FK_Tube_PlatePosition_Tubes]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[Tube_PlatePosition]  WITH CHECK ADD  CONSTRAINT [FK_Tube_PlatePosition_Tubes] FOREIGN KEY([TubeID])
REFERENCES [dbo].[Tubes] ([TubeID])
GO
ALTER TABLE [dbo].[Tube_PlatePosition] CHECK CONSTRAINT [FK_Tube_PlatePosition_Tubes]
GO
/****** Object:  ForeignKey [FK_TubeGroup_PoolingRulesConfiguration]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[TubeGroup]  WITH CHECK ADD  CONSTRAINT [FK_TubeGroup_PoolingRulesConfiguration] FOREIGN KEY([PoolingRulesID])
REFERENCES [dbo].[PoolingRulesConfiguration] ([PoolingRulesID])
GO
ALTER TABLE [dbo].[TubeGroup] CHECK CONSTRAINT [FK_TubeGroup_PoolingRulesConfiguration]
GO
/****** Object:  ForeignKey [FK_TubeGroup_TubesBatch]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[TubeGroup]  WITH CHECK ADD  CONSTRAINT [FK_TubeGroup_TubesBatch] FOREIGN KEY([TubesBatchID])
REFERENCES [dbo].[TubesBatch] ([TubesBatchID])
GO
ALTER TABLE [dbo].[TubeGroup] CHECK CONSTRAINT [FK_TubeGroup_TubesBatch]
GO
/****** Object:  ForeignKey [FK_Tubes_TubeGroup]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[Tubes]  WITH CHECK ADD  CONSTRAINT [FK_Tubes_TubeGroup] FOREIGN KEY([TubeGroupID])
REFERENCES [dbo].[TubeGroup] ([TubeGroupID])
GO
ALTER TABLE [dbo].[Tubes] CHECK CONSTRAINT [FK_Tubes_TubeGroup]
GO
/****** Object:  ForeignKey [FK_TubesBatch_ExperimentsInfo]    Script Date: 02/10/2012 15:24:13 ******/
ALTER TABLE [dbo].[TubesBatch]  WITH CHECK ADD  CONSTRAINT [FK_TubesBatch_ExperimentsInfo] FOREIGN KEY([ExperimentID])
REFERENCES [dbo].[ExperimentsInfo] ([ExperimentID])
GO
ALTER TABLE [dbo].[TubesBatch] CHECK CONSTRAINT [FK_TubesBatch_ExperimentsInfo]
GO
