Update EmailTemplate
set Subject = 'Selected Add On, No Payment Made - Acct # #LoginId#', Template = '<p>Account: #LoginId#</p>  <p>Contract ID: #ContractID#</p>  <p>Add On type: #AddOnType#</p>  <p>Terms: #PaymentTerms#</p>  <p>Sales Rep: #SalesRep#</p>'
where Name = 'Select Add On'