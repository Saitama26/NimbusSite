CREATE OR REPLACE VIEW vw_Tasks AS
SELECT 
    Id,
    ProjectId,
    TenantId,
    Title,
    Status,
    UpdatedAt
FROM Tasks;

