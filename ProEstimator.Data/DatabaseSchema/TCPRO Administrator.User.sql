USE [FocusWrite]
GO
CREATE USER [TCPRO\Administrator] WITH DEFAULT_SCHEMA=[TCPRO\Administrator]
GO
ALTER ROLE [db_owner] ADD MEMBER [TCPRO\Administrator]
GO
