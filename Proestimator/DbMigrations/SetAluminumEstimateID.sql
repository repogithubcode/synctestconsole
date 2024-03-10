UPDATE SiteGlobals
SET AluminumEstimateID = (SELECT MAX(ID) FROM AdminInfo)