use [SYM-Connect]


INSERT INTO dbo.Users
(FullName, Email, PasswordHash, Role, CreatedAt, Status)
VALUES
('Maria Santos','maria@gmail.com','hash123','admin',GETDATE(),'active'),
('Juan Dela Cruz','juan@gmail.com','hash456','Leader',GETDATE(),'active'),
('Ana Reyes','ana@gmail.com','hash789','Member',GETDATE(),'active');

INSERT INTO dbo.Users
(FullName, Email, PasswordHash, Role, CreatedAt, Status)
VALUES
('Maria Santos','ais@gmail.com','','admin',GETDATE(),'active')

