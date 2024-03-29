USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[RecompileAllProcedures]
as
begin
    declare cur cursor for 
    (
        select quotename(s.name) + '.' + quotename(o.name) as procname
        from 
           sys.objects o
           inner join sys.schemas s on o.schema_id = s.schema_id
        where  o.[type] in ('P', 'FN', 'IF')
    );

    declare @procname sysname;

    open cur;
    fetch next from cur into @procname;
    while @@fetch_status=0 
    begin
        exec sp_recompile @procname;
        fetch next from cur into @procname;
    end;
    close cur;
    deallocate cur;
end;
GO
