USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE procedure [dbo].[cspDefragIndexes] 
	( @tableIn		varchar(200) = NULL
	, @ExecuteDefrag	chAR(1)		 = 'N' )
	
AS
SET NOCOUNT ON;
DECLARE @objectid       int;
declare @holdOid        int
DECLARE @indexid        int;
declare @DBid           int
DECLARE @partitioncount bigint;
DECLARE @schemaname     sysname;
DECLARE @objectname     sysname;
DECLARE @indexname		sysname;
DECLARE @partitionnum	bigint;
DECLARE @IndexFragmentationCursor	bigint;
DECLARE @frag			float;
DECLARE @command		varchar(8000);
declare @PageLock       int
declare @cnt			int

select @dbid = db_id()
print 'Database id = ' + convert(varchar(10), @dbid)

--************************************************************************
-- If Table id is Null will run for ALL tables
--************************************************************************
if @tablein is null
	set @objectid = NULL
else
	select @objectid = object_id from sys.objects where name = @tableIN 

SELECT
    object_id AS objectid,
    index_id AS indexid,
    partition_number AS partitionnum,
    avg_fragmentation_in_percent AS frag
INTO #IndexFragmentationTable
FROM sys.dm_db_index_physical_stats (@dbid, @objectid, NULL , NULL, 'LIMITED')
order by Object_Id

select @cnt = count(*) from #IndexFragmentationTable

-- Declare the cursor for the list of IndexFragmentationCursor to be processed.
If @ExecuteDefrag = 'Y' --> then defrag, else print defrag table
  begin

	if @tableIn is not null
		print 'Defrag indexes for table ' + @tableIn
	else 
		Print 'Defrag Indexes for entire database' 

	print 'Starting process for ' + convert(varchar(10), @cnt) + ' indexes'
	
	DECLARE IndexFragmentationCursor CURSOR 
	FOR 
		SELECT * 
		FROM #IndexFragmentationTable
		order by objectId, IndexId;

	-- Open the cursor.
	OPEN IndexFragmentationCursor;

	-- Loop through the IndexFragmentationCursor.
	FETCH NEXT
	   FROM IndexFragmentationCursor
	   INTO @objectid, @indexid, @partitionnum, @frag;

	set @holdOid = @objectid

	WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @objectname = o.name, @schemaname = s.name
			FROM sys.objects AS o
			JOIN sys.schemas as s ON s.schema_id = o.schema_id
			WHERE o.object_id = @objectid;

			if @objectid != @HoldOid
			  begin
				set @command = 'update statistics ' + @objectname
				print @command
				execute(@command)
				set @holdOid = @objectId
			  end
					

			SELECT	@indexname	= name 
				,	@PageLock	= Allow_Page_locks
			FROM sys.indexes
			WHERE  object_id = @objectid AND index_id = @indexid;

			SELECT @partitioncount = count (*) 
			FROM sys.partitions
			WHERE object_id = @objectid AND index_id = @indexid;

		IF  @frag < 30.0
		and @frag > 5.0
		and @PageLock = 1
			  BEGIN
				SELECT @command = 'ALTER INDEX ' + @indexname + ' ON ' + @schemaname + '.' + @objectname + ' REORGANIZE';
				IF @partitioncount > 1
					SELECT @command = @command + ' PARTITION=' + CONVERT (CHAR, @partitionnum);
				EXEC (@command);
				PRINT 'Executed ' + @command;

			  END;
		else
		IF @frag >= 30.0
		or  ( @PageLock = 0 and @frag > 5.0 )
		  BEGIN
			SELECT @command = 'ALTER INDEX ' + @indexname +' ON ' + @schemaname + '.' + @objectname + ' REBUILD';
			IF @partitioncount > 1
				SELECT @command = @command + ' PARTITION=' + CONVERT (CHAR, @partitionnum);
			EXEC (@command);
			PRINT 'Executed ' + @command;
		  END;
		else
			Print 'Bypassed fragmentation for ' + @indexname

		FETCH NEXT FROM IndexFragmentationCursor INTO @objectid, @indexid, @partitionnum, @frag;
	  END;

	set @command = 'update statistics ' + @objectname
	print @command
	execute(@command)

	-- Close and deallocate the cursor.
	CLOSE IndexFragmentationCursor;
	DEALLOCATE IndexFragmentationCursor;
  end
else
	select	 t.name			as TableName 
			,i.name			as IndexName 
			,w.IndexId 
			,w.Partitionnum	As Part 
			,w.frag			as FragPct
			, fill_factor
			,i.type_Desc     as IndexType
			, Case is_unique
				when 1	then 'Y'
				else ' '
			  end as 'Unique'
			, case is_primary_key
				when 1	then 'Y'
				else ' '
			  end as PKIndex
	 from #IndexFragmentationTable w
	 inner join sys.objects t on w.objectId = t.object_id 
	 inner join sys.indexes i on w.objectId = i.object_id and w.indexId = i.index_id 
	 where i.type_desc not like '%HEAP%'
	 and w.frag > '6.00'
	order by t.name, i.Index_Id
	
-- drop the temporary table

	drop table #IndexFragmentationTable


GO
