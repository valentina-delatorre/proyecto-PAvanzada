/* =========================================================================
   03 - Capa de compatibilidad.

   El modelo nuevo no tiene las tablas Usuario, DatoPaciente, Medico ni Turno,
   pero todo BIZ/Datos las consulta por esos nombres. Estas cuatro vistas las
   reconstruyen desde las tablas del DER, y los triggers INSTEAD OF resuelven
   las escrituras, porque una vista que cruza varias tablas no es actualizable
   por si sola.

   Asi la aplicacion sigue funcionando sin tocar una linea de C#.
   Se ejecuta al final: en la facultad, despues de 04_migracion.sql.
   ========================================================================= */

USE ConsultorioI;
GO

-- SSMS lo pone solo, sqlcmd no, y sin esto fallan el indice filtrado
-- del correo y la creacion de los triggers.
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('Turno', 'V')        IS NOT NULL DROP VIEW Turno;
IF OBJECT_ID('Medico', 'V')       IS NOT NULL DROP VIEW Medico;
IF OBJECT_ID('DatoPaciente', 'V') IS NOT NULL DROP VIEW DatoPaciente;
IF OBJECT_ID('Usuario', 'V')      IS NOT NULL DROP VIEW Usuario;
GO

/* ----- Usuario ----------------------------------------------------------- */

CREATE VIEW Usuario AS
SELECT U.ID_USUARIOS     AS IdUsuario,
       U.NOMBRE_USUARIOS AS NombreUsuario,
       U.CONTRASENIA     AS Contrasenia,
       R.NOMBRE_ROL      AS Rol,
       U.ESTADO          AS Activo,
       P.CORREO          AS Email,
       P.TELEFONO        AS Telefono
FROM USUARIOS U
JOIN PERSONA P ON P.ID_PERSONA = U.ID_PERSONA
JOIN ROLES   R ON R.ID_ROL     = U.ID_ROL;
GO

-- Registro.aspx solo manda usuario, contrasenia y rol. Creamos la PERSONA
-- minima aca y despues la completa la ficha del paciente.
CREATE TRIGGER TR_Usuario_Insert ON Usuario INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @nuevas TABLE (ID_PERSONA INT, NOMBRE VARCHAR(80));

    INSERT INTO PERSONA (NOMBRE, APELLIDO, CORREO, TELEFONO, ID_CATEGORIA)
    OUTPUT INSERTED.ID_PERSONA, INSERTED.NOMBRE INTO @nuevas
    SELECT i.NombreUsuario, '(sin completar)', i.Email, i.Telefono,
           (SELECT ID_CATEGORIA FROM CATEGORIA WHERE NOMBRE =
                CASE i.Rol WHEN 'Medico'   THEN 'Profesional'
                           WHEN 'Paciente' THEN 'Paciente'
                           ELSE 'Administrativo' END)
    FROM inserted i;

    INSERT INTO USUARIOS (ID_PERSONA, NOMBRE_USUARIOS, CONTRASENIA, ESTADO, ID_ROL)
    SELECT n.ID_PERSONA, i.NombreUsuario, i.Contrasenia, ISNULL(i.Activo, 1), R.ID_ROL
    FROM inserted i
    JOIN @nuevas n ON n.NOMBRE = i.NombreUsuario
    JOIN ROLES   R ON R.NOMBRE_ROL = i.Rol;
END;
GO

CREATE TRIGGER TR_Usuario_Update ON Usuario INSTEAD OF UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE U
    SET NOMBRE_USUARIOS = i.NombreUsuario,
        CONTRASENIA     = i.Contrasenia,
        ESTADO          = i.Activo,
        ID_ROL          = ISNULL((SELECT ID_ROL FROM ROLES WHERE NOMBRE_ROL = i.Rol), U.ID_ROL)
    FROM USUARIOS U
    JOIN inserted i ON i.IdUsuario = U.ID_USUARIOS;

    UPDATE P
    SET CORREO = i.Email, TELEFONO = i.Telefono
    FROM PERSONA P
    JOIN USUARIOS U ON U.ID_PERSONA = P.ID_PERSONA
    JOIN inserted i ON i.IdUsuario  = U.ID_USUARIOS;
END;
GO

/* ----- DatoPaciente ------------------------------------------------------ */
-- Es la ficha personal de un usuario: en el modelo nuevo son las columnas de
-- PERSONA. Pedimos DNI cargado porque en la base vieja la fila de
-- DatoPaciente recien existia cuando el paciente completaba la ficha, y hay
-- codigo que se apoya en eso (Obtener devuelve null si todavia no la lleno).

CREATE VIEW DatoPaciente AS
SELECT P.ID_PERSONA  AS IdDatoPaciente,
       U.ID_USUARIOS AS IdUsuario,
       P.NOMBRE      AS Nombre,
       P.APELLIDO    AS Apellido,
       P.DNI         AS Dni,
       P.TELEFONO    AS Telefono,
       P.CORREO      AS Email
FROM PERSONA P
JOIN USUARIOS U ON U.ID_PERSONA = P.ID_PERSONA
WHERE P.DNI IS NOT NULL;
GO

CREATE TRIGGER TR_DatoPaciente_Update ON DatoPaciente INSTEAD OF UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE P
    SET NOMBRE = i.Nombre, APELLIDO = i.Apellido, DNI = i.Dni,
        TELEFONO = i.Telefono, CORREO = i.Email
    FROM PERSONA P
    JOIN USUARIOS U ON U.ID_PERSONA = P.ID_PERSONA
    JOIN inserted i ON i.IdUsuario  = U.ID_USUARIOS;
END;
GO

-- La PERSONA siempre existe, asi que un alta se resuelve completandola.
CREATE TRIGGER TR_DatoPaciente_Insert ON DatoPaciente INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE P
    SET NOMBRE = i.Nombre, APELLIDO = i.Apellido, DNI = i.Dni,
        TELEFONO = i.Telefono, CORREO = i.Email
    FROM PERSONA P
    JOIN USUARIOS U ON U.ID_PERSONA = P.ID_PERSONA
    JOIN inserted i ON i.IdUsuario  = U.ID_USUARIOS;
END;
GO

/* ----- Medico ------------------------------------------------------------ */
-- En el DER no hay tabla de medicos: un medico es una PERSONA de categoria
-- Profesional, con especialidad y matricula. Por eso IdMedico es ID_PERSONA
-- y "Activo" sale del estado de la matricula, que es lo que lo habilita.

CREATE VIEW Medico AS
SELECT P.ID_PERSONA  AS IdMedico,
       U.ID_USUARIOS AS IdUsuario,
       P.NOMBRE      AS Nombre,
       P.APELLIDO    AS Apellido,
       ISNULL(E.NOMBRE, 'Sin especialidad') AS Especialidad,
       CAST(ISNULL((SELECT MAX(CAST(M.ESTADO AS TINYINT))
                    FROM MATRICULA M WHERE M.ID_PERSONA = P.ID_PERSONA), 0) AS BIT) AS Activo
FROM PERSONA P
JOIN CATEGORIA C         ON C.ID_CATEGORIA     = P.ID_CATEGORIA AND C.NOMBRE = 'Profesional'
LEFT JOIN ESPECIALIDAD E ON E.ID_ESPECIALIDAD  = P.ID_ESPECIALIDAD
LEFT JOIN USUARIOS U     ON U.ID_PERSONA       = P.ID_PERSONA;
GO

CREATE TRIGGER TR_Medico_Insert ON Medico INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @cat INT = (SELECT ID_CATEGORIA FROM CATEGORIA WHERE NOMBRE = 'Profesional');

    INSERT INTO ESPECIALIDAD (NOMBRE)
    SELECT DISTINCT i.Especialidad FROM inserted i
    WHERE i.Especialidad IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM ESPECIALIDAD E WHERE E.NOMBRE = i.Especialidad);

    -- si el alta se hace sobre un usuario ya existente, se reusa su persona
    UPDATE P
    SET NOMBRE = i.Nombre, APELLIDO = i.Apellido,
        ID_CATEGORIA = @cat, ID_ESPECIALIDAD = E.ID_ESPECIALIDAD
    FROM PERSONA P
    JOIN USUARIOS U ON U.ID_PERSONA = P.ID_PERSONA
    JOIN inserted i ON i.IdUsuario  = U.ID_USUARIOS
    LEFT JOIN ESPECIALIDAD E ON E.NOMBRE = i.Especialidad;

    -- y si no, se crea una persona nueva
    INSERT INTO PERSONA (NOMBRE, APELLIDO, ID_CATEGORIA, ID_ESPECIALIDAD)
    SELECT i.Nombre, i.Apellido, @cat, E.ID_ESPECIALIDAD
    FROM inserted i
    LEFT JOIN ESPECIALIDAD E ON E.NOMBRE = i.Especialidad
    WHERE i.IdUsuario IS NULL
       OR NOT EXISTS (SELECT 1 FROM USUARIOS U WHERE U.ID_USUARIOS = i.IdUsuario);

    INSERT INTO MATRICULA (ID_PERSONA, NRO_MATRICULA, ESTADO)
    SELECT P.ID_PERSONA, 'A completar', 1
    FROM PERSONA P
    WHERE P.ID_CATEGORIA = @cat
      AND NOT EXISTS (SELECT 1 FROM MATRICULA M WHERE M.ID_PERSONA = P.ID_PERSONA);
END;
GO

CREATE TRIGGER TR_Medico_Update ON Medico INSTEAD OF UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE M SET ESTADO = i.Activo
    FROM MATRICULA M
    JOIN inserted i ON i.IdMedico = M.ID_PERSONA;

    UPDATE P
    SET NOMBRE = i.Nombre, APELLIDO = i.Apellido,
        ID_ESPECIALIDAD = ISNULL(E.ID_ESPECIALIDAD, P.ID_ESPECIALIDAD)
    FROM PERSONA P
    JOIN inserted i ON i.IdMedico = P.ID_PERSONA
    LEFT JOIN ESPECIALIDAD E ON E.NOMBRE = i.Especialidad;
END;
GO

/* ----- Turno ------------------------------------------------------------- */
-- El medico del turno sale de la agenda a la que esta enganchado.
-- La especialidad pedida y el tipo de consulta viven en TURNO_APP.

CREATE VIEW Turno AS
SELECT T.ID_TURNO     AS IdTurno,
       U.ID_USUARIOS  AS IdUsuarioPaciente,
       A.ID_PERSONA_PROFESIONAL AS IdMedico,
       ISNULL(TA.ESPECIALIDAD_SOLICITADA, ISNULL(E.NOMBRE, 'Sin especialidad')) AS Especialidad,
       TA.TIPO_CONSULTA AS TipoConsulta,
       T.FECHA_TURNO    AS Fecha,
       CONVERT(VARCHAR(5), T.HORA_TURNO, 108) AS Hora,
       T.ESTADO         AS Estado,
       T.OBSERVACIONES  AS Observaciones
FROM TURNOS T
JOIN USUARIOS U ON U.ID_PERSONA = T.ID_PERSONA_PACIENTE
LEFT JOIN AGENDA A       ON A.ID_AGENDA       = T.ID_AGENDA
LEFT JOIN PERSONA PR     ON PR.ID_PERSONA     = A.ID_PERSONA_PROFESIONAL
LEFT JOIN ESPECIALIDAD E ON E.ID_ESPECIALIDAD = PR.ID_ESPECIALIDAD
LEFT JOIN TURNO_APP TA   ON TA.ID_TURNO       = T.ID_TURNO;
GO

CREATE TRIGGER TR_Turno_Insert ON Turno INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- el turno se engancha a una agenda, asi que si el profesional todavia no
    -- tiene ninguna se le crea una
    INSERT INTO AGENDA (ID_PERSONA_PROFESIONAL, DIA_ATENCION, HORA_INICIO, HORA_FIN)
    SELECT DISTINCT i.IdMedico, 'A confirmar', '09:00', '18:00'
    FROM inserted i
    WHERE i.IdMedico IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM AGENDA A WHERE A.ID_PERSONA_PROFESIONAL = i.IdMedico);

    INSERT INTO TURNOS (ID_AGENDA, ID_PERSONA_PACIENTE, FECHA_TURNO, HORA_TURNO, ESTADO, OBSERVACIONES)
    SELECT (SELECT TOP 1 A.ID_AGENDA FROM AGENDA A
            WHERE A.ID_PERSONA_PROFESIONAL = i.IdMedico AND A.ESTADO = 1),
           U.ID_PERSONA, i.Fecha, CAST(i.Hora AS TIME),
           ISNULL(i.Estado, 'Solicitado'), i.Observaciones
    FROM inserted i
    JOIN USUARIOS U ON U.ID_USUARIOS = i.IdUsuarioPaciente;

    INSERT INTO TURNO_APP (ID_TURNO, ESPECIALIDAD_SOLICITADA, TIPO_CONSULTA)
    SELECT T.ID_TURNO, i.Especialidad, i.TipoConsulta
    FROM TURNOS T
    JOIN USUARIOS U ON U.ID_PERSONA = T.ID_PERSONA_PACIENTE
    JOIN inserted i ON i.IdUsuarioPaciente = U.ID_USUARIOS
                   AND i.Fecha = T.FECHA_TURNO
                   AND CAST(i.Hora AS TIME) = T.HORA_TURNO
    WHERE NOT EXISTS (SELECT 1 FROM TURNO_APP X WHERE X.ID_TURNO = T.ID_TURNO);
END;
GO

CREATE TRIGGER TR_Turno_Update ON Turno INSTEAD OF UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AGENDA (ID_PERSONA_PROFESIONAL, DIA_ATENCION, HORA_INICIO, HORA_FIN)
    SELECT DISTINCT i.IdMedico, 'A confirmar', '09:00', '18:00'
    FROM inserted i
    WHERE i.IdMedico IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM AGENDA A WHERE A.ID_PERSONA_PROFESIONAL = i.IdMedico);

    UPDATE T
    SET ESTADO        = i.Estado,
        OBSERVACIONES = i.Observaciones,
        FECHA_TURNO   = i.Fecha,
        HORA_TURNO    = CAST(i.Hora AS TIME),
        ID_AGENDA     = ISNULL((SELECT TOP 1 A.ID_AGENDA FROM AGENDA A
                                WHERE A.ID_PERSONA_PROFESIONAL = i.IdMedico AND A.ESTADO = 1),
                               T.ID_AGENDA)
    FROM TURNOS T
    JOIN inserted i ON i.IdTurno = T.ID_TURNO;

    UPDATE TA
    SET ESPECIALIDAD_SOLICITADA = i.Especialidad, TIPO_CONSULTA = i.TipoConsulta
    FROM TURNO_APP TA
    JOIN inserted i ON i.IdTurno = TA.ID_TURNO;
END;
GO
