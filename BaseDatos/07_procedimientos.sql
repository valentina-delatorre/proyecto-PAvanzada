/* =========================================================================
   07 - Procedimientos almacenados.
   SP_AGENDA_MES: arma en una tabla temporal el mes completo de un
   profesional (un renglon por dia, con cuantos turnos ocupados tiene y
   a que hora) para dibujar el almanaque del modulo de Turnos y el del
   paciente. Ocupa el horario tanto un turno confirmado como uno pedido,
   y tambien los pedidos "sin preferencia" de su especialidad, que todavia
   no tienen medico.
   Va despues del 03. Se puede correr las veces que haga falta.
   El servidor de la facultad es SQL Server 2008 R2: por eso no se usan
   CREATE OR ALTER, DATEFROMPARTS ni STRING_AGG.
   ========================================================================= */

USE ConsultorioI;
GO

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('SP_AGENDA_MES', 'P') IS NOT NULL DROP PROCEDURE SP_AGENDA_MES;
GO

CREATE PROCEDURE SP_AGENDA_MES
    @ID_PROFESIONAL INT,
    @ANIO           INT,
    @MES            INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Un renglon por cada dia del mes, arranca todo en cero
    CREATE TABLE #DIAS (
        FECHA    DATE         NOT NULL,
        DIA      INT          NOT NULL,
        TURNOS   INT          NOT NULL,
        HORARIOS VARCHAR(500) NOT NULL
    );

    DECLARE @f DATE = CAST(CAST(@ANIO AS VARCHAR(4)) + '-' + RIGHT('0' + CAST(@MES AS VARCHAR(2)), 2) + '-01' AS DATE);
    WHILE MONTH(@f) = @MES
    BEGIN
        INSERT INTO #DIAS (FECHA, DIA, TURNOS, HORARIOS) VALUES (@f, DAY(@f), 0, '');
        SET @f = DATEADD(DAY, 1, @f);
    END

    -- Los turnos que le ocupan un horario a este profesional:
    --   - los pedidos o confirmados con el (los rechazados liberan el horario)
    --   - los pedidos "sin preferencia" de su especialidad: todavia no tienen
    --     medico (ID_AGENDA nulo), pero ese horario ya lo pidio alguien
    DECLARE @ESPECIALIDAD VARCHAR(80) = (SELECT E.NOMBRE FROM PERSONA P
                                         JOIN ESPECIALIDAD E ON E.ID_ESPECIALIDAD = P.ID_ESPECIALIDAD
                                         WHERE P.ID_PERSONA = @ID_PROFESIONAL);

    SELECT T.FECHA_TURNO, T.HORA_TURNO
    INTO #OCUPADOS
    FROM TURNOS T
    LEFT JOIN AGENDA A     ON A.ID_AGENDA = T.ID_AGENDA
    LEFT JOIN TURNO_APP TA ON TA.ID_TURNO = T.ID_TURNO
    WHERE T.ESTADO IN ('Solicitado', 'Confirmado')
      AND (A.ID_PERSONA_PROFESIONAL = @ID_PROFESIONAL
           OR (T.ID_AGENDA IS NULL AND TA.ESPECIALIDAD_SOLICITADA = @ESPECIALIDAD));

    -- Se completan los dias que tienen horarios ocupados
    UPDATE D
    SET TURNOS   = X.CANTIDAD,
        HORARIOS = X.HORAS
    FROM #DIAS D
    JOIN (
        SELECT O.FECHA_TURNO,
               COUNT(*) AS CANTIDAD,
               -- las horas del dia unidas con ' - ' (FOR XML PATH es el STRING_AGG de 2008)
               STUFF((SELECT ' - ' + CONVERT(VARCHAR(5), O2.HORA_TURNO)
                      FROM #OCUPADOS O2
                      WHERE O2.FECHA_TURNO = O.FECHA_TURNO
                      ORDER BY O2.HORA_TURNO
                      FOR XML PATH('')), 1, 3, '') AS HORAS
        FROM #OCUPADOS O
        GROUP BY O.FECHA_TURNO
    ) X ON X.FECHA_TURNO = D.FECHA;

    SELECT FECHA, DIA, TURNOS, HORARIOS
    FROM #DIAS
    ORDER BY FECHA;

    DROP TABLE #DIAS;
    DROP TABLE #OCUPADOS;
END;
GO
