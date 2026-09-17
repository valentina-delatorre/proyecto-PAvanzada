/* =========================================================================
   02 - Carga minima para que la aplicacion arranque.
   Los nombres de los roles tienen que coincidir letra por letra con
   BIZ/Modelo/Roles.cs, porque Login.aspx.cs redirige comparando ese texto.
   ========================================================================= */

USE ConsultorioI;
GO

-- SSMS lo pone solo, sqlcmd no, y sin esto fallan el indice filtrado
-- del correo y la creacion de los triggers.
SET QUOTED_IDENTIFIER ON;
GO

INSERT INTO CATEGORIA (NOMBRE) VALUES ('Paciente'), ('Profesional'), ('Administrativo');

INSERT INTO ESPECIALIDAD (NOMBRE, DESCRIPCION) VALUES
    ('Clinica Medica', 'Consulta general y seguimiento'),
    ('Cardiologia',    'Corazon y sistema circulatorio'),
    ('Pediatria',      'Atencion de ninos y adolescentes'),
    ('Traumatologia',  'Huesos, articulaciones y lesiones');

INSERT INTO ROLES (NOMBRE_ROL) VALUES
    ('Seguridad'), ('Usuarios'), ('Especialistas'),
    ('Turnos'), ('Reportes'), ('Medico'), ('Paciente');

INSERT INTO PERMISOS (NOMBRE_PERMISO) VALUES
    ('ABM Usuarios'), ('ABM Especialistas'), ('Gestion de Turnos'),
    ('Ver Reportes'), ('Panel Medico'), ('Portal Paciente');

INSERT INTO URL (RUTA, ID_PERMISO)
SELECT v.RUTA, P.ID_PERMISO
FROM (VALUES
    ('~/Usuarios/ListadoUsuarios.aspx',           'ABM Usuarios'),
    ('~/Especialistas/ListadoEspecialistas.aspx', 'ABM Especialistas'),
    ('~/Turnos/ListadoTurnos.aspx',               'Gestion de Turnos'),
    ('~/Reportes/ReporteDiario.aspx',             'Ver Reportes'),
    ('~/Medicos/PanelMedico.aspx',                'Panel Medico'),
    ('~/Portal/PortalPaciente.aspx',              'Portal Paciente'),
    ('~/Portal/AgendarTurno.aspx',                'Portal Paciente')
) AS v(RUTA, PERMISO)
JOIN PERMISOS P ON P.NOMBRE_PERMISO = v.PERMISO;

-- Cada rol operativo tiene su permiso; Seguridad los tiene todos.
INSERT INTO ROL_PERMISO (ID_ROL, ID_PERMISO)
SELECT R.ID_ROL, P.ID_PERMISO
FROM ROLES R
JOIN PERMISOS P ON (R.NOMBRE_ROL = 'Seguridad')
                OR (R.NOMBRE_ROL = 'Usuarios'      AND P.NOMBRE_PERMISO = 'ABM Usuarios')
                OR (R.NOMBRE_ROL = 'Especialistas' AND P.NOMBRE_PERMISO = 'ABM Especialistas')
                OR (R.NOMBRE_ROL = 'Turnos'        AND P.NOMBRE_PERMISO = 'Gestion de Turnos')
                OR (R.NOMBRE_ROL = 'Reportes'      AND P.NOMBRE_PERMISO = 'Ver Reportes')
                OR (R.NOMBRE_ROL = 'Medico'        AND P.NOMBRE_PERMISO = 'Panel Medico')
                OR (R.NOMBRE_ROL = 'Paciente'      AND P.NOMBRE_PERMISO = 'Portal Paciente');
GO

/* ----- Una persona de cada tipo, con su usuario --------------------------
   Solo si la base arranca limpia. Si venimos de la base vieja, los usuarios
   reales los trae 04_migracion.sql y estos chocarian con ellos.
   -------------------------------------------------------------------------- */

IF OBJECT_ID('Usuario_old', 'U') IS NULL
BEGIN
    INSERT INTO PERSONA (NOMBRE, APELLIDO, TIPO_DNI, DNI, CORREO, TELEFONO, ID_CATEGORIA, ID_ESPECIALIDAD)
    SELECT 'Ana', 'Duarte', 'DNI', '30111222', 'ana.duarte@consultorio.com', '1155000001',
           (SELECT ID_CATEGORIA FROM CATEGORIA WHERE NOMBRE = 'Administrativo'), NULL
    UNION ALL
    SELECT 'Martin', 'Ferreyra', 'DNI', '28999888', 'm.ferreyra@consultorio.com', '1155000002',
           (SELECT ID_CATEGORIA FROM CATEGORIA WHERE NOMBRE = 'Profesional'),
           (SELECT ID_ESPECIALIDAD FROM ESPECIALIDAD WHERE NOMBRE = 'Clinica Medica')
    UNION ALL
    SELECT 'Lucia', 'Gomez', 'DNI', '42333444', 'lucia.gomez@mail.com', '1155000003',
           (SELECT ID_CATEGORIA FROM CATEGORIA WHERE NOMBRE = 'Paciente'), NULL;

    INSERT INTO USUARIOS (ID_PERSONA, NOMBRE_USUARIOS, CONTRASENIA, ID_ROL)
    SELECT P.ID_PERSONA, v.USUARIO, v.PASS, R.ID_ROL
    FROM (VALUES
        ('30111222', 'admin',     'admin123',    'Seguridad'),
        ('28999888', 'mferreyra', 'medico123',   'Medico'),
        ('42333444', 'lgomez',    'paciente123', 'Paciente')
    ) AS v(DNI, USUARIO, PASS, ROL)
    JOIN PERSONA P ON P.DNI = v.DNI
    JOIN ROLES R   ON R.NOMBRE_ROL = v.ROL;

    INSERT INTO MATRICULA (ID_PERSONA, NRO_MATRICULA, JURISDICCION)
    SELECT ID_PERSONA, 'MN 45210', 'Nacional' FROM PERSONA WHERE DNI = '28999888';

    -- Agenda semanal del profesional: las vistas la usan para saber de quien
    -- es cada turno.
    INSERT INTO AGENDA (ID_PERSONA_PROFESIONAL, DIA_ATENCION, HORA_INICIO, HORA_FIN)
    SELECT P.ID_PERSONA, d.DIA, '09:00', '13:00'
    FROM PERSONA P
    CROSS JOIN (VALUES ('Lunes'), ('Martes'), ('Miercoles'), ('Jueves'), ('Viernes')) AS d(DIA)
    WHERE P.DNI = '28999888';
END
GO
