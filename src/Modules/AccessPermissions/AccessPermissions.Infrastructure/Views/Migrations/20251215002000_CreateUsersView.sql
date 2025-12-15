CREATE VIEW vw_Users AS
SELECT 
    Id,
    Email,
    Name,
    Status,
    UpdatedAt
FROM NimbusSite_Users.Users;

