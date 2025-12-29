CREATE OR REPLACE VIEW vw_Projects AS
SELECT 
    Id,
    TenantId,
    Name,
    Status,
    UpdatedAt
FROM Projects;

