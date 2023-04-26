USE [SUTECH]
GO

/****** Object:  Table [dbo].[Result]    Script Date: 19.03.2023 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Result](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SILO1] [nvarchar](20) NULL,
	[SILO2] [nvarchar](20) NULL,
	[SILO3] [nvarchar](20) NULL,
	[SILO4] [nvarchar](20) NULL,
	[SILO5] [nvarchar](20) NULL,
	[SILO6] [nvarchar](20) NULL,
	[SILO7] [nvarchar](20) NULL,
	[SILO8] [nvarchar](20) NULL,
	[HAT1] [nvarchar](20) NULL,
	[HAT2] [nvarchar](20) NULL,
	[TARIH] [nvarchar](20) NULL,
 CONSTRAINT [PK__Result__0519C6AF] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO