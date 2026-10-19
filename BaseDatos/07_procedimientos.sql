/* =========================================================================
   07 - Procedimientos almacenados.
   SP_AGENDA_MES: arma en una tabla temporal el mes completo de un
   profesional (un renglon por dia, con cuantos turnos confirmados tiene y
   a que hora) para dibujar el almanaque del modulo de Turnos.
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

    -- Se completan los dias que tienen turnos confirmados con ese profesional
    UPDATE D
    SET TURNOS   = X.CANTIDAD,
        HORARIOS = X.HORAS
    FROM #DIAS D
    JOIN (
        SELECT T.FECHA_TURNO,
               COUNT(*) AS CANTIDAD,
               -- las horas del dia unidas con ' - ' (FOR XML PATH es el STRING_AGG de 2008)
               STUFF((SELECT ' - ' + CONVERT(VARCHAR(5), T2.HORA_TURNO)
                      FROM TURNOS T2
                      JOIN AGENDA A2 ON A2.ID_AGENDA = T2.ID_AGENDA
                      WHERE A2.ID_PERSONA_PROFESIONAL = @ID_PROFESIONAL
                        AND T2.ESTADO = 'Confirmado'
                        AND T2.FECHA_TURNO = T.FECHA_TURNO
                      ORDER BY T2.HORA_TURNO
                      FOR XML PATH('')), 1, 3, '') AS HORAS
        FROM TURNOS T
        JOIN AGENDA A ON A.ID_AGENDA = T.ID_AGENDA
        WHERE A.ID_PERSONA_PROFESIONAL = @ID_PROFESIONAL
          AND T.ESTADO = 'Confirmado'
        GROUP BY T.FECHA_TURNO
    ) X ON X.FECHA_TURNO = D.FECHA;

    SELECT FECHA, DIA, TURNOS, HORARIOS
    FROM #DIAS
    ORDER BY FECHA;

    DROP TABLE #DIAS;
END;
GO
