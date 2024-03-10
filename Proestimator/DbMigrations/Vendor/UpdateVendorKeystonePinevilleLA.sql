IF EXISTS(SELECT 1 FROM Vendor WHERE TRIM(CompanyName) = 'KEYSTONE - PINEVILLE, LA' AND TRIM([State]) = 'CA')
BEGIN
	UPDATE Vendor SET [State] = 'LA' 
	WHERE TRIM(CompanyName) = 'KEYSTONE - PINEVILLE, LA' AND TRIM([State]) = 'CA';
END
ELSE
BEGIN
	PRINT 'No record to update';
END