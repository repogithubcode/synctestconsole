IF NOT EXISTS (SELECT 1 FROM SalesRepPermission WHERE Tag = 'CarFaxEdit')
BEGIN
  INSERT INTO SalesRepPermission 
	(PermissionGroup, Tag, Name, Description)
  VALUES 
	(3, 'CarFaxEdit', 'Car Fax: Allow Edits', 'Allows users with access to Car Fax to edit the information.')
END