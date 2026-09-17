/* =========================================================================
   TP Programacion Avanzada - Consultorio2026
   01 - Esquema segun el DER (TP_BASES_DATOS_FINAL.drawio)

   Se ejecuta sobre la base ConsultorioI, una sola vez y antes que los demas.
   Orden de los scripts: 01 -> 02 -> (04 solo en la facultad) -> 03

   Lecturas del DER que tuvimos que asumir:
     - CATEGORIA es el tipo de persona: Paciente, Profesional, Administrativo.
     - PERSONA -> ESPECIALIDAD es N:1, asi que la FK va en PERSONA y es nula
       para todo el que no sea profesional.
     - ROLES <-> PERMISOS es N:N, va con la tabla intermedia ROL_PERMISO.
     - TURNOS.ID_AGENDA es nulo: el paciente puede pedir turno por especialidad
       sin tener todavia un profesional asignado.
     - ATENCION -> TURNOS es 1:1, la FK va en ATENCION.
     - ESTADO se implementa como BIT (1 activo / 0 inactivo).
     - La tabla del DER TURNO va como TURNOS, en plural. Es el unico nombre
       que cambiamos: la vista de compatibilidad del 03 tiene que llamarse
       Turno y SQL Server no distingue mayusculas de minusculas, asi que
       TURNO y Turno serian el mismo objeto y chocarian.
   ========================================================================= */

IF DB_ID('ConsultorioI') IS NULL
    CREATE DATABASE ConsultorioI;
GO

USE ConsultorioI;
GO

-- SSMS lo pone solo, sqlcmd no, y sin esto fallan el indice filtrado
-- del correo y la creacion de los triggers.
SET QUOTED_IDENTIFIER ON;
GO

/* Si la base ya tenia el esquema viejo, lo corremos a un costado antes de
   empezar, porque el 03 necesita esos nombres libres para las vistas.
   Los datos no se pierden: quedan en las tablas *_old y el 04 los migra. */
IF OBJECT_ID('Usuario', 'U')            IS NOT NULL EXEC sp_rename 'Usuario', 'Usuario_old';
IF OBJECT_ID('DatoPaciente', 'U')       IS NOT NULL EXEC sp_rename 'DatoPaciente', 'DatoPaciente_old';
IF OBJECT_ID('Medico', 'U')             IS NOT NULL EXEC sp_rename 'Medico', 'Medico_old';
IF OBJECT_ID('Turno', 'U')              IS NOT NULL EXEC sp_rename 'Turno', 'Turno_old';
IF OBJECT_ID('CodigoVerificacion', 'U') IS NOT NULL EXEC sp_rename 'CodigoVerificacion', 'CodigoVerificacion_old';
GO

CREATE TABLE CATEGORIA (
    ID_CATEGORIA INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE       VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE ESPECIALIDAD (
    ID_ESPECIALIDAD INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE          VARCHAR(80)  NOT NULL UNIQUE,
    DESCRIPCION     VARCHAR(250) NULL
);

CREATE TABLE PERSONA (
    ID_PERSONA       INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE           VARCHAR(80)  NOT NULL,
    APELLIDO         VARCHAR(80)  NOT NULL,
    TIPO_DNI         VARCHAR(10)  NULL,
    DNI              VARCHAR(15)  NULL,
    FECHA_NACIMIENTO DATE         NULL,
    CORREO           VARCHAR(120) NULL,
    TELEFONO         VARCHAR(30)  NULL,
    DOMICILIO        VARCHAR(150) NULL,
    ID_CATEGORIA     INT NOT NULL REFERENCES CATEGORIA(ID_CATEGORIA),
    ID_ESPECIALIDAD  INT NULL     REFERENCES ESPECIALIDAD(ID_ESPECIALIDAD)
);

-- El correo es con lo que se inicia sesion, asi que no se puede repetir.
-- Va filtrado porque una persona sin usuario puede no tener correo cargado.
CREATE UNIQUE INDEX UX_PERSONA_CORREO ON PERSONA(CORREO) WHERE CORREO IS NOT NULL;

CREATE TABLE MATRICULA (
    ID_MATRICULA  INT IDENTITY(1,1) PRIMARY KEY,
    ID_PERSONA    INT NOT NULL REFERENCES PERSONA(ID_PERSONA),
    NRO_MATRICULA VARCHAR(20) NOT NULL,
    JURISDICCION  VARCHAR(60) NULL,
    ESTADO        BIT NOT NULL DEFAULT 1
);

CREATE TABLE ROLES (
    ID_ROL     INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE_ROL VARCHAR(50) NOT NULL UNIQUE,
    ESTADO     BIT NOT NULL DEFAULT 1
);

CREATE TABLE USUARIOS (
    ID_USUARIOS     INT IDENTITY(1,1) PRIMARY KEY,
    ID_PERSONA      INT NOT NULL UNIQUE REFERENCES PERSONA(ID_PERSONA),
    NOMBRE_USUARIOS VARCHAR(50)  NOT NULL UNIQUE,
    CONTRASENIA     VARCHAR(100) NOT NULL,
    ESTADO          BIT NOT NULL DEFAULT 1,
    ID_ROL          INT NOT NULL REFERENCES ROLES(ID_ROL)
);

CREATE TABLE PERMISOS (
    ID_PERMISO     INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE_PERMISO VARCHAR(60) NOT NULL UNIQUE,
    ESTADO         BIT NOT NULL DEFAULT 1
);

CREATE TABLE ROL_PERMISO (
    ID_ROL     INT NOT NULL REFERENCES ROLES(ID_ROL),
    ID_PERMISO INT NOT NULL REFERENCES PERMISOS(ID_PERMISO),
    PRIMARY KEY (ID_ROL, ID_PERMISO)
);

CREATE TABLE URL (
    ID_URL     INT IDENTITY(1,1) PRIMARY KEY,
    RUTA       VARCHAR(150) NOT NULL,
    ESTADO     BIT NOT NULL DEFAULT 1,
    ID_PERMISO INT NOT NULL REFERENCES PERMISOS(ID_PERMISO)
);

CREATE TABLE LOGS (
    ID_LOG      INT IDENTITY(1,1) PRIMARY KEY,
    FECHA_HORA  DATETIME NOT NULL DEFAULT GETDATE(),
    ACCION      VARCHAR(250) NOT NULL,
    ID_USUARIO  INT NULL REFERENCES USUARIOS(ID_USUARIOS)
);

CREATE TABLE AGENDA (
    ID_AGENDA              INT IDENTITY(1,1) PRIMARY KEY,
    ID_PERSONA_PROFESIONAL INT NOT NULL REFERENCES PERSONA(ID_PERSONA),
    DIA_ATENCION           VARCHAR(15) NOT NULL,   -- Lunes..Sabado
    HORA_INICIO            TIME NOT NULL,
    HORA_FIN               TIME NOT NULL,
    ESTADO                 BIT NOT NULL DEFAULT 1,
    OBSERVACIONES          VARCHAR(250) NULL
);

CREATE TABLE TURNOS (
    ID_TURNO            INT IDENTITY(1,1) PRIMARY KEY,
    ID_AGENDA           INT NULL REFERENCES AGENDA(ID_AGENDA),
    ID_PERSONA_PACIENTE INT NOT NULL REFERENCES PERSONA(ID_PERSONA),
    FECHA_TURNO         DATE NOT NULL,
    HORA_TURNO          TIME NOT NULL,
    ESTADO              VARCHAR(20) NOT NULL DEFAULT 'Solicitado',
    OBSERVACIONES       VARCHAR(250) NULL
);

CREATE TABLE ATENCION (
    ID_ATENCION            INT IDENTITY(1,1) PRIMARY KEY,
    ID_TURNO               INT NULL UNIQUE REFERENCES TURNOS(ID_TURNO),
    ID_PERSONA_PROFESIONAL INT NOT NULL REFERENCES PERSONA(ID_PERSONA),
    ID_PERSONA_PACIENTE    INT NOT NULL REFERENCES PERSONA(ID_PERSONA),
    FECHA_ATENCION         DATE NOT NULL,
    HORA_ATENCION          TIME NULL,
    MOTIVO_CONSULTA        VARCHAR(250) NULL,
    OBSERVACIONES          VARCHAR(250) NULL,
    HISTORIA_CLINICA       VARCHAR(MAX) NULL,
    DIAGNOSTICO            VARCHAR(250) NULL,
    TRATAMIENTO            VARCHAR(250) NULL,
    ESTUDIO_SOLICITADO     VARCHAR(250) NULL
);
GO

/* ----- Tablas de aplicacion -------------------------------------------------
   No estan en el DER. Guardan lo que la pantalla de turnos necesita y el
   modelo del profesor no contempla, para no tener que ensuciar TURNOS.
   -------------------------------------------------------------------------- */

CREATE TABLE TURNO_APP (
    ID_TURNO               INT PRIMARY KEY REFERENCES TURNOS(ID_TURNO),
    ESPECIALIDAD_SOLICITADA VARCHAR(80) NULL,
    TIPO_CONSULTA          VARCHAR(50) NULL
);

CREATE TABLE CodigoVerificacion (
    IdCodigo    INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario   INT NOT NULL REFERENCES USUARIOS(ID_USUARIOS),
    Codigo      VARCHAR(10) NOT NULL,
    FechaExpira DATETIME NOT NULL,
    Usado       BIT NOT NULL DEFAULT 0
);
GO
