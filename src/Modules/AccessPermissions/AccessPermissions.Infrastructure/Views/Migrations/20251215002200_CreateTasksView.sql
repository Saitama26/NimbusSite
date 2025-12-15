CREATE VIEW vw_Tasks AS
SELECT 
    Id,
    ProjectId,
    TenantId,
    Title,
    Status,
    UpdatedAt
FROM NimbusSite_Tasks.Tasks;

