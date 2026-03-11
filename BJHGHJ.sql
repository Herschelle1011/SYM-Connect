-- Rename table to avoid VS conflict
EXEC sp_rename 'dbo.Users', 'AppUsers';



INSERT INTO dbo.AppUsers (FullName, Email, PasswordHash, Role, Status, CreatedAt)
VALUES 
('Herschelle G. Libradilla', 'Anchill@gmail.com',  '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Leader', 'Active',   GETDATE()),
('Regil V. Sanchez',         'Regil@gmail.com',    '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Member', 'Inactive', GETDATE()),
('Janicar Lepiten',          'Jani@gmail.com',     '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Leader', 'Inactive', GETDATE()),
('Louis Vladimir E. Padigos','Louis12@gmail.com',  '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Leader', 'Active',   GETDATE()),
('Wind Say Ais D. Retasa',   'ais@gmail.com',      '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Member', 'Inactive', GETDATE());