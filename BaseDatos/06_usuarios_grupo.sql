/* =========================================================================
   06 - Usuarios del grupo y un medico por especialidad.
   Se entra con el correo. Contrasena de todos: Usal2026.
   Se puede correr las veces que haga falta: no duplica nada.
   Va despues del 03 (usa las tablas nuevas directamente).
   ========================================================================= */

USE ConsultorioI;
GO

-- SSMS lo pone solo, sqlcmd no, y sin esto falla el indice filtrado del correo.
SET QUOTED_IDENTIFIER ON;
GO

/* ----- Integrantes del grupo (categoria Administrativo) ------------------- */

INSERT INTO PERSONA (NOMBRE, APELLIDO, CORREO, ID_CATEGORIA)
SELECT v.NOMBRE, v.APELLIDO, v.CORREO,
       (SELECT ID_CATEGORIA FROM CATEGORIA WHERE NOMBRE = 'Administrativo')
FROM (VALUES
    ('Valentina', 'De la Torre', 'valentina.d@usal.edu.ar'),
    ('Micaela',   'Candore',     'micaelamorena.candore@usal.edu.ar'),
    ('Martino',   'Dominguez',   'martino.dominguez@usal.edu.ar'),
    ('Agustin',   'Villalpando', 'a.villalpando@usal.edu.ar')
) AS v(NOMBRE, APELLIDO, CORREO)
WHERE NOT EXISTS (SELECT 1 FROM PERSONA P WHERE P.CORREO = v.CORREO);

-- Cada integrante tiene el rol del modulo que le toca. Seguridad entra a todo.
INSERT INTO USUARIOS (ID_PERSONA, NOMBRE_USUARIOS, CONTRASENIA, ID_ROL)
SELECT P.ID_PERSONA, v.USUARIO, 'Usal2026', R.ID_ROL
FROM (VALUES
    ('valentina.d@usal.edu.ar',           'vdelatorre',   'Seguridad'),
    ('micaelamorena.candore@usal.edu.ar', 'mcandore',     'Usuarios'),
    ('martino.dominguez@usal.edu.ar',     'mdominguez',   'Turnos'),
    ('a.villalpando@usal.edu.ar',         'avillalpando', 'Reportes')
) AS v(CORREO, USUARIO, ROL)
JOIN PERSONA P ON P.CORREO = v.CORREO
JOIN ROLES R   ON R.NOMBRE_ROL = v.ROL
WHERE NOT EXISTS (SELECT 1 FROM USUARIOS U WHERE U.ID_PERSONA = P.ID_PERSONA);
GO

/* ----- Un medico por especialidad (Ferreyra ya es Clinica Medica) ---------- */

INSERT INTO PERSONA (NOMBRE, APELLIDO, TIPO_DNI, DNI, CORREO, TELEFONO, ID_CATEGORIA, ID_ESPECIALIDAD)
SELECT v.NOMBRE, v.APELLIDO, 'DNI', v.DNI, v.CORREO, v.TELEFONO,
       (SELECT ID_CATEGORIA FROM CATEGORIA WHERE NOMBRE = 'Profesional'),
       E.ID_ESPECIALIDAD
FROM (VALUES
    ('Laura',  'Pereyra', '29555666', 'l.pereyra@consultorio.com', '1155000004', 'Cardiologia'),
    ('Sofia',  'Acosta',  '31777888', 's.acosta@consultorio.com',  '1155000005', 'Pediatria'),
    ('Julian', 'Rios',    '27888999', 'j.rios@consultorio.com',    '1155000006', 'Traumatologia')
) AS v(NOMBRE, APELLIDO, DNI, CORREO, TELEFONO, ESPECIALIDAD)
JOIN ESPECIALIDAD E ON E.NOMBRE = v.ESPECIALIDAD
WHERE NOT EXISTS (SELECT 1 FROM PERSONA P WHERE P.CORREO = v.CORREO);

INSERT INTO USUARIOS (ID_PERSONA, NOMBRE_USUARIOS, CONTRASENIA, ID_ROL)
SELECT P.ID_PERSONA, v.USUARIO, 'Usal2026', R.ID_ROL
FROM (VALUES
    ('l.pereyra@consultorio.com', 'lpereyra'),
    ('s.acosta@consultorio.com',  'sacosta'),
    ('j.rios@consultorio.com',    'jrios')
) AS v(CORREO, USUARIO)
JOIN PERSONA P ON P.CORREO = v.CORREO
JOIN ROLES R   ON R.NOMBRE_ROL = 'Medico'
WHERE NOT EXISTS (SELECT 1 FROM USUARIOS U WHERE U.ID_PERSONA = P.ID_PERSONA);

-- La matricula activa es lo que hace que el medico figure como Activo.
INSERT INTO MATRICULA (ID_PERSONA, NRO_MATRICULA, JURISDICCION)
SELECT P.ID_PERSONA, v.MATRICULA, 'Nacional'
FROM (VALUES
    ('l.pereyra@consultorio.com', 'MN 51200'),
    ('s.acosta@consultorio.com',  'MN 48730'),
    ('j.rios@consultorio.com',    'MN 39915')
) AS v(CORREO, MATRICULA)
JOIN PERSONA P ON P.CORREO = v.CORREO
WHERE NOT EXISTS (SELECT 1 FROM MATRICULA M WHERE M.ID_PERSONA = P.ID_PERSONA);

-- Agenda de lunes a viernes, como la de Ferreyra.
INSERT INTO AGENDA (ID_PERSONA_PROFESIONAL, DIA_ATENCION, HORA_INICIO, HORA_FIN)
SELECT P.ID_PERSONA, d.DIA, '09:00', '13:00'
FROM PERSONA P
CROSS JOIN (VALUES ('Lunes'), ('Martes'), ('Miercoles'), ('Jueves'), ('Viernes')) AS d(DIA)
WHERE P.CORREO IN ('l.pereyra@consultorio.com', 's.acosta@consultorio.com', 'j.rios@consultorio.com')
  AND NOT EXISTS (SELECT 1 FROM AGENDA A WHERE A.ID_PERSONA_PROFESIONAL = P.ID_PERSONA);

-- Ferreyra pasa a la misma contrasena que el resto para las pruebas.
UPDATE U SET CONTRASENIA = 'Usal2026'
FROM USUARIOS U
JOIN PERSONA P ON P.ID_PERSONA = U.ID_PERSONA
WHERE P.CORREO = 'm.ferreyra@consultorio.com';
GO

/* ----- Las paginas nuevas de cada panel, con el permiso Panel Medico ------- */

INSERT INTO URL (RUTA, ID_PERMISO)
SELECT v.RUTA, P.ID_PERMISO
FROM (VALUES
    ('~/Medicos/PanelClinica.aspx'),
    ('~/Medicos/PanelCardiologia.aspx'),
    ('~/Medicos/PanelPediatria.aspx'),
    ('~/Medicos/PanelTraumatologia.aspx')
) AS v(RUTA)
JOIN PERMISOS P ON P.NOMBRE_PERMISO = 'Panel Medico'
WHERE NOT EXISTS (SELECT 1 FROM URL U WHERE U.RUTA = v.RUTA);
GO
