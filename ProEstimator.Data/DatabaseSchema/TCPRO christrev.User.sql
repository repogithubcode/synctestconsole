USE [FocusWrite]
GO
CREATE USER [TCPRO\christrev] WITH DEFAULT_SCHEMA=[TCPRO\christrev]
GO
ALTER ROLE [db_owner] ADD MEMBER [TCPRO\christrev]
GO
