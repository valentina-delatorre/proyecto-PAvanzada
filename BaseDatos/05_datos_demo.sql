/* ============================================================================
   05_datos_demo.sql  -  Actividad de prueba para la demo
   ----------------------------------------------------------------------------
   Corre DESPUES de 01_esquema + 02_datos + 03_compatibilidad.
   Agrega pacientes y turnos para que los paneles dejen de mostrar ceros.

   Las fechas son relativas a GETDATE(), asi que el script sirve cualquier dia:
   no hay que retocarlo antes de presentar.

   Los turnos se insertan por la vista Turno, que es el mismo camino que usa
   la aplicacion, asi que de paso se prueba el trigger de compatibilidad.

   Es idempotente: si ya se corrio, no duplica nada.
   ============================================================================ */

USE ConsultorioI;
GO

SET NOCOUNT ON;
GO

/* ----- Pacientes -----------------------------------------------------------
   Con un solo paciente las grillas se ven repetidas. Sumamos cuatro.
   -------------------------------------------------------------------------- */

INSERT INTO PERSONA (NOMBRE, APELLIDO, TIPO_DNI, DNI, CORREO, TELEFONO, ID_CATEGORIA, ID_ESPECIALIDAD)
SELECT v.NOMBRE, v.APELLIDO, 'DNI', v.DNI, v.CORREO, v.TEL,
       (SELECT ID_CATEGORIA FROM CATEGORIA WHERE NOMBRE = 'Paciente'), NULL
FROM (VALUES
    ('Carla',  'Benitez',  '35222111', 'carla.benitez@mail.com',  '1155000101'),
    ('Diego',  'Sosa',     '31444555', 'diego.sosa@mail.com',     '1155000102'),
    ('Marina', 'Quiroga',  '44777888', 'marina.quiroga@mail.com', '1155000103'),
    ('Hernan', 'Ledesma',  '27333999', 'hernan.ledesma@mail.com', '1155000104')
) AS v(NOMBRE, APELLIDO, DNI, CORREO, TEL)
WHERE NOT EXISTS (SELECT 1 FROM PERSONA P WHERE P.DNI = v.DNI);

INSERT INTO USUARIOS (ID_PERSONA, NOMBRE_USUARIOS, CONTRASENIA, ID_ROL)
SELECT P.ID_PERSONA, v.USUARIO, 'paciente123',
       (SELECT ID_ROL FROM ROLES WHERE NOMBRE_ROL = 'Paciente')
FROM (VALUES
    ('35222111', 'cbenitez'),
    ('31444555', 'dsosa'),
    ('44777888', 'mquiroga'),
    ('27333999', 'hledesma')
) AS v(DNI, USUARIO)
JOIN PERSONA P ON P.DNI = v.DNI
WHERE NOT EXISTS (SELECT 1 FROM USUARIOS U WHERE U.ID_PERSONA = P.ID_PERSONA);
GO

/* ----- Turnos --------------------------------------------------------------
   Los dias se calculan sobre hoy. Tres grupos, que son los tres numeros que
   muestra el Panel Medico:

     Confirmado + hoy        -> "turnos hoy"
     Confirmado + a futuro   -> "proximos confirmados"
     Solicitado              -> "solicitudes pendientes"

   Y unos Atendido/Cancelado viejos para que el listado general y el reporte
   no arranquen en blanco.
   -------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM TURNOS)
BEGIN
    DECLARE @medico INT = (SELECT IdMedico FROM Medico WHERE Apellido = 'Ferreyra');

    INSERT INTO Turno (IdUsuarioPaciente, IdMedico, Especialidad, TipoConsulta,
                       Fecha, Hora, Estado, Observaciones)
    SELECT U.ID_USUARIOS, @medico, v.ESPECIALIDAD, v.TIPO,
           CAST(DATEADD(DAY, v.DIAS, GETDATE()) AS DATE), v.HORA, v.ESTADO, v.OBS
    FROM (VALUES
        -- Hoy, confirmados
        ('lgomez',   0, '09:00', 'Confirmado', 'Clinica Medica', 'Control',      'Control anual'),
        ('cbenitez', 0, '09:30', 'Confirmado', 'Clinica Medica', 'Primera vez',  'Derivada de guardia'),
        ('dsosa',    0, '10:30', 'Confirmado', 'Clinica Medica', 'Control',      'Trae analisis de sangre'),
        ('mquiroga', 0, '11:00', 'Confirmado', 'Clinica Medica', 'Control',      NULL),

        -- Proximos dias, confirmados
        ('hledesma', 1, '09:00', 'Confirmado', 'Clinica Medica', 'Primera vez',  'Dolor lumbar'),
        ('lgomez',   2, '10:00', 'Confirmado', 'Clinica Medica', 'Control',      'Revision de tratamiento'),
        ('cbenitez', 3, '11:30', 'Confirmado', 'Clinica Medica', 'Control',      NULL),
        ('dsosa',    6, '09:30', 'Confirmado', 'Clinica Medica', 'Control',      'Renovacion de receta'),

        -- Solicitados: son los que el medico tiene que aceptar o rechazar
        ('mquiroga', 1, '12:00', 'Solicitado', 'Clinica Medica', 'Primera vez',  'Pide turno por dolor de cabeza'),
        ('hledesma', 4, '10:30', 'Solicitado', 'Clinica Medica', 'Control',      'Quiere seguimiento post estudio'),
        ('lgomez',   5, '11:00', 'Solicitado', 'Clinica Medica', 'Primera vez',  NULL),

        -- Historial
        ('cbenitez', -7, '09:00', 'Atendido',  'Clinica Medica', 'Control',      'Se le pidio hemograma'),
        ('dsosa',   -14, '10:00', 'Atendido',  'Clinica Medica', 'Primera vez',  'Diagnostico: hipertension leve'),
        ('mquiroga', -3, '11:30', 'Cancelado', 'Clinica Medica', 'Control',      'La paciente aviso que no podia')
    ) AS v(USUARIO, DIAS, HORA, ESTADO, ESPECIALIDAD, TIPO, OBS)
    JOIN USUARIOS U ON U.NOMBRE_USUARIOS = v.USUARIO;
END
GO

/* ----- Atenciones ----------------------------------------------------------
   Los turnos ya atendidos dejan su ficha, asi la historia clinica tiene algo.
   -------------------------------------------------------------------------- */

INSERT INTO ATENCION (ID_TURNO, ID_PERSONA_PROFESIONAL, ID_PERSONA_PACIENTE,
                      FECHA_ATENCION, HORA_ATENCION, MOTIVO_CONSULTA,
                      DIAGNOSTICO, TRATAMIENTO, ESTUDIO_SOLICITADO)
SELECT T.ID_TURNO, A.ID_PERSONA_PROFESIONAL, T.ID_PERSONA_PACIENTE,
       T.FECHA_TURNO, T.HORA_TURNO,
       'Consulta de control',
       CASE WHEN P.APELLIDO = 'Sosa' THEN 'Hipertension arterial leve'
            ELSE 'Sin hallazgos significativos' END,
       CASE WHEN P.APELLIDO = 'Sosa' THEN 'Dieta hiposodica y control en 30 dias'
            ELSE 'Control anual' END,
       CASE WHEN P.APELLIDO = 'Benitez' THEN 'Hemograma completo' END
FROM TURNOS T
JOIN AGENDA A  ON A.ID_AGENDA  = T.ID_AGENDA
JOIN PERSONA P ON P.ID_PERSONA = T.ID_PERSONA_PACIENTE
WHERE T.ESTADO = 'Atendido'
  AND NOT EXISTS (SELECT 1 FROM ATENCION X WHERE X.ID_TURNO = T.ID_TURNO);
GO

/* ----- Control -------------------------------------------------------------- */

SELECT 'turnos hoy (confirmados)' AS Indicador, COUNT(*) AS Valor
FROM Turno WHERE Estado = 'Confirmado' AND Fecha = CAST(GETDATE() AS DATE)
UNION ALL
SELECT 'proximos confirmados', COUNT(*)
FROM Turno WHERE Estado = 'Confirmado' AND Fecha > CAST(GETDATE() AS DATE)
UNION ALL
SELECT 'solicitudes pendientes', COUNT(*)
FROM Turno WHERE Estado = 'Solicitado'
UNION ALL
SELECT 'turnos totales', COUNT(*) FROM Turno
UNION ALL
SELECT 'pacientes', COUNT(*) FROM Usuario WHERE Rol = 'Paciente';
GO
