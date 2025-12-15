CREATE VIEW vw_Tenants AS
SELECT 
    Id,
    Name,
    Status,
    Subdomain,
    UpdatedAt
FROM NimbusSite_Tenants.Tenants;

