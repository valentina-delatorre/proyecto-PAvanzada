/* =========================================================================
   04 - Migracion de los datos viejos al modelo nuevo.

   Solo hace falta en la base de la facultad, donde ya habia datos cargados.
   En una base limpia no hay nada que migrar y se puede saltear.

   Orden: 01 (que renombra las tablas viejas a *_old) -> 02 -> 04 -> 03.

   Para no andar con cursores, PERSONA y TURNOS llevan durante la migracion
   una columna auxiliar con el id viejo. Al final se borran.

   La base vieja de la facultad es anterior a la local: Usuario no tiene
   Email ni Telefono, y no existen DatoPaciente ni CodigoVerificacion.
   Las especialidades viejas vienen con tilde ("Cardiología"), por eso se
   comparan sin acentos para no duplicarlas.
   ========================================================================= */

USE ConsultorioI;
GO

-- SSMS lo pone solo, sqlcmd no, y sin esto fallan el indice filtrado
-- del correo y la creacion de los triggers.
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('Usuario_old', 'U') IS NULL
BEGIN
    PRINT 'No hay tablas *_old: la base estaba limpia, no hay nada que migrar.';
    RETURN;
END
GO

ALTER TABLE PERSONA ADD MIG_USUARIO INT NULL, MIG_MEDICO INT NULL;
ALTER TABLE TURNOS   ADD MIG_TURNO   INT NULL;
GO

/* ----- 1. Usuarios -> PERSONA + USUARIOS --------------------------------- */

-- por las dudas, cualquier rol viejo que no este en la tabla ROLES
INSERT INTO ROLES (NOMBRE_ROL)
SELECT DISTINCT U.Rol FROM Usuario_old U
WHERE U.Rol IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM ROLES R WHERE R.NOMBRE_ROL = U.Rol);

INSERT INTO PERSONA (NOMBRE, APELLIDO, ID_CATEGORIA, MIG_USUARIO)
SELECT U.NombreUsuario, '(sin completar)',
       (SELECT ID_CATEGORIA FROM CATEGORIA WHERE NOMBRE =
            CASE U.Rol WHEN 'Medico'   THEN 'Profesional'
                       WHEN 'Paciente' THEN 'Paciente'
                       ELSE 'Administrativo' END),
       U.IdUsuario
FROM Usuario_old U;

INSERT INTO USUARIOS (ID_PERSONA, NOMBRE_USUARIOS, CONTRASENIA, ESTADO, ID_ROL)
SELECT P.ID_PERSONA, U.NombreUsuario, U.Contrasenia, U.Activo, R.ID_ROL
FROM Usuario_old U
JOIN PERSONA P ON P.MIG_USUARIO = U.IdUsuario
JOIN ROLES   R ON R.NOMBRE_ROL  = U.Rol;
GO

/* ----- 2. Fichas de paciente -> completan la PERSONA --------------------- */

-- (solo si la base vieja tenia esa tabla; en la facultad no existe)
IF OBJECT_ID('DatoPaciente_old', 'U') IS NOT NULL
EXEC('UPDATE P
      SET NOMBRE = D.Nombre, APELLIDO = D.Apellido, TIPO_DNI = ''DNI'', DNI = D.Dni,
          TELEFONO = ISNULL(D.Telefono, P.TELEFONO), CORREO = ISNULL(D.Email, P.CORREO)
      FROM PERSONA P
      JOIN DatoPaciente_old D ON D.IdUsuario = P.MIG_USUARIO');
GO

/* ----- 3. Medicos -> personas profesionales ------------------------------ */

INSERT INTO ESPECIALIDAD (NOMBRE)
SELECT DISTINCT M.Especialidad FROM Medico_old M
WHERE M.Especialidad IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM ESPECIALIDAD E
                  WHERE E.NOMBRE = M.Especialidad COLLATE Modern_Spanish_CI_AI);

INSERT INTO ESPECIALIDAD (NOMBRE)
SELECT DISTINCT T.Especialidad FROM Turno_old T
WHERE T.Especialidad IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM ESPECIALIDAD E
                  WHERE E.NOMBRE = T.Especialidad COLLATE Modern_Spanish_CI_AI);
GO

DECLARE @prof INT = (SELECT ID_CATEGORIA FROM CATEGORIA WHERE NOMBRE = 'Profesional');

-- el medico que ya tenia usuario reusa su persona
UPDATE P
SET NOMBRE = M.Nombre, APELLIDO = M.Apellido, ID_CATEGORIA = @prof,
    ID_ESPECIALIDAD = E.ID_ESPECIALIDAD, MIG_MEDICO = M.IdMedico
FROM PERSONA P
JOIN Medico_old M        ON M.IdUsuario = P.MIG_USUARIO
LEFT JOIN ESPECIALIDAD E ON E.NOMBRE    = M.Especialidad COLLATE Modern_Spanish_CI_AI;

-- el que no tenia usuario entra como persona nueva
INSERT INTO PERSONA (NOMBRE, APELLIDO, ID_CATEGORIA, ID_ESPECIALIDAD, MIG_MEDICO)
SELECT M.Nombre, M.Apellido, @prof, E.ID_ESPECIALIDAD, M.IdMedico
FROM Medico_old M
LEFT JOIN ESPECIALIDAD E ON E.NOMBRE = M.Especialidad COLLATE Modern_Spanish_CI_AI
WHERE NOT EXISTS (SELECT 1 FROM PERSONA P WHERE P.MIG_MEDICO = M.IdMedico);
GO

-- la matricula es lo que deja al profesional activo o inactivo
INSERT INTO MATRICULA (ID_PERSONA, NRO_MATRICULA, ESTADO)
SELECT P.ID_PERSONA, 'A completar', M.Activo
FROM PERSONA P
JOIN Medico_old M ON M.IdMedico = P.MIG_MEDICO;

-- agenda semanal por defecto, para que los turnos tengan de donde colgar
INSERT INTO AGENDA (ID_PERSONA_PROFESIONAL, DIA_ATENCION, HORA_INICIO, HORA_FIN)
SELECT P.ID_PERSONA, d.DIA, '09:00', '18:00'
FROM PERSONA P
CROSS JOIN (VALUES ('Lunes'), ('Martes'), ('Miercoles'), ('Jueves'), ('Viernes')) AS d(DIA)
WHERE P.MIG_MEDICO IS NOT NULL;
GO

/* ----- 4. Turnos --------------------------------------------------------- */

INSERT INTO TURNOS (ID_AGENDA, ID_PERSONA_PACIENTE, FECHA_TURNO, HORA_TURNO,
                   ESTADO, OBSERVACIONES, MIG_TURNO)
SELECT (SELECT TOP 1 A.ID_AGENDA
        FROM AGENDA A
        JOIN PERSONA PM ON PM.ID_PERSONA = A.ID_PERSONA_PROFESIONAL
        WHERE PM.MIG_MEDICO = T.IdMedico),
       P.ID_PERSONA, T.Fecha, CAST(T.Hora AS TIME),
       T.Estado, T.Observaciones, T.IdTurno
FROM Turno_old T
JOIN PERSONA P ON P.MIG_USUARIO = T.IdUsuarioPaciente;

INSERT INTO TURNO_APP (ID_TURNO, ESPECIALIDAD_SOLICITADA, TIPO_CONSULTA)
SELECT N.ID_TURNO, T.Especialidad, T.TipoConsulta
FROM TURNOS N
JOIN Turno_old T ON T.IdTurno = N.MIG_TURNO;
GO

/* ----- 5. Codigos de verificacion ---------------------------------------- */

-- (idem: en la facultad no habia tabla de codigos)
IF OBJECT_ID('CodigoVerificacion_old', 'U') IS NOT NULL
EXEC('INSERT INTO CodigoVerificacion (IdUsuario, Codigo, FechaExpira, Usado)
      SELECT U.ID_USUARIOS, C.Codigo, C.FechaExpira, C.Usado
      FROM CodigoVerificacion_old C
      JOIN PERSONA P  ON P.MIG_USUARIO = C.IdUsuario
      JOIN USUARIOS U ON U.ID_PERSONA  = P.ID_PERSONA');
GO

/* ----- 6. Correo de inicio de sesion -------------------------------------
   Ahora se entra con el correo. El que en la base vieja no lo tenia cargado
   se quedaria afuera, asi que le armamos uno provisorio con su nombre de
   usuario. Hay que reemplazarlo por el correo real de cada uno.
   -------------------------------------------------------------------------- */

UPDATE P
SET CORREO = U.NOMBRE_USUARIOS + '@consultorio.local'
FROM PERSONA P
JOIN USUARIOS U ON U.ID_PERSONA = P.ID_PERSONA
WHERE P.CORREO IS NULL OR P.CORREO = '';

SELECT U.NOMBRE_USUARIOS, P.CORREO AS CORREO_PROVISORIO
FROM USUARIOS U
JOIN PERSONA P ON P.ID_PERSONA = U.ID_PERSONA
WHERE P.CORREO LIKE '%@consultorio.local';
GO

/* ----- 7. Limpieza ------------------------------------------------------- */

ALTER TABLE PERSONA DROP COLUMN MIG_USUARIO, MIG_MEDICO;
ALTER TABLE TURNOS   DROP COLUMN MIG_TURNO;
GO

-- Las tablas *_old quedan por si hay que mirar algo. Cuando este todo
-- verificado se pueden borrar con DROP TABLE.
SELECT 'PERSONA' AS TABLA, COUNT(*) AS FILAS FROM PERSONA
UNION ALL SELECT 'USUARIOS', COUNT(*) FROM USUARIOS
UNION ALL SELECT 'MATRICULA', COUNT(*) FROM MATRICULA
UNION ALL SELECT 'AGENDA', COUNT(*) FROM AGENDA
UNION ALL SELECT 'TURNOS', COUNT(*) FROM TURNOS;
GO
