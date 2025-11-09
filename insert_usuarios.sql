-- Script para insertar usuarios iniciales en la base de datos aldeauruel
-- Fecha: 2025-11-09

-- Insertar usuario Administrador
INSERT INTO Usuarios (NombreUsuario, Password, NombreCompleto, Rol, Activo, FechaCreacion)
VALUES (
    'admin',
    'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', -- Hash de 'admin123'
    'Administrador del Sistema',
    'Administrador',
    1,
    GETDATE()
);

-- Insertar usuario Operador
INSERT INTO Usuarios (NombreUsuario, Password, NombreCompleto, Rol, Activo, FechaCreacion)
VALUES (
    'operador',
    'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', -- Hash de 'operador123'
    'Operador del Sistema',
    'Operador',
    1,
    GETDATE()
);

-- Verificar que se insertaron correctamente
SELECT Id, NombreUsuario, NombreCompleto, Rol, Activo, FechaCreacion
FROM Usuarios;
