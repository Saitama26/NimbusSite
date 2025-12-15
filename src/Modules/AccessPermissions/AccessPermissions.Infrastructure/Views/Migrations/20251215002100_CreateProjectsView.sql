CREATE VIEW vw_Projects AS
SELECT 
    Id,
    TenantId,
    Name,
    Status,
    UpdatedAt
FROM NimbusSite_Projects.Projects;

