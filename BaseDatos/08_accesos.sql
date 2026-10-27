/* =========================================================================
   08 - Accesos por usuario y asistente con IA.

   Hasta aca cada usuario entraba a lo que le daba su ROL (ROL_PERMISO).
   Ahora Seguridad puede habilitar o deshabilitar un modulo a UNA persona
   puntual sin tocar su rol. Eso se guarda en USUARIO_PERMISO: solo hay
   renglon cuando el usuario es distinto a lo que dice su rol.

   Acceso real = lo que diga USUARIO_PERMISO, y si no dice nada, lo del rol.
   La vista VW_ACCESOS ya devuelve esa cuenta hecha.

   Va despues del 07. Se puede correr las veces que haga falta.
   ========================================================================= */

USE ConsultorioI;
GO

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('USUARIO_PERMISO', 'U') IS NULL
CREATE TABLE USUARIO_PERMISO (
    ID_USUARIO INT NOT NULL REFERENCES USUARIOS(ID_USUARIOS),
    ID_PERMISO INT NOT NULL REFERENCES PERMISOS(ID_PERMISO),
    HABILITADO BIT NOT NULL,
    PRIMARY KEY (ID_USUARIO, ID_PERMISO)
);
GO

/* ----- Los dos modulos nuevos ----------------------------------------------- */

INSERT INTO PERMISOS (NOMBRE_PERMISO)
SELECT v.NOMBRE FROM (VALUES ('Seguridad y Accesos'), ('Asistente IA')) AS v(NOMBRE)
WHERE NOT EXISTS (SELECT 1 FROM PERMISOS P WHERE P.NOMBRE_PERMISO = v.NOMBRE);

INSERT INTO URL (RUTA, ID_PERMISO)
SELECT v.RUTA, P.ID_PERMISO
FROM (VALUES
    ('~/Seguridad/Accesos.aspx',     'Seguridad y Accesos'),
    ('~/Asistente/Consultar.ashx',   'Asistente IA')
) AS v(RUTA, PERMISO)
JOIN PERMISOS P ON P.NOMBRE_PERMISO = v.PERMISO
WHERE NOT EXISTS (SELECT 1 FROM URL U WHERE U.RUTA = v.RUTA);

-- El asistente lo tienen todos los roles; despues se le puede sacar a alguien.
INSERT INTO ROL_PERMISO (ID_ROL, ID_PERMISO)
SELECT R.ID_ROL, P.ID_PERMISO
FROM ROLES R
JOIN PERMISOS P ON P.NOMBRE_PERMISO = 'Asistente IA'
WHERE NOT EXISTS (SELECT 1 FROM ROL_PERMISO X WHERE X.ID_ROL = R.ID_ROL AND X.ID_PERMISO = P.ID_PERMISO);

-- Turnos ya entraba a Especialistas (lo decia el Web.config de la carpeta):
-- ahora que los accesos salen de la base, lo dejamos escrito aca.
INSERT INTO ROL_PERMISO (ID_ROL, ID_PERMISO)
SELECT R.ID_ROL, P.ID_PERMISO
FROM ROLES R
JOIN PERMISOS P ON P.NOMBRE_PERMISO = 'ABM Especialistas'
WHERE R.NOMBRE_ROL = 'Turnos'
  AND NOT EXISTS (SELECT 1 FROM ROL_PERMISO X WHERE X.ID_ROL = R.ID_ROL AND X.ID_PERMISO = P.ID_PERMISO);

-- "Seguridad y Accesos" NO va por rol: es solo de Valentina.
INSERT INTO USUARIO_PERMISO (ID_USUARIO, ID_PERMISO, HABILITADO)
SELECT U.ID_USUARIOS, P.ID_PERMISO, 1
FROM USUARIOS U
JOIN PERSONA  PE ON PE.ID_PERSONA = U.ID_PERSONA AND PE.CORREO = 'valentina.d@usal.edu.ar'
JOIN PERMISOS P  ON P.NOMBRE_PERMISO = 'Seguridad y Accesos'
WHERE NOT EXISTS (SELECT 1 FROM USUARIO_PERMISO X WHERE X.ID_USUARIO = U.ID_USUARIOS AND X.ID_PERMISO = P.ID_PERMISO);
GO

/* ----- Acceso real de cada usuario a cada modulo ---------------------------- */

IF OBJECT_ID('VW_ACCESOS', 'V') IS NOT NULL DROP VIEW VW_ACCESOS;
GO

CREATE VIEW VW_ACCESOS AS
SELECT U.ID_USUARIOS AS ID_USUARIO,
       P.ID_PERMISO,
       P.NOMBRE_PERMISO,
       CAST(CASE WHEN RP.ID_PERMISO IS NULL THEN 0 ELSE 1 END AS BIT) AS POR_ROL,
       CAST(ISNULL(UP.HABILITADO, CASE WHEN RP.ID_PERMISO IS NULL THEN 0 ELSE 1 END) AS BIT) AS HABILITADO
FROM USUARIOS U
CROSS JOIN PERMISOS P
LEFT JOIN ROL_PERMISO     RP ON RP.ID_ROL     = U.ID_ROL      AND RP.ID_PERMISO = P.ID_PERMISO
LEFT JOIN USUARIO_PERMISO UP ON UP.ID_USUARIO = U.ID_USUARIOS AND UP.ID_PERMISO = P.ID_PERMISO
WHERE P.ESTADO = 1;
GO
