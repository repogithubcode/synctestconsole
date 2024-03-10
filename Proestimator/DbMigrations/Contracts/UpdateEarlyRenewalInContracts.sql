update contracts set EarlyRenewal = 1 where ContractID in (
select ContractID from contracts where effectivedate > dateadd(dd,30,datecreated) and promoid > 0 and datecreated > '2022-01-01'
or contractid in (
select contractid from invoices
where invoices.notes like 'EARLYRENEWAL%' and addonid = 0)
)