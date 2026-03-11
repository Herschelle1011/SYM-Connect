INSERT INTO dbo.Users (FullName, Email, PasswordHash, Role, CreatedAt)
VALUES (
    'Test User',
    'test@email.com',
    '123456',
    'Admin',
    GETDATE()
);

SELECT * FROM dbo.Users;  